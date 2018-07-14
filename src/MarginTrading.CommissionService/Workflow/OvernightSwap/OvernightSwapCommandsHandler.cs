using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.MarginTrading.CommissionService.Contracts.Commands;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.OvernightSwap.Events;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Workflow.OvernightSwap
{
    public class OvernightSwapCommandsHandler
    {
        private readonly IOvernightSwapService _overnightSwapService;
        private readonly ISystemClock _systemClock;
        private readonly IChaosKitty _chaosKitty;

        public OvernightSwapCommandsHandler(
            IOvernightSwapService overnightSwapService,
            ISystemClock systemClock,
            IChaosKitty chaosKitty)
        {
            _overnightSwapService = overnightSwapService;
            _systemClock = systemClock;
            _chaosKitty = chaosKitty;
        }

        /// <summary>
        /// Calculate commission size
        /// </summary>
        [UsedImplicitly]
        private async Task<CommandHandlingResult> Handle(StartOvernightSwapsProcessCommand command,
            IEventPublisher publisher)
        {
            if (!await _overnightSwapService.CheckOperationIsNew(command.OperationId))
            {
                return CommandHandlingResult.Ok(); //idempotency violated - no need to retry
            }

            var calculatedSwaps = await _overnightSwapService.Calculate(command.OperationId, command.CreationTimestamp);

            foreach(var swap in calculatedSwaps)
            {
                publisher.PublishEvent(new OvernightSwapCalculatedInternalEvent(
                    operationId: swap.Id,
                    creationTimestamp: _systemClock.UtcNow.DateTime,
                    accountId: swap.AccountId,
                    positionId: swap.PositionId,
                    assetPairId: swap.Instrument,
                    swapAmount: swap.SwapValue));
                
//                _chaosKitty.Meow(nameof(OvernightSwapCommandsHandler));
            }
            
            return CommandHandlingResult.Ok();
        }

    }
}