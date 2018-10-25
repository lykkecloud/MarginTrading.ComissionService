using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Events;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Workflow.ChargeCommission
{
    internal class OrderExecCommissionCommandsHandler
    {
        public const string OperationName = "ExecutedOrderCommission";
        private readonly ICommissionCalcService _commissionCalcService;
        private readonly IChaosKitty _chaosKitty;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly ISystemClock _systemClock;

        public OrderExecCommissionCommandsHandler(ICommissionCalcService commissionCalcService,
            IChaosKitty chaosKitty, 
            IOperationExecutionInfoRepository executionInfoRepository,
            ISystemClock systemClock)
        {
            _commissionCalcService = commissionCalcService;
            _chaosKitty = chaosKitty;
            _executionInfoRepository = executionInfoRepository;
            _systemClock = systemClock;
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
                    lastModified: _systemClock.UtcNow.UtcDateTime,
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
                var commissionAmount = await _commissionCalcService.CalculateOrderExecutionCommission(
                  command.AccountId, command.Instrument, command.LegalEntity, command.Volume, command.OrderExecutionPrice);

                //no failure handling.. so operation will be retried on fail

                publisher.PublishEvent(new OrderExecCommissionCalculatedInternalEvent(
                    operationId: command.OperationId,
                    accountId: command.AccountId,
                    orderId: command.OrderId,
                    assetPairId: command.Instrument,
                    amount: commissionAmount,
                    commissionType: CommissionType.OrderExecution,
                    reason:
                    $"{CommissionType.OrderExecution.ToString()} commission for {command.Instrument} order #{command.OrderCode}, id: {command.OrderId}, volume: {command.Volume}",
                    tradingDay: command.TradingDay
                ));
                
                _chaosKitty.Meow(command.OperationId);
                
                await _executionInfoRepository.Save(executionInfo);
            }

            return CommandHandlingResult.Ok();
        }
    }
}