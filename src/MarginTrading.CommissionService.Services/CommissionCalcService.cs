// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.Services
{
    public class CommissionCalcService : ICommissionCalcService
    {
        private readonly ICfdCalculatorService _cfdCalculatorService;
        private readonly IAssetsCache _assetsCache;
        private readonly IRateSettingsService _rateSettingsService;
        private readonly IOrderEventsApi _orderEventsApi;
        private readonly IAccountRedisCache _accountRedisCache;
        private readonly ILog _log;
        private readonly IInterestRatesCacheService _interestRatesCacheService;

        public CommissionCalcService(
            ICfdCalculatorService cfdCalculatorService,
            IAssetsCache assetsCache,
            IRateSettingsService rateSettingsService,
            IOrderEventsApi orderEventsApi,
            IAccountRedisCache accountRedisCache,
            ILog log,
            IInterestRatesCacheService interestRatesCacheService)
        {
            _cfdCalculatorService = cfdCalculatorService;
            _assetsCache = assetsCache;
            _rateSettingsService = rateSettingsService;
            _orderEventsApi = orderEventsApi;
            _accountRedisCache = accountRedisCache;
            _log = log;
            _interestRatesCacheService = interestRatesCacheService;
        }

        /// <summary>
        /// Value must be charged as it is, without negation
        /// </summary>
        public async Task<(decimal Swap, string Details)> GetOvernightSwap(string accountId, string instrument,
            decimal volume, decimal closePrice, PositionDirection direction, int numberOfFinancingDays,
            int financingDaysPerYear)
        {
            var rateSettings = await _rateSettingsService.GetOvernightSwapRate(instrument);
            var account = await _accountRedisCache.GetAccount(accountId);

            var calculationBasis = _cfdCalculatorService.GetFxRateForAssetPair(account.BaseAssetId,
                                       instrument, account.LegalEntity)
                                   * Math.Abs(volume) * closePrice;

            var variableRateBase = _interestRatesCacheService.GetRate(rateSettings.VariableRateBase);
            var variableRateQuote = _interestRatesCacheService.GetRate(rateSettings.VariableRateQuote);

            var financingRate = -rateSettings.FixRate
                                - (direction == PositionDirection.Short
                                    ? rateSettings.RepoSurchargePercent
                                    : 0)
                                + (variableRateBase - variableRateQuote)
                                * (direction == PositionDirection.Long ? 1 : -1);

            var dayFactor = (decimal) numberOfFinancingDays / financingDaysPerYear;

            return (Math.Round(calculationBasis * financingRate * dayFactor,
                        _assetsCache.GetAccuracy(account.BaseAssetId)),
                    new
                    {
                        CalculationBasis = calculationBasis,
                        FinancingRate = financingRate,
                        DayFactor = dayFactor,
                        FixRate = rateSettings.FixRate
                    }.ToJson()
                );
        }

        public async Task<decimal> CalculateOrderExecutionCommission(string accountId, string instrument,
            decimal volume, decimal orderExecutionPrice, decimal orderExecutionFxRate)
        {
            var rateSettings = await _rateSettingsService.GetOrderExecutionRate(instrument);
            var account = await _accountRedisCache.GetAccount(accountId);

            var fxRate = account.BaseAssetId != rateSettings.CommissionAsset
                ? _cfdCalculatorService.GetFxRateForAssetPair(rateSettings.CommissionAsset, instrument,
                    account.LegalEntity)
                : orderExecutionFxRate;
            
            var volumeInCommissionAsset = fxRate * Math.Abs(volume) * orderExecutionPrice;
            var commissionToAccountRate = _cfdCalculatorService.GetFxRate(rateSettings.CommissionAsset, 
                account.BaseAssetId, rateSettings.LegalEntity);
            
            var commission = Math.Min(
                rateSettings.CommissionCap, 
                Math.Max(
                    rateSettings.CommissionFloor,
                    rateSettings.CommissionRate * volumeInCommissionAsset))
                * commissionToAccountRate;

            return Math.Round(commission, _assetsCache.GetAccuracy(account.BaseAssetId));
        }

        public async Task<(int ActionsNum, decimal Commission)> CalculateOnBehalfCommissionAsync(string orderId,
            string accountAssetId, string assetPairId)
        {
            var events = await ApiHelpers
                .RefitRetryPolicy<List<OrderEventWithAdditionalContract>>(
                    r => r.Any(oec =>
                        new[] { OrderUpdateTypeContract.Executed, OrderUpdateTypeContract.Reject, OrderUpdateTypeContract.Cancel }
                            .Contains(oec.UpdateType)),
                    3, 1000)
                .ExecuteAsync(async ct =>
                    await _orderEventsApi.OrderById(orderId), CancellationToken.None);

            if (events.All(e => e.UpdateType != OrderUpdateTypeContract.Executed))
            {
                if (events.Any(e => e.UpdateType == OrderUpdateTypeContract.Reject ||
                                    e.UpdateType == OrderUpdateTypeContract.Cancel))
                {
                    _log.WriteWarning(nameof(CalculateOnBehalfCommissionAsync), events.ToJson(),
                        $"Order {orderId} for instrument {assetPairId} was not executed and on-behalf will not be charged");
                    return (0, 0);
                }

                throw new Exception(
                    $"Order {orderId} for instrument {assetPairId} was not executed or rejected/cancelled. On-behalf can not be calculated");
            }

            var onBehalfEvents = events.Where(CheckOnBehalfFlag).ToList();

            var changeEventsCount = onBehalfEvents.Count(o => o.UpdateType == OrderUpdateTypeContract.Change);

            var placeEventCharged = !onBehalfEvents.Exists(o => o.UpdateType == OrderUpdateTypeContract.Place)
                                    || onBehalfEvents.Exists(o => o.UpdateType == OrderUpdateTypeContract.Place
                                                                  && !string.IsNullOrWhiteSpace(o.ParentOrderId)
                                                                  && CorrelatesWithParent(o).Result)
                ? 0
                : 1;

            var actionsNum = changeEventsCount + placeEventCharged;

            var rateSettings = await _rateSettingsService.GetOnBehalfRate(); 
            
            //use fx rates to convert to account asset
            var quote = _cfdCalculatorService.GetFxRate(rateSettings.CommissionAsset, accountAssetId, 
                rateSettings.LegalEntity);

            var commission = Math.Round(actionsNum * rateSettings.Commission * quote, 
                _assetsCache.GetAccuracy(accountAssetId));
            
            //calculate commission
            return (actionsNum, commission);

            async Task<bool> CorrelatesWithParent(OrderEventContract order) =>
                (await _orderEventsApi.OrderById(order.ParentOrderId, OrderStatusContract.Placed))
                .Any(p => p.CorrelationId == order.CorrelationId);
        }

        private bool CheckOnBehalfFlag(OrderEventContract orderEvent)
        {
            //used to be o => o.Originator == OriginatorTypeContract.OnBehalf
            try
            {
                return JsonConvert.DeserializeAnonymousType(orderEvent.AdditionalInfo, new {WithOnBehalfFees = false})
                    .WithOnBehalfFees;
            }
            catch
            {
                return false;
            }
        }
    }
}