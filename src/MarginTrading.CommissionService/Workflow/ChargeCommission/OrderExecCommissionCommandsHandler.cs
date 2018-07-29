using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Events;

namespace MarginTrading.CommissionService.Workflow.ChargeCommission
{
    internal class OrderExecCommissionCommandsHandler
    {
        private readonly ICommissionCalcService _commissionCalcService;
        private readonly IChaosKitty _chaosKitty;

        public OrderExecCommissionCommandsHandler(
            ICommissionCalcService commissionCalcService,
            IChaosKitty chaosKitty)
        {
            _commissionCalcService = commissionCalcService;
            _chaosKitty = chaosKitty;
        }

        /// <summary>
        /// Calculate commission size
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(HandleOrderExecInternalCommand command,
            IEventPublisher publisher)
        {
            //todo ensure idempotency

            var commissionAmount = _commissionCalcService.CalculateOrderExecutionCommission(
                command.Instrument, command.LegalEntity, command.Volume);
            
            //no failure handling.. so operation will be retried on fail
            
            _chaosKitty.Meow(command.OperationId);
            
            publisher.PublishEvent(new OrderExecCommissionCalculatedInternalEvent(
                operationId: command.OperationId,
                accountId: command.AccountId,
                orderId: command.OrderId,
                assetPairId: command.Instrument,
                amount: commissionAmount,
                commissionType: CommissionType.OrderExecution,
                reason: $"{CommissionType.OrderExecution.ToString()} commission for {command.Instrument} order #{command.OrderCode}, id: {command.OrderId}, volume: {command.Volume}"
            ));
            
            return CommandHandlingResult.Ok();
        }
    }
}