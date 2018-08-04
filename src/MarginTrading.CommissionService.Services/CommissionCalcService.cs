using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
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
        private readonly IRateSettingsService _rateSettingsService;
        private readonly IOrderEventsApi _orderEventsApi;
        private readonly ILog _log;

        public CommissionCalcService(
            ICfdCalculatorService cfdCalculatorService,
            IRateSettingsService rateSettingsService,
            IOrderEventsApi orderEventsApi,
            ILog log)
        {
            _cfdCalculatorService = cfdCalculatorService;
            _rateSettingsService = rateSettingsService;
            _orderEventsApi = orderEventsApi;
            _log = log;
        }

        /// <summary>
        /// Value must be charged as it is, without negation
        /// </summary>
        /// <param name="openPosition"></param>
        /// <param name="assetPair"></param>
        /// <returns></returns>
        public async Task<decimal> GetOvernightSwap(IOpenPosition openPosition, IAssetPair assetPair)
        {
            var rateSettings = await _rateSettingsService.GetOvernightSwapRate(assetPair.Id);
            var volumeInAsset = _cfdCalculatorService.GetQuoteRateForQuoteAsset(rateSettings.CommissionAsset,
                                    openPosition.AssetPairId, assetPair.LegalEntity)
                                * Math.Abs(openPosition.CurrentVolume);
            var basisOfCalc = - rateSettings.FixRate
                - (openPosition.Direction == PositionDirection.Short ? rateSettings.RepoSurchargePercent : 0)
                + (rateSettings.VariableRateBase - rateSettings.VariableRateQuote)
                              * (openPosition.Direction == PositionDirection.Long ? 1 : -1);
            return volumeInAsset * basisOfCalc / 365;
        }

        public async Task<decimal> CalculateOrderExecutionCommission(string instrument, string legalEntity, decimal volume)
        {
            var rateSettings = await _rateSettingsService.GetOrderExecutionRate(instrument);

            var volumeInAsset = _cfdCalculatorService.GetQuoteRateForQuoteAsset(rateSettings.CommissionAsset,
                                    instrument, legalEntity)
                                * Math.Abs(volume);
            
            var commission = Math.Min(
                rateSettings.CommissionCap, 
                Math.Max(
                    rateSettings.CommissionFloor,
                    rateSettings.CommissionRate * volumeInAsset));

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

            var rateSettings = await _rateSettingsService.GetOnBehalfRate(); 
            
            //use fx rates to convert to account asset
            var quote = _cfdCalculatorService.GetQuote(rateSettings.CommissionAsset, accountAssetId, 
                rateSettings.LegalEntity);
            
            //calculate commission
            return (actionsNum, actionsNum * rateSettings.Commission * quote);

            async Task<bool> CorrelatesWithParent(OrderEventContract order) =>
                (await _orderEventsApi.OrderById(order.ParentOrderId, OrderStatusContract.Placed, false))
                .Any(p => p.CorrelationId == order.CorrelationId);
        }
    }
}