using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.MarginTrading.CommissionService.Contracts.Commands;
using Lykke.MarginTrading.CommissionService.Contracts.Events;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.OvernightSwap.Events;
using MarginTrading.CommissionService.Services;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Workflow.OvernightSwap
{
    public class OvernightSwapCommandsHandler
    {
        private readonly IOvernightSwapService _overnightSwapService;
        private readonly IOvernightSwapListener _overnightSwapListener;
        private readonly ISystemClock _systemClock;
        private readonly IChaosKitty _chaosKitty;
        private readonly ILog _log;
        private readonly IThreadSwitcher _threadSwitcher;

        public OvernightSwapCommandsHandler(
            IOvernightSwapService overnightSwapService,
            IOvernightSwapListener overnightSwapListener,
            ISystemClock systemClock,
            IChaosKitty chaosKitty,
            ILog log,
            IThreadSwitcher threadSwitcher)
        {
            _overnightSwapService = overnightSwapService;
            _overnightSwapListener = overnightSwapListener;
            _systemClock = systemClock;
            _chaosKitty = chaosKitty;
            _log = log;
            _threadSwitcher = threadSwitcher;
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

            IReadOnlyList<IOvernightSwapCalculation> calculatedSwaps = null;
            try
            {
                calculatedSwaps = await _overnightSwapService.Calculate(command.OperationId, command.CreationTimestamp, 
                    command.NumberOfFinancingDays, command.FinancingDaysPerYear);
            }
            catch (Exception exception)
            {
                publisher.PublishEvent(new OvernightSwapsStartFailedEvent(
                    operationId: command.OperationId,
                    creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                    failReason: exception.Message
                ));
                await _log.WriteErrorAsync(nameof(DailyPnlCommandsHandler), nameof(Handle), exception, _systemClock.UtcNow.UtcDateTime);
                return CommandHandlingResult.Ok();//no retries
            }
            
            publisher.PublishEvent(new OvernightSwapsCalculatedEvent(
                operationId: command.OperationId,
                creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                total: calculatedSwaps.Count,
                failed: calculatedSwaps.Count(x => !x.IsSuccess) 
            ));

            var swapsToCharge = calculatedSwaps.Where(x => x.IsSuccess && x.SwapValue > 0);

            _threadSwitcher.SwitchThread(() => _overnightSwapListener.TrackCharging(
                operationId: command.OperationId, 
                operationIds: swapsToCharge.Select(x => x.Id).ToList(),
                publisher: publisher
            ));
            
            foreach(var swap in swapsToCharge)
            {
                publisher.PublishEvent(new OvernightSwapCalculatedInternalEvent(
                    operationId: swap.Id,
                    creationTimestamp: _systemClock.UtcNow.DateTime,
                    accountId: swap.AccountId,
                    positionId: swap.PositionId,
                    assetPairId: swap.Instrument,
                    swapAmount: swap.SwapValue,
                    details: swap.Details,
                    tradingDay: command.TradingDay));
            }
            
            return CommandHandlingResult.Ok();
        }

    }
}