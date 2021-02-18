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
        private readonly IAssetsCache _assetsCache;
        private readonly IAccountRedisCache _accountsCache;
        private readonly IBrokerSettingsService _brokerSettingsService;
        private readonly IConvertService _convertService;

        private List<OrderStatusContract> _finalStatuses = new List<OrderStatusContract>()
        {
            OrderStatusContract.Executed,
            OrderStatusContract.Canceled,
            OrderStatusContract.Expired,
            OrderStatusContract.Rejected,
        };

        public OrderDetailsCalculationService(IOrderEventsApi orderEventsApi,
            ICommissionHistoryRepository commissionHistoryRepository,
            IAssetsCache assetsCache,
            IAccountRedisCache accountsCache,
            IBrokerSettingsService brokerSettingsService,
            IConvertService convertService)
        {
            _orderEventsApi = orderEventsApi;
            _commissionHistoryRepository = commissionHistoryRepository;
            _assetsCache = assetsCache;
            _accountsCache = accountsCache;
            _brokerSettingsService = brokerSettingsService;
            _convertService = convertService;
        }

        public async Task<OrderDetailsData> Calculate(string orderId, string accountId)
        {
            var orderHistory = await _orderEventsApi.OrderById(orderId);
            var order = orderHistory
                            .OrderByDescending(x => x.ModifiedTimestamp)
                            .FirstOrDefault(x => _finalStatuses.Contains(x.Status))
                        ?? orderHistory
                            .OrderByDescending(x => x.ModifiedTimestamp)
                            .FirstOrDefault(x => x.Status == OrderStatusContract.Placed);

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
                notional = order.Volume * order.ExecutionPrice.Value;
                notionalEUR = notional / exchangeRate;
            }

            var accountName = (await _accountsCache.GetAccount(order.AccountId))?.AccountName;

            if (string.IsNullOrEmpty(accountName))
                accountName = accountId;

            result.Instrument = _assetsCache.GetName(order.AssetPairId);
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
            result.ProductCost = commissionHistory?.ProductCost;
            result.OrderDirection = _convertService.Convert<OrderDirectionContract, OrderDirection>(order.Direction);
            result.Origin = _convertService.Convert<OriginatorTypeContract, OriginatorType>(order.Originator);
            result.OrderId = order.Id;
            result.CreatedTimestamp = order.CreatedTimestamp;
            result.ModifiedTimestamp = order.ModifiedTimestamp;
            result.ExecutedTimestamp = order.ExecutedTimestamp;
            result.CanceledTimestamp = order.CanceledTimestamp;
            result.ValidityTime = order.ValidityTime;
            // todo: check that this is the correct comment
            result.OrderComment = GetString(order.AdditionalInfo, "UserComments");
            result.ForceOpen = order.ForceOpen;
            result.Commission = commissionHistory?.Commission;
            result.TotalCostsAndCharges = commissionHistory?.Commission + commissionHistory?.ProductCost;
            result.ProductComplexityConfirmationReceived =
                GetBoolean(order.AdditionalInfo, "ProductComplexityConfirmationReceived");
            result.TotalCostPercent = GetDecimal(order.AdditionalInfo, "TotalCostPercentShownToUser");
            result.LossRatioMin = GetDecimal(order.AdditionalInfo, "LossRatioMinShownToUser");
            result.LossRatioMax = GetDecimal(order.AdditionalInfo, "LossRatioMaxShownToUser");
            result.AccountName = accountName;
            result.SettlementCurrency = await _brokerSettingsService.GetSettlementCurrencyAsync();
            result.EnableAllWarnings = GetAllWarningsFlag(order);

            return result;
        }

        private bool GetAllWarningsFlag(OrderEventWithAdditionalContract order)
        {
            if (order.Type == OrderTypeContract.StopLoss
                || order.Type == OrderTypeContract.TakeProfit
                || order.Type == OrderTypeContract.TrailingStop)
                return false;

            if (order.Originator == OriginatorTypeContract.System)
                return false;

            // todo: return false when order is executed from "close position" button on FE - how to detect? 

            return true;
        }

        private string GetString(string orderInfo, string fieldName)
        {
            var info = JsonConvert.DeserializeObject<Dictionary<string, object>>(orderInfo);

            if (info.TryGetValue(fieldName, out var fieldValue))
            {
                return fieldValue.ToString();
            }

            return null;
        }

        private bool GetBoolean(string orderInfo, string fieldName)
        {
            var info = JsonConvert.DeserializeObject<Dictionary<string, object>>(orderInfo);

            if (info.TryGetValue(fieldName, out var fieldValue))
            {
                if (bool.TryParse(fieldValue.ToString(), out var result))
                {
                    return result;
                }
            }

            return false;
        }

        private decimal? GetDecimal(string orderInfo, string fieldName)
        {
            var info = JsonConvert.DeserializeObject<Dictionary<string, object>>(orderInfo);

            if (info.TryGetValue(fieldName, out var fieldValue))
            {
                if (decimal.TryParse(fieldValue.ToString(), out var result))
                {
                    return result;
                }
            }

            return null;
        }
    }
}