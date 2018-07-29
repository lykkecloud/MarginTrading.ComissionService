using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Commands;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.Services
{
    public class ExecutedOrdersHandlingService : IExecutedOrdersHandlingService
    {
        private readonly IEventSender _eventSender;
        private readonly ISystemClock _systemClock;
        private readonly ILog _log;
        
        public static readonly string OnBehalfPostfix = "order-on-behalf";
        public static readonly string OrderExecPostfix = "order-executed";

        public ExecutedOrdersHandlingService(
            IEventSender eventSender,
            ISystemClock systemClock,
            ILog log)
        {
            _eventSender = eventSender;
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
                _eventSender.SendHandleOnBehalfInternalCommand(new HandleOnBehalfInternalCommand(
                    operationId: $"{order.Id}-{OnBehalfPostfix}",
                    createdTimestamp: _systemClock.UtcNow.UtcDateTime,
                    accountId: order.AccountId,
                    accountAssetId: order.AccountAssetId,
                    orderId: order.Id,
                    assetPairId: order.AssetPairId
                )),
                //order exec commission
                _eventSender.SendHandleExecutedOrderInternalCommand(new HandleOrderExecInternalCommand(
                    $"{order.Id}-{OrderExecPostfix}",
                    order.AccountId.RequiredNotNullOrWhiteSpace(nameof(order.AccountId)),
                    order.Id.RequiredNotNullOrWhiteSpace(nameof(order.Id)),
                    order.Code.RequiredGreaterThan(default(long), nameof(order.Code)),
                    order.AssetPairId.RequiredNotNullOrWhiteSpace(nameof(order.AssetPairId)),
                    order.LegalEntity.RequiredNotNullOrWhiteSpace(nameof(order.LegalEntity)),
                    order.Volume.RequiredNotNull(nameof(order.Volume)))
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