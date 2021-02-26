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
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.Services
{
    public class CommissionCalcService : ICommissionCalcService
    {
        private readonly ICfdCalculatorService _cfdCalculatorService;
        private readonly IAssetPairsCache _assetPairsCache;
        private readonly IRateSettingsCache _rateSettingsCache;
        private readonly IOrderEventsApi _orderEventsApi;
        private readonly IAccountRedisCache _accountRedisCache;
        private readonly IProductsCache _productsCache;
        private readonly ILog _log;
        private readonly IInterestRatesCacheService _interestRatesCacheService;
        private readonly CommissionServiceSettings _settings;
        private readonly OrderExecutionSettings _defaultOrderExecutionRateSettings;

        public CommissionCalcService(
            ICfdCalculatorService cfdCalculatorService,
            IAssetPairsCache assetPairsCache,
            IRateSettingsCache rateSettingsCache,
            IOrderEventsApi orderEventsApi,
            IAccountRedisCache accountRedisCache,
            IProductsCache productsCache,
            ILog log,
            IInterestRatesCacheService interestRatesCacheService,
            CommissionServiceSettings settings,
            OrderExecutionSettings defaultOrderExecutionRateSettings)
        {
            _cfdCalculatorService = cfdCalculatorService;
            _assetPairsCache = assetPairsCache;
            _rateSettingsCache = rateSettingsCache;
            _orderEventsApi = orderEventsApi;
            _accountRedisCache = accountRedisCache;
            _productsCache = productsCache;
            _log = log;
            _interestRatesCacheService = interestRatesCacheService;
            _settings = settings;
            _defaultOrderExecutionRateSettings = defaultOrderExecutionRateSettings;
        }

        /// <summary>
        /// Value must be charged as it is, without negation
        /// </summary>
        public async Task<(decimal Swap, string Details)> GetOvernightSwap(string accountId, string instrument,
            decimal volume, decimal closePrice, decimal fxRate, PositionDirection direction,
            int numberOfFinancingDays, int financingDaysPerYear)
        {
            var account = await _accountRedisCache.GetAccount(accountId);
            var rateSettings = await _rateSettingsCache.GetOvernightSwapRate(instrument, account.TradingConditionId);

            var calculationBasis = fxRate * Math.Abs(volume) * closePrice;

            var financingRate = -rateSettings.FixRate
                                - (direction == PositionDirection.Short
                                    ? rateSettings.RepoSurchargePercent
                                    : 0);
            
            var assetPair = _assetPairsCache.GetAssetPairById(instrument);

            if (!_settings.AssetTypesWithZeroInterestRates.Contains(assetPair.AssetType))
            {
                var variableRateBase = _interestRatesCacheService.GetRate(rateSettings.VariableRateBase);
                var variableRateQuote = _interestRatesCacheService.GetRate(rateSettings.VariableRateQuote);

                financingRate += (variableRateBase - variableRateQuote)
                                 * (direction == PositionDirection.Long ? 1 : -1);
            }
            
            var dayFactor = (decimal) numberOfFinancingDays / financingDaysPerYear;

            return (Math.Round(calculationBasis * financingRate * dayFactor,
                        _productsCache.GetAccuracy(account.BaseAssetId)),
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
            var account = await _accountRedisCache.GetAccount(accountId);
            var clientProfileSettings = _assetsCache.GetClientProfile(instrument, account.TradingConditionId);
            var volumeInSettlementCurrency = orderExecutionFxRate * Math.Abs(volume) * orderExecutionPrice;

            var commission = Math.Min(
                clientProfileSettings?.ExecutionFeesCap ?? _defaultOrderExecutionRateSettings.ExecutionFeesCap,
                Math.Max(
                    clientProfileSettings?.ExecutionFeesFloor ?? _defaultOrderExecutionRateSettings.ExecutionFeesFloor,
                    (clientProfileSettings?.ExecutionFeesRate ?? _defaultOrderExecutionRateSettings.ExecutionFeesRate) * volumeInSettlementCurrency));

            return Math.Round(commission, _productsCache.GetAccuracy(account.BaseAssetId));
        }

        public async Task<(int ActionsNum, decimal Commission)> CalculateOnBehalfCommissionAsync(string orderId,
            string accountAssetId, string assetPairId, string accountId)
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

            var assetPair = _assetPairsCache.GetAssetPairById(assetPairId);
            
            var rateSettings = await _rateSettingsCache.GetOnBehalfRate(accountId, assetPair.AssetType); 
            
            var commission = Math.Round(actionsNum * rateSettings.Commission, 
                _productsCache.GetAccuracy(accountAssetId));
            
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