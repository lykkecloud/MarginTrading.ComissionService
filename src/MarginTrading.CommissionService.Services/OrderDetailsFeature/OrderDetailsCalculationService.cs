// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.OrderDetailFeature;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Services.OrderDetailsFeature;
using MarginTrading.TradingHistory.Client;
using MarginTrading.TradingHistory.Client.Models;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.Services.OrderDetailsFeature
{
    public class OrderDetailsCalculationService : IOrderDetailsCalculationService
    {
        private readonly IOrderEventsApi _orderEventsApi;
        private readonly ICommissionHistoryRepository _commissionHistoryRepository;
        private readonly IProductsCache _productsCache;
        private readonly IAccountRedisCache _accountsCache;
        private readonly IBrokerSettingsService _brokerSettingsService;
        private readonly IRateSettingsService _rateSettingsService;
        private readonly IQuoteCacheService _quoteCacheService;
        private readonly IInterestRatesCacheService _interestRatesCacheService;
        private readonly IProductCostCalculationService _productCostCalculationService;
        private readonly ICfdCalculatorService _cfdCalculatorService;
        private readonly ICommissionCalcService _commissionCalcService;
        private readonly IConvertService _convertService;

        private const int OvernightFeeDays = 1;

        private List<OrderStatusContract> _finalStatuses = new List<OrderStatusContract>()
        {
            OrderStatusContract.Executed,
            OrderStatusContract.Canceled,
            OrderStatusContract.Expired,
            OrderStatusContract.Rejected,
        };

        public OrderDetailsCalculationService(IOrderEventsApi orderEventsApi,
            ICommissionHistoryRepository commissionHistoryRepository,
            IProductsCache productsCache,
            IAccountRedisCache accountsCache,
            IBrokerSettingsService brokerSettingsService,
            IRateSettingsService rateSettingsService,
            IQuoteCacheService quoteCacheService,
            IInterestRatesCacheService interestRatesCacheService,
            IProductCostCalculationService productCostCalculationService,
            ICfdCalculatorService cfdCalculatorService,
            ICommissionCalcService commissionCalcService,
            IConvertService convertService)
        {
            _orderEventsApi = orderEventsApi;
            _commissionHistoryRepository = commissionHistoryRepository;
            _productsCache = productsCache;
            _accountsCache = accountsCache;
            _brokerSettingsService = brokerSettingsService;
            _rateSettingsService = rateSettingsService;
            _quoteCacheService = quoteCacheService;
            _interestRatesCacheService = interestRatesCacheService;
            _productCostCalculationService = productCostCalculationService;
            _cfdCalculatorService = cfdCalculatorService;
            _commissionCalcService = commissionCalcService;
            _convertService = convertService;
        }

        public async Task<OrderDetailsData> Calculate(string orderId, string accountId)
        {
            var order = await GetOrder(orderId);

            if (order == null) throw new Exception($"Cannot find order {orderId}");

            if (order.AccountId != accountId)
                throw new Exception(
                    $"AccountId does not match for {orderId}: {order.AccountId} on order, {accountId} in request");

            var commissionHistory = await _commissionHistoryRepository.GetByOrderIdAsync(orderId);
            if (order.Status == OrderStatusContract.Executed && commissionHistory == null)
                throw new Exception($"Commission has not been calculated yet for executed order {orderId}");

            var result = new OrderDetailsData();

            var exchangeRate = order.FxRate == 0 ? 1 : 1 / order.FxRate;

            decimal? notional = null;
            decimal? notionalEUR = null;
            if (order.Status == OrderStatusContract.Executed && order.ExecutionPrice.HasValue)
            {
                notional = Math.Abs(order.Volume * order.ExecutionPrice.Value);
                notionalEUR = notional / exchangeRate;
            }

            var accountName = (await _accountsCache.GetAccount(order.AccountId))?.AccountName;

            if (string.IsNullOrEmpty(accountName))
                accountName = accountId;

            var (productCost, commission, totalCost) = await CalculateCosts(order);

            result.Instrument = _productsCache.GetName(order.AssetPairId);
            result.Quantity = order.Volume;
            result.Status = _convertService.Convert<OrderStatusContract, OrderStatus>(order.Status);
            result.OrderType = _convertService.Convert<OrderTypeContract, OrderType>(order.Type);
            result.LimitStopPrice = order.ExpectedOpenPrice;
            result.TakeProfitPrice = order.TakeProfit?.Price;
            result.StopLossPrice = order.StopLoss?.Price;
            result.ExecutionPrice = order.ExecutionPrice;
            result.Notional = notional;
            result.NotionalEUR = notionalEUR;
            result.ExchangeRate = exchangeRate;
            result.ProductCost = productCost;
            result.OrderDirection = _convertService.Convert<OrderDirectionContract, OrderDirection>(order.Direction);
            result.Origin = _convertService.Convert<OriginatorTypeContract, OriginatorType>(order.Originator);
            result.OrderId = order.Id;
            result.CreatedTimestamp = order.CreatedTimestamp;
            result.ModifiedTimestamp = order.ModifiedTimestamp;
            result.ExecutedTimestamp = order.ExecutedTimestamp;
            result.CanceledTimestamp = order.CanceledTimestamp;
            result.ValidityTime = order.ValidityTime;
            result.OrderComment = GetFieldAsString(order.AdditionalInfo, "UserComments");
            result.ForceOpen = order.ForceOpen;
            result.Commission = commission;
            result.TotalCostsAndCharges = totalCost;
            result.ProductComplexityConfirmationReceived =
                GetBoolean(order.AdditionalInfo, "ProductComplexityConfirmationReceived") ?? false;
            result.TotalCostPercent = GetDecimal(order.AdditionalInfo, "TotalCostPercentShownToUser");
            result.LossRatioMin = GetDecimal(order.AdditionalInfo, "LossRatioMinShownToUser");
            result.LossRatioMax = GetDecimal(order.AdditionalInfo, "LossRatioMaxShownToUser");
            result.AccountName = accountName;
            result.SettlementCurrency = await _brokerSettingsService.GetSettlementCurrencyAsync();
            result.EnableAllWarnings = GetAllWarningsFlag(order);

            return result;
        }

        private async Task<(decimal? ProductCost, decimal? Commission, decimal? TotalCost)> CalculateCosts(
            OrderEventWithAdditionalContract order)
        {
            if (order.Status == OrderStatusContract.Executed)
            {
                var commissionHistory = await _commissionHistoryRepository.GetByOrderIdAsync(order.Id);
                if (order.Status == OrderStatusContract.Executed && commissionHistory == null)
                    throw new Exception($"Commission has not been calculated yet for executed order {order.Id}");

                var exchangeRate = order.FxRate == 0 ? 1 : 1 / order.FxRate;
                var transactionVolume = order.FxRate * Math.Abs(order.Volume) * order.ExecutionPrice.Value;

                // spread for executed orders is already calculated with transaction volume
                var productCost = _productCostCalculationService.ExecutedOrderProductCost(order.Spread,
                    commissionHistory.ProductCostCalculationData.OvernightSwapRate,
                    transactionVolume,
                    exchangeRate,
                    OvernightFeeDays,
                    commissionHistory.ProductCostCalculationData.VariableRateBase,
                    commissionHistory.ProductCostCalculationData.VariableRateQuote,
                    (OrderDirection) order.Direction);

                return (productCost, -commissionHistory.Commission, productCost + commissionHistory.Commission);
            }
            else if (order.Status == OrderStatusContract.Canceled
                     || order.Status == OrderStatusContract.Rejected
                     || order.Status == OrderStatusContract.Expired)
            {
                // no costs for not executed orders in final status
                return (null, null, null);
            }
            else
            {
                var overnightSwapRate =
                    await _rateSettingsService.GetOvernightSwapRate(order.AssetPairId);
                var currentBestPrice = _quoteCacheService.GetBidAskPair(order.AssetPairId);
                var price = GetPrice(order, currentBestPrice); 

                var spread = currentBestPrice.Ask - currentBestPrice.Bid;

                var variableRateBase = _interestRatesCacheService.GetRate(overnightSwapRate.VariableRateBase);
                var variableRateQuote = _interestRatesCacheService.GetRate(overnightSwapRate.VariableRateQuote);

                var account = await _accountsCache.GetAccount(order.AccountId);
                var fxRate =
                    _cfdCalculatorService.GetFxRateForAssetPair(account.BaseAssetId, order.AssetPairId,
                        order.LegalEntity);
                var exchangeRate = 1 / fxRate;

                var commission = -1 * (await _commissionCalcService.CalculateOrderExecutionCommission(account.Id,
                    order.AssetPairId,
                    order.Volume,
                    price,
                    fxRate
                ));

                var entryExitCommission = commission * 2;

                var transactionVolume = fxRate * Math.Abs(order.Volume) * price;

                var productCost = _productCostCalculationService.ProductCost(spread,
                    overnightSwapRate,
                    transactionVolume,
                    exchangeRate,
                    1,
                    variableRateBase,
                    variableRateQuote,
                    (OrderDirection) order.Direction);

                return (productCost, entryExitCommission, productCost + entryExitCommission);
            }
        }

        private decimal GetPrice(OrderEventWithAdditionalContract order, InstrumentBidAskPair currentBestPrice)
        {
            if (order.Type == OrderTypeContract.Market)
            {
                return order.Direction == OrderDirectionContract.Buy ? currentBestPrice.Ask : currentBestPrice.Bid;
            }

            if (!order.ExpectedOpenPrice.HasValue)
                throw new Exception($"Order {order.Id} with type {order.Type} does not have ExpectedOpenPrice");
            return order.ExpectedOpenPrice.Value;
        }

        private async Task<OrderEventWithAdditionalContract> GetOrder(string orderId)
        {
            var orderHistory = await _orderEventsApi.OrderById(orderId);

            var orderInFinalStatus = orderHistory
                .OrderByDescending(x => x.ModifiedTimestamp)
                .FirstOrDefault(x => _finalStatuses.Contains(x.Status));

            var updatedOrder = orderHistory
                .OrderByDescending(x => x.ModifiedTimestamp)
                .FirstOrDefault(x => x.UpdateType == OrderUpdateTypeContract.Change);

            var placedOrder = orderHistory
                .OrderByDescending(x => x.ModifiedTimestamp)
                .FirstOrDefault(x => x.Status == OrderStatusContract.Placed);

            var order = orderInFinalStatus ?? updatedOrder ?? placedOrder;
            return order;
        }

        private bool GetAllWarningsFlag(OrderEventWithAdditionalContract order)
        {
            if (order.Type == OrderTypeContract.StopLoss
                || order.Type == OrderTypeContract.TakeProfit
                || order.Type == OrderTypeContract.TrailingStop)
                return false;

            if (order.Originator == OriginatorTypeContract.System)
                return false;

            // order is executed from "close position" button on FE
            if (!string.IsNullOrEmpty(order.Comment) && order.Comment.Contains("Close positions"))
                return false;

            return true;
        }


        private bool? GetBoolean(string orderInfo, string fieldName)
        {
            var value = GetFieldAsString(orderInfo, fieldName);
            if (value == null) return null;

            if (bool.TryParse(value, out var result))
            {
                return result;
            }

            return null;
        }

        private decimal? GetDecimal(string orderInfo, string fieldName)
        {
            var value = GetFieldAsString(orderInfo, fieldName);
            if (value == null) return null;

            if (decimal.TryParse(value, out var result))
            {
                return result;
            }

            return null;
        }

        private string GetFieldAsString(string json, string fieldName)
        {
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);

            return dict.TryGetValue(fieldName, out var fieldValue) ? fieldValue?.ToString() : null;
        }
    }
}