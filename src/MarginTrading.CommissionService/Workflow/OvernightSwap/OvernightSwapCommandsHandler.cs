// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Repositories;
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
        public const string OperationName = "OvernightSwapCommission";
        
        private readonly IOvernightSwapService _overnightSwapService;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly ISystemClock _systemClock;
        private readonly IChaosKitty _chaosKitty;
        private readonly ILog _log;
        private readonly CommissionServiceSettings _commissionServiceSettings;

        public OvernightSwapCommandsHandler(
            IOvernightSwapService overnightSwapService,
            IOperationExecutionInfoRepository executionInfoRepository,
            ISystemClock systemClock,
            IChaosKitty chaosKitty,
            ILog log,
            CommissionServiceSettings commissionServiceSettings)
        {
            _overnightSwapService = overnightSwapService;
            _executionInfoRepository = executionInfoRepository;
            _systemClock = systemClock;
            _chaosKitty = chaosKitty;
            _log = log;
            _commissionServiceSettings = commissionServiceSettings;
        }

        /// <summary>
        /// Calculate commission size
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(StartOvernightSwapsProcessCommand command,
            IEventPublisher publisher)
        {
            var executionInfo = await _executionInfoRepository.GetOrAddAsync(
                operationName: OperationName, 
                operationId: command.OperationId,
                factory: () => new OperationExecutionInfo<OvernightSwapOperationData>(
                    operationName: OperationName,
                    id: command.OperationId,
                    lastModified: _systemClock.UtcNow.UtcDateTime,
                    data: new OvernightSwapOperationData
                    {
                        NumberOfFinancingDays = command.NumberOfFinancingDays,
                        FinancingDaysPerYear = command.FinancingDaysPerYear,
                        TradingDay = command.TradingDay.ValidateTradingDay(_log, nameof(StartOvernightSwapsProcessCommand)),
                        State = CommissionOperationState.Initiated,
                    }
                ));

            if (executionInfo?.Data?.State != CommissionOperationState.Initiated)
            {
                return;
            }

            var now = _systemClock.UtcNow.UtcDateTime;

            if (executionInfo.Data.TradingDay > now)
            {
                publisher.PublishEvent(new OvernightSwapsStartFailedEvent(
                    operationId: command.OperationId,
                    creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                    failReason: $"TradingDay {executionInfo.Data.TradingDay} is invalid. Must be today or yesterday."
                ));
                return; //no retries 
            }

            IReadOnlyList<IOvernightSwapCalculation> calculatedSwaps = null;
            try
            {
                calculatedSwaps = await _overnightSwapService.Calculate(command.OperationId, command.CreationTimestamp, 
                    command.NumberOfFinancingDays, command.FinancingDaysPerYear, executionInfo.Data.TradingDay);
            }
            catch (Exception exception)
            {
                publisher.PublishEvent(new OvernightSwapsStartFailedEvent(
                    operationId: command.OperationId,
                    creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                    failReason: exception.Message
                ));
                await _log.WriteErrorAsync(nameof(OvernightSwapCommandsHandler), nameof(Handle), exception, _systemClock.UtcNow.UtcDateTime);
                return; //no retries
            }

            var swapsToCharge = calculatedSwaps.Where(x => x.IsSuccess).ToList();
            
            publisher.PublishEvent(new OvernightSwapsCalculatedEvent(
                operationId: command.OperationId,
                creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                total: calculatedSwaps.Count,
                failed: calculatedSwaps.Count(x => !x.IsSuccess)
            ));
            
            _chaosKitty.Meow(command.OperationId);

            foreach (var swap in swapsToCharge)
            {
                //prepare state for sub operations
                var swapExecutionInfo = await _executionInfoRepository.GetOrAddAsync(
                    operationName: OperationName,
                    operationId: swap.Id,
                    factory: () => new OperationExecutionInfo<OvernightSwapOperationData>(
                        operationName: OperationName,
                        id: swap.Id,
                        lastModified: _systemClock.UtcNow.UtcDateTime,
                        data: new OvernightSwapOperationData
                        {
                            State = CommissionOperationState.Initiated,
                            TradingDay = executionInfo.Data.TradingDay,
                            NumberOfFinancingDays = executionInfo.Data.NumberOfFinancingDays,
                            FinancingDaysPerYear = executionInfo.Data.FinancingDaysPerYear,
                        }
                    ));

                if (swapExecutionInfo?.Data?.State != CommissionOperationState.Initiated)
                {
                    continue;
                }

                publisher.PublishEvent(new OvernightSwapCalculatedInternalEvent(
                    operationId: swap.Id,
                    creationTimestamp: _systemClock.UtcNow.DateTime,
                    accountId: swap.AccountId,
                    positionId: swap.PositionId,
                    assetPairId: swap.Instrument,
                    swapAmount: swap.SwapValue,
                    details: swap.Details,
                    tradingDay: executionInfo.Data.TradingDay));
            }
        }

        private async Task<CommandHandlingResult> Handle(ChargeSwapsTimeoutInternalCommand command,
            IEventPublisher publisher)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<OvernightSwapOperationData>(
                operationName: OperationName,
                id: command.OperationId);

            if (executionInfo?.Data != null)
            {
                if (executionInfo.Data.State > CommissionOperationState.Calculated)
                {
                    return CommandHandlingResult.Ok();
                }
                
                if (_systemClock.UtcNow.UtcDateTime >= command.CreationTime.AddSeconds(command.TimeoutSeconds))
                {
                    var (total, failed, _) = await _overnightSwapService.GetOperationState(command.OperationId);
                    
                    publisher.PublishEvent(new OvernightSwapsChargedEvent(
                        operationId: command.OperationId,
                        creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                        total: total,
                        failed: failed
                    ));

                    return CommandHandlingResult.Ok();
                }
            }

            return CommandHandlingResult.Fail(_commissionServiceSettings.OvernightSwapsRetryTimeout);
        }
    }
}