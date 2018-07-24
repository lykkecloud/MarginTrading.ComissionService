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
    internal class OrderExecCommissionCommandsHandler
    {
        public const string OperationName = "ExecutedOrderCommission";
        private readonly ICommissionCalcService _commissionCalcService;
        private readonly IChaosKitty _chaosKitty;
        private readonly IConvertService _convertService;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;

        public OrderExecCommissionCommandsHandler(ICommissionCalcService commissionCalcService,
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
        private async Task<CommandHandlingResult> Handle(HandleOrderExecInternalCommand command,
            IEventPublisher publisher)
        {
            //ensure idempotency
            var executionInfo = await _executionInfoRepository.GetOrAddAsync(
                operationName: OperationName,
                operationId: command.OperationId,
                factory: () => new OperationExecutionInfo<ExecutedOrderOperationData>(
                    operationName: OperationName,
                    id: command.OperationId,
                    data: new ExecutedOrderOperationData()
                    {
                        AccountId = command.AccountId,
                        OrderId = command.OrderId,
                        OrderCode = command.OrderCode,
                        Instrument = command.Instrument,
                        LegalEntity = command.LegalEntity,
                        Volume = command.Volume,
                        State = CommissionOperationState.Initiated,
                    }
                ));
            
            if (ChargeCommissionSaga.SwitchState(executionInfo?.Data, CommissionOperationState.Initiated,
                CommissionOperationState.Started))
            {

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
                    reason:
                    $"{CommissionType.OrderExecution.ToString()} commission for {command.Instrument} order #{command.OrderCode}, id: {command.OrderId}, volume: {command.Volume}"
                ));
                
                _chaosKitty.Meow(command.OperationId);
                
                await _executionInfoRepository.Save(executionInfo);
            }

            return CommandHandlingResult.Ok();
        }
    }
}