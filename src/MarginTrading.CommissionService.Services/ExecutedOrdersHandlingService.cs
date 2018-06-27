using System;
using System.Threading.Tasks;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.Backend.Contracts.Orders;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;

namespace MarginTrading.CommissionService.Services
{
    public class ExecutedOrdersHandlingService : IExecutedOrdersHandlingService
    {
        private readonly IEventSender _eventSender;

        public ExecutedOrdersHandlingService(IEventSender eventSender)
        {
            _eventSender = eventSender;
        }

        public Task Handle(OrderHistoryEvent orderHistoryEvent)
        {
            //todo ensure idempotency
            
            var order = orderHistoryEvent.OrderSnapshot;
            if (order == null)
            {
                throw new ArgumentNullException(nameof(orderHistoryEvent.OrderSnapshot), "Order cannot be null");
            }

            if (order.Status != OrderStatusContract.Executed)
            {
                return Task.CompletedTask;
            }
            
            return _eventSender.SendHandleExecutedOrderInternalCommand(new HandleExecutedOrderInternalCommand(
                $"{orderHistoryEvent.OrderSnapshot.Id}-order-executed", 
                order.AccountId.RequiredNotNullOrWhiteSpace(nameof(order.AccountId)), 
                order.Id.RequiredNotNullOrWhiteSpace(nameof(order.Id)), 
                order.Code.RequiredGreaterThan(default(long), nameof(order.Code)), 
                order.AssetPairId.RequiredNotNullOrWhiteSpace(nameof(order.AssetPairId)),
                order.LegalEntity.RequiredNotNullOrWhiteSpace(nameof(order.LegalEntity)),
                order.Volume.RequiredNotNull(nameof(order.Volume)))
            );
        }
    }
}