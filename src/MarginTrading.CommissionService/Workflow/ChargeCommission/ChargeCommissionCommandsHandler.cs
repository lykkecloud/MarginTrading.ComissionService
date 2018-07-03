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
    internal class ChargeCommissionCommandsHandler
    {
        private readonly ICommissionCalcService _commissionCalcService;
        private readonly IChaosKitty _chaosKitty;
        private readonly IConvertService _convertService;

        public ChargeCommissionCommandsHandler(ICommissionCalcService commissionCalcService,
            IChaosKitty chaosKitty, IConvertService convertService)
        {
            _commissionCalcService = commissionCalcService;
            _chaosKitty = chaosKitty;
            _convertService = convertService;
        }

        /// <summary>
        /// Calculate commision size
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(HandleExecutedOrderInternalCommand command,
            IEventPublisher publisher)
        {
            var commissionAmount = _commissionCalcService.CalculateOrderExecutionCommission(
                command.Instrument, command.LegalEntity, command.Volume);
            
            //no failure handling.. so operation will be retried on fail
            
            _chaosKitty.Meow(command.OperationId);
            
            publisher.PublishEvent(new CommissionCalculatedInternalEvent(
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