﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Commands;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.Services
{
    public class ExecutedOrdersHandlingService : IExecutedOrdersHandlingService
    {
        private readonly ICqrsMessageSender _cqrsMessageSender;
        private readonly ISystemClock _systemClock;
        private readonly ILog _log;
        
        public static readonly string OnBehalfPostfix = "order-on-behalf";
        public static readonly string OrderExecPostfix = "order-executed";

        public ExecutedOrdersHandlingService(
            ICqrsMessageSender cqrsMessageSender,
            ISystemClock systemClock,
            ILog log)
        {
            _cqrsMessageSender = cqrsMessageSender;
            _systemClock = systemClock;
            _log = log;
        }

        public Task Handle(OrderHistoryEvent orderHistoryEvent)
        {
            var order = orderHistoryEvent.OrderSnapshot;
            if (order == null)
            {
                throw new ArgumentNullException(nameof(orderHistoryEvent.OrderSnapshot), "Order cannot be null");
            }

            if (order.Status != OrderStatusContract.Executed)
            {
                return Task.CompletedTask;
            }

            new List<Task>
            {
                //on behalf
                _cqrsMessageSender.SendHandleOnBehalfInternalCommand(new HandleOnBehalfInternalCommand(
                    operationId: $"{order.Id}-{OnBehalfPostfix}",
                    createdTimestamp: _systemClock.UtcNow.UtcDateTime,
                    accountId: order.AccountId,
                    accountAssetId: order.AccountAssetId,
                    orderId: order.Id,
                    assetPairId: order.AssetPairId,
                    tradingDay: order.ModifiedTimestamp
                )),
                //order exec commission
                _cqrsMessageSender.SendHandleExecutedOrderInternalCommand(new HandleOrderExecInternalCommand(
                    operationId: $"{order.Id}-{OrderExecPostfix}",
                    accountId: order.AccountId.RequiredNotNullOrWhiteSpace(nameof(order.AccountId)),
                    orderId: order.Id.RequiredNotNullOrWhiteSpace(nameof(order.Id)),
                    orderCode: order.Code.RequiredGreaterThan(default(long), nameof(order.Code)),
                    instrument: order.AssetPairId.RequiredNotNullOrWhiteSpace(nameof(order.AssetPairId)),
                    legalEntity: order.LegalEntity.RequiredNotNullOrWhiteSpace(nameof(order.LegalEntity)),
                    volume: order.Volume.RequiredNotNull(nameof(order.Volume)),
                    tradingDay: order.ModifiedTimestamp,
                    orderExecutionPrice: order.ExecutionPrice ?? 0)
                )
            }.ForEach(task => Task.Run(async () =>
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    await _log.WriteErrorAsync(nameof(Handle), "SwitchThread", "", ex);
                }
            }));
            
            return Task.CompletedTask;
        }
    }
}