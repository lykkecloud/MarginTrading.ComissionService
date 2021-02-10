// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.OrderDetailFeature;
using MarginTrading.CommissionService.Core.Repositories;
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
        private readonly IMapper _mapper;

        public OrderDetailsCalculationService(IOrderEventsApi orderEventsApi,
            ICommissionHistoryRepository commissionHistoryRepository,
            IMapper mapper)
        {
            _orderEventsApi = orderEventsApi;
            _commissionHistoryRepository = commissionHistoryRepository;
            _mapper = mapper;
        }

        public async Task<OrderDetailsData> Calculate(string orderId, string accountId)
        {
            var orderHistory = await _orderEventsApi.OrderById(orderId);
            var order = orderHistory
                // check order & status
                .OrderByDescending(x => x.ModifiedTimestamp)
                .FirstOrDefault(x => x.Status == OrderStatusContract.Executed
                                     || x.Status == OrderStatusContract.Canceled
                                     || x.Status == OrderStatusContract.Expired
                                     || x.Status == OrderStatusContract.Rejected);

            if (order == null) throw new Exception("Cannot find order");
            if (order.AccountId != accountId) throw new Exception("AccountId does not match");

            var commissionHistory = await _commissionHistoryRepository.GetByOrderIdAsync(orderId);
            if (order.Status == OrderStatusContract.Executed && commissionHistory == null)
                throw new Exception("Commission has not been calculated yet");

            var result = new OrderDetailsData();

            // todo: check api (download as pdf from frontend grid)
            // donut api for executed orders
            var exchangeRate = order.FxRate == 0 ? 1 : 1 / order.FxRate;

            decimal? notional = null;
            decimal? notionalEUR = null;
            if (order.Status == OrderStatusContract.Executed && order.ExecutionPrice.HasValue)
            {
                notional = order.Volume * order.ExecutionPrice.Value;
                notionalEUR = notional / exchangeRate;
            }

            result.Instrument = order.AssetPairId;
            result.Quantity = order.Volume;
            result.Status = _mapper.Map<OrderStatusContract, OrderStatus>(order.Status);
            result.OrderType = _mapper.Map<OrderTypeContract, OrderType>(order.Type);
            result.LimitStopPrice = order.ExpectedOpenPrice;
            result.TakeProfitPrice = order.TakeProfit?.Price;
            result.StopLossPrice = order.StopLoss?.Price;
            result.ExecutionPrice = order.ExecutionPrice;
            result.Notional = notional;
            result.NotionalEUR = notionalEUR;
            result.ExchangeRate = exchangeRate;
            result.ProductCost = commissionHistory.ProductCost;
            result.OrderDirection = _mapper.Map<OrderDirectionContract, OrderDirection>(order.Direction);
            result.Origin = _mapper.Map<OriginatorTypeContract, OriginatorType>(order.Originator);
            result.OrderId = order.Id;
            result.CreatedTimestamp = order.CreatedTimestamp;
            result.ModifiedTimestamp = order.ModifiedTimestamp;
            result.ExecutedTimestamp = order.ExecutedTimestamp;
            result.CanceledTimestamp = order.CanceledTimestamp;
            result.ValidityTime = order.ValidityTime;
            result.OrderComment = order.Comment;
            result.ForceOpen = order.ForceOpen;
            result.Commission = commissionHistory.Commission;
            result.TotalCostsAndCharges = commissionHistory.Commission + commissionHistory.ProductCost;
            result.ConfirmedManually = GetManualConfirmationStatus(order.AdditionalInfo);
            result.AccountId = order.AccountId;

            return result;
        }

        private bool GetManualConfirmationStatus(string orderInfo)
        {
            const string fieldName = "ConfirmedManually";

            var info = JsonConvert.DeserializeObject<Dictionary<string, object>>(orderInfo);

            if (info.TryGetValue(fieldName, out var confirmedManuallyFlagStr))
            {
                if (bool.TryParse(confirmedManuallyFlagStr.ToString(), out var confirmedManually))
                {
                    return confirmedManually;
                }
            }

            return false;
        }
    }
}