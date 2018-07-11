using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Events;

namespace MarginTrading.CommissionService.Workflow.ChargeCommission
{
    internal class ChargeCommissionCommandsHandler
    {
        private const string OperationName = "ExecutedOrderCommission";
        private readonly ICommissionCalcService _commissionCalcService;
        private readonly IChaosKitty _chaosKitty;
        private readonly IConvertService _convertService;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;

        public ChargeCommissionCommandsHandler(ICommissionCalcService commissionCalcService,
            IChaosKitty chaosKitty, 
            IConvertService convertService,
            IOperationExecutionInfoRepository executionInfoRepository)
        {
            _commissionCalcService = commissionCalcService;
            _chaosKitty = chaosKitty;
            _convertService = convertService;
            _executionInfoRepository = executionInfoRepository;
        }

        /// <summary>
        /// Calculate commission size
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(HandleExecutedOrderInternalCommand command,
            IEventPublisher publisher)
        {
            //ensure idempotency
            if (await _executionInfoRepository.GetAsync<ExecutedOrderOperationData>(OperationName, command.OperationId) != null)
                return CommandHandlingResult.Ok();//no retries
            
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