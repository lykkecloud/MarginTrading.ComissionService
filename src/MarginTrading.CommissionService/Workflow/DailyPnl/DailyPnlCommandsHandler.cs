using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.MarginTrading.CommissionService.Contracts.Commands;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Workflow.DailyPnl.Events;
using MarginTrading.CommissionService.Workflow.ChargeCommission;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Workflow.OvernightSwap
{
    public class DailyPnlCommandsHandler
    {
        public const string OperationName = "DailyPnlCommission";
        private readonly IDailyPnlService _dailyPnlService;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly ISystemClock _systemClock;
        private readonly IChaosKitty _chaosKitty;

        public DailyPnlCommandsHandler(
            IDailyPnlService dailyPnlService,
            IOperationExecutionInfoRepository executionInfoRepository,
            ISystemClock systemClock,
            IChaosKitty chaosKitty)
        {
            _dailyPnlService = dailyPnlService;
            _executionInfoRepository = executionInfoRepository;
            _systemClock = systemClock;
            _chaosKitty = chaosKitty;
        }

        /// <summary>
        /// Calculate PnL
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(StartDailyPnlProcessCommand command,
            IEventPublisher publisher)
        {
            //ensure idempotency of the whole operation
            var executionInfo = await _executionInfoRepository.GetOrAddAsync(
                operationName: OperationName, 
                operationId: command.OperationId,
                factory: () => new OperationExecutionInfo<DailyPnlOperationData>(
                    operationName: OperationName,
                    id: command.OperationId,
                    lastModified: _systemClock.UtcNow.UtcDateTime,
                    data: new DailyPnlOperationData
                    {
                        TradingDay = command.CreationTimestamp,
                        State = CommissionOperationState.Initiated,
                    }
                ));

            if (ChargeCommissionSaga.SwitchState(executionInfo?.Data, CommissionOperationState.Initiated,
                CommissionOperationState.Started))
            {
                var calculatedPnLs = await _dailyPnlService.Calculate(command.OperationId, command.CreationTimestamp);

                foreach (var pnl in calculatedPnLs)
                {
                    //prepare state for sub operations
                    await _executionInfoRepository.GetOrAddAsync(
                        operationName: OperationName, 
                        operationId: pnl.GetId(),
                        factory: () => new OperationExecutionInfo<DailyPnlOperationData>(
                            operationName: OperationName,
                            id: pnl.GetId(),
                            lastModified: _systemClock.UtcNow.UtcDateTime,
                            data: new DailyPnlOperationData
                            {
                                TradingDay = command.CreationTimestamp,
                                State = CommissionOperationState.Started,
                            }
                        ));
                    
                    publisher.PublishEvent(new DailyPnlCalculatedInternalEvent(
                        operationId: pnl.OperationId,
                        creationTimestamp: _systemClock.UtcNow.DateTime,
                        accountId: pnl.AccountId,
                        positionId: pnl.PositionId,
                        assetPairId: pnl.Instrument,
                        pnl: pnl.Pnl,
                        tradingDay: pnl.TradingDay,
                        volume: pnl.Volume,
                        fxRate: pnl.FxRate));

                    _chaosKitty.Meow(nameof(OvernightSwapCommandsHandler));
                }
                
                await _executionInfoRepository.Save(executionInfo);
            }

            return CommandHandlingResult.Ok();
        }

    }
}