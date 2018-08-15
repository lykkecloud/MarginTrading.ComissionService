using System;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings.Rates;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.Services
{
    public class CommissionCalcService : ICommissionCalcService
    {
        private readonly ICfdCalculatorService _cfdCalculatorService;
        private readonly DefaultRateSettings _defaultRateSettings;
        private readonly IOrderEventsApi _orderEventsApi;

        public CommissionCalcService(
            ICfdCalculatorService cfdCalculatorService,
            DefaultRateSettings defaultRateSettings,
            IOrderEventsApi orderEventsApi)
        {
            _cfdCalculatorService = cfdCalculatorService;
            _defaultRateSettings = defaultRateSettings;
            _orderEventsApi = orderEventsApi;
        }

        private decimal CalculateSwaps(string accountAssetId, string instrument, DateTime? openDate, DateTime? closeDate,
            decimal volume, decimal swapRate, string legalEntity)
        {
            decimal result = 0;

//            if (openDate.HasValue)
//            {
//                var close = closeDate ?? DateTime.UtcNow;
//                var seconds = (decimal) (close - openDate.Value).TotalSeconds;
//
//                const int secondsInYear = 31536000;
//                var quote = _cfdCalculatorService.GetQuoteRateForBaseAsset(accountAssetId, instrument, legalEntity, 
//                    volume * swapRate > 0);
//                var swaps = quote * volume * swapRate * seconds / secondsInYear;
//                result = Math.Round(swaps, _assetsCache.GetAssetAccuracy(accountAssetId));
//            }

            return result;
        }

        /// <summary>
        /// Value must be charged as it is, without negation
        /// </summary>
        public decimal GetOvernightSwap(IOpenPosition openPosition, IAssetPair assetPair,
            int numberOfFinancingDays, int financingDaysPerYear)
        {
            var defaultSettings = _defaultRateSettings.DefaultOvernightSwapSettings;
            var volumeInAsset = _cfdCalculatorService.GetQuoteRateForQuoteAsset(defaultSettings.CommissionAsset,
                                    openPosition.AssetPairId, assetPair.LegalEntity)
                                * Math.Abs(openPosition.CurrentVolume);
            var basisOfCalc = - defaultSettings.FixRate
                - (openPosition.Direction == PositionDirection.Short ? defaultSettings.RepoSurchargePercent : 0)
                + (defaultSettings.VariableRateBase - defaultSettings.VariableRateQuote)
                              * (openPosition.Direction == PositionDirection.Long ? 1 : -1);

            var dayFactor = (decimal) numberOfFinancingDays / financingDaysPerYear;
            
            return volumeInAsset * basisOfCalc * dayFactor;
        }

        public decimal CalculateOrderExecutionCommission(string instrument, string legalEntity, decimal volume)
        {
            var defaultSettings = _defaultRateSettings.DefaultOrderExecutionSettings;

            var volumeInAsset = _cfdCalculatorService.GetQuoteRateForQuoteAsset(defaultSettings.CommissionAsset,
                                    instrument, legalEntity)
                                * Math.Abs(volume);
            
            var commission = Math.Min(
                defaultSettings.CommissionCap, 
                Math.Max(
                    defaultSettings.CommissionFloor,
                    defaultSettings.CommissionRate * volumeInAsset));

            return commission;
        }

        public async Task<(int ActionsNum, decimal Commission)> CalculateOnBehalfCommissionAsync(string orderId,
            string accountAssetId)
        {
            var onBehalfEvents = (await _orderEventsApi.OrderById(orderId, null, false))
                .Where(o => o.Originator == OriginatorTypeContract.OnBehalf).ToList();

            var changeEventsCount = onBehalfEvents.Count(o => o.UpdateType == OrderUpdateTypeContract.Change);

            var placeEventCharged = !onBehalfEvents.Exists(o => o.UpdateType == OrderUpdateTypeContract.Place)
                                    || onBehalfEvents.Exists(o => o.UpdateType == OrderUpdateTypeContract.Place
                                                                  && !string.IsNullOrWhiteSpace(o.ParentOrderId)
                                                                  && CorrelatesWithParent(o).Result)
                ? 0
                : 1;

            var actionsNum = changeEventsCount + placeEventCharged;
            
            //use fx rates to convert to account asset
            var quote = _cfdCalculatorService.GetQuote(_defaultRateSettings.DefaultOnBehalfSettings.CommissionAsset, 
                accountAssetId, _defaultRateSettings.DefaultOnBehalfSettings.DefaultLegalEntity);
            
            //calculate commission
            return (actionsNum, actionsNum * _defaultRateSettings.DefaultOnBehalfSettings.Commission * quote);

            async Task<bool> CorrelatesWithParent(OrderEventContract order) =>
                (await _orderEventsApi.OrderById(order.ParentOrderId, OrderStatusContract.Placed, false))
                .Any(p => p.CorrelationId == order.CorrelationId);
        }
    }
}