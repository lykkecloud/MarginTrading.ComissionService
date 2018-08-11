using System;
using System.Collections.Generic;
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
        private readonly IAccountRedisCache _accountRedisCache;
        private readonly ILog _log;

        public CommissionCalcService(
            ICfdCalculatorService cfdCalculatorService,
            IRateSettingsService rateSettingsService,
            IOrderEventsApi orderEventsApi,
            IAccountRedisCache accountRedisCache,
            ILog log)
        {
            _cfdCalculatorService = cfdCalculatorService;
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
            
            var calculationBasis = _cfdCalculatorService.GetQuoteRateForQuoteAsset(account.BaseAssetId,
                                    openPosition.AssetPairId, assetPair.LegalEntity)
                                * Math.Abs(openPosition.CurrentVolume);

            interestRates.TryGetValue(rateSettings.VariableRateBase, out var variableRateBase);
            interestRates.TryGetValue(rateSettings.VariableRateQuote, out var variableRateQuote);
            
            var financingRate = - rateSettings.FixRate
                - (openPosition.Direction == PositionDirection.Short ? rateSettings.RepoSurchargePercent : 0)
                + (variableRateBase - variableRateQuote)
                              * (openPosition.Direction == PositionDirection.Long ? 1 : -1);

            var dayFactor = numberOfFinancingDays / financingDaysPerYear;

            return (calculationBasis * financingRate * dayFactor,
                    $"<calculation_basis>{calculationBasis}</calculation_basis><financing_rate>{financingRate}</financing_rate><day_factor>{dayFactor}</day_factor><fix_rate>{rateSettings.FixRate}</fix_rate>");
        }

        public async Task<decimal> CalculateOrderExecutionCommission(string accountId, string instrument, 
            string legalEntity, decimal volume)
        {
            var rateSettings = await _rateSettingsService.GetOrderExecutionRate(instrument);
            var account = await _accountRedisCache.GetAccount(accountId);

            var volumeInCommissionAsset = _cfdCalculatorService.GetQuoteRateForQuoteAsset(rateSettings.CommissionAsset,
                                    instrument, legalEntity)
                                * Math.Abs(volume);
            var commissionToAccountRate = _cfdCalculatorService.GetQuote(rateSettings.CommissionAsset, 
                account.BaseAssetId, rateSettings.LegalEntity);
            
            var commission = Math.Min(
                rateSettings.CommissionCap, 
                Math.Max(
                    rateSettings.CommissionFloor,
                    rateSettings.CommissionRate * volumeInCommissionAsset))
                * commissionToAccountRate;

            return commission;
        }

        public async Task<(int ActionsNum, decimal Commission)> CalculateOnBehalfCommissionAsync(string orderId,
            string accountAssetId)
        {
            var onBehalfEvents = (await _orderEventsApi.OrderById(orderId, null, false))
                .Where(CheckOnBehalfFlag).ToList();

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

        private bool CheckOnBehalfFlag(OrderEventContract orderEvent)
        {
            //used to be o => o.Originator == OriginatorTypeContract.OnBehalf
            try
            {
                return JsonConvert.DeserializeAnonymousType(orderEvent.AdditionalInfo, new {IsOnBehalf = false})
                    .IsOnBehalf;
            }
            catch
            {
                return false;
            }
        }
    }
}