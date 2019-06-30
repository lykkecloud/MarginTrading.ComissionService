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

        public CommissionCalcService(
            ICfdCalculatorService cfdCalculatorService,
            IAssetsCache assetsCache,
            IRateSettingsService rateSettingsService,
            IOrderEventsApi orderEventsApi,
            IAccountRedisCache accountRedisCache,
            ILog log)
        {
            _cfdCalculatorService = cfdCalculatorService;
            _assetsCache = assetsCache;
            _rateSettingsService = rateSettingsService;
            _orderEventsApi = orderEventsApi;
            _accountRedisCache = accountRedisCache;
            _log = log;
        }

        /// <summary>
        /// Value must be charged as it is, without negation
        /// </summary>
        public async Task<(decimal Swap, string Details)> GetOvernightSwap(Dictionary<string, decimal> interestRates,
            IOpenPosition openPosition, IAssetPair assetPair, int numberOfFinancingDays, int financingDaysPerYear)
        {
            var rateSettings = await _rateSettingsService.GetOvernightSwapRate(assetPair.Id);
            var account = await _accountRedisCache.GetAccount(openPosition.AccountId);

            var calculationBasis = _cfdCalculatorService.GetFxRateForAssetPair(account.BaseAssetId,
                                       openPosition.AssetPairId, assetPair.LegalEntity)
                                   * Math.Abs(openPosition.CurrentVolume) * openPosition.ClosePrice;

            interestRates.TryGetValue(rateSettings.VariableRateBase ?? string.Empty, out var variableRateBase);
            interestRates.TryGetValue(rateSettings.VariableRateQuote ?? string.Empty, out var variableRateQuote);
            
            var financingRate = - rateSettings.FixRate
                - (openPosition.Direction == PositionDirection.Short ? rateSettings.RepoSurchargePercent : 0)
                + (variableRateBase - variableRateQuote)
                              * (openPosition.Direction == PositionDirection.Long ? 1 : -1);

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
            string legalEntity, decimal volume, decimal orderExecutionPrice)
        {
            var rateSettings = await _rateSettingsService.GetOrderExecutionRate(accountId, instrument)
                ?? await _rateSettingsService.GetOrderExecutionRate(RateSettingsService.TradingProfile, instrument);
            var account = await _accountRedisCache.GetAccount(accountId);

            var volumeInCommissionAsset = _cfdCalculatorService.GetFxRateForAssetPair(rateSettings.CommissionAsset,
                                              instrument, legalEntity)
                                          * Math.Abs(volume) * orderExecutionPrice;
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
            string accountAssetId)
        {
            var events = await ApiHelpers
                .RefitRetryPolicy<List<OrderEventContract>>(
                    r => r.Any(oec =>
                        new[] { OrderUpdateTypeContract.Executed, OrderUpdateTypeContract.Reject, OrderUpdateTypeContract.Cancel }
                            .Contains(oec.UpdateType)),
                    3, 1000)
                .ExecuteAsync(async ct =>
                    await _orderEventsApi.OrderById(orderId, null, false), CancellationToken.None);

            if (events.All(e => e.UpdateType != OrderUpdateTypeContract.Executed))
            {
                if (events.Any(e => e.UpdateType == OrderUpdateTypeContract.Reject ||
                                    e.UpdateType == OrderUpdateTypeContract.Cancel))
                {
                    _log.WriteWarning(nameof(CalculateOnBehalfCommissionAsync), events.ToJson(),
                        $"Order {orderId} for instrument {accountAssetId} was not executed and on-behalf will not be charged");
                    return (0, 0);
                }

                throw new Exception(
                    $"Order {orderId} for instrument {accountAssetId} was not executed or rejected/cancelled. On-behalf can not be calculated");
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

            var rateSettings = await _rateSettingsService.GetOnBehalfRate(onBehalfEvents.First().AccountId)
                ?? await _rateSettingsService.GetOnBehalfRate(RateSettingsService.TradingProfile); 
            
            //use fx rates to convert to account asset
            var quote = _cfdCalculatorService.GetFxRate(rateSettings.CommissionAsset, accountAssetId, 
                rateSettings.LegalEntity);

            var commission = Math.Round(actionsNum * rateSettings.Commission * quote, 
                _assetsCache.GetAccuracy(accountAssetId));
            
            //calculate commission
            return (actionsNum, commission);

            async Task<bool> CorrelatesWithParent(OrderEventContract order) =>
                (await _orderEventsApi.OrderById(order.ParentOrderId, OrderStatusContract.Placed, false))
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