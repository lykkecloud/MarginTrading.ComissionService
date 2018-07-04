using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.MarginTrading.CommissionService.Contracts.Commands;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Workflow.DailyPnl.Events;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Workflow.OvernightSwap
{
    public class DailyPnlCommandsHandler
    {
        private readonly IDailyPnlService _dailyPnlService;
        private readonly ISystemClock _systemClock;
        private readonly IChaosKitty _chaosKitty;

        public DailyPnlCommandsHandler(
            IDailyPnlService dailyPnlService,
            ISystemClock systemClock,
            IChaosKitty chaosKitty)
        {
            _systemClock = systemClock;
            _chaosKitty = chaosKitty;
            _dailyPnlService = dailyPnlService;
        }

        /// <summary>
        /// Calculate PnL
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(StartDailyPnlProcessCommand command,
            IEventPublisher publisher)
        {
            //todo ensure idempotency https://lykke-snow.atlassian.net/browse/MTC-205

            var calculatedPnLs = await _dailyPnlService.Calculate(command.OperationId, command.CreationTimestamp);

            foreach(var pnl in calculatedPnLs)
            {
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

//                _chaosKitty.Meow(nameof(OvernightSwapCommandsHandler));
            }
            
            return CommandHandlingResult.Ok();
        }

    }
}