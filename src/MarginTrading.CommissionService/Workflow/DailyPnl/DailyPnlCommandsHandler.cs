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
using MarginTrading.CommissionService.Core.Workflow.DailyPnl.Events;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Workflow.DailyPnl
{
    public class DailyPnlCommandsHandler
    {
        public const string OperationName = "DailyPnlCommission";
        private readonly IDailyPnlService _dailyPnlService;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly ISystemClock _systemClock;
        private readonly IChaosKitty _chaosKitty;
        private readonly ILog _log;
        private readonly CommissionServiceSettings _commissionServiceSettings;

        public DailyPnlCommandsHandler(
            IDailyPnlService dailyPnlService,
            IOperationExecutionInfoRepository executionInfoRepository,
            ISystemClock systemClock,
            IChaosKitty chaosKitty,
            ILog log,
            CommissionServiceSettings commissionServiceSettings)
        {
            _dailyPnlService = dailyPnlService;
            _executionInfoRepository = executionInfoRepository;
            _systemClock = systemClock;
            _chaosKitty = chaosKitty;
            _log = log;
            _commissionServiceSettings = commissionServiceSettings;
        }

        /// <summary>
        /// Calculate PnL
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(StartDailyPnlProcessCommand command, IEventPublisher publisher)
        {
            var executionInfo = await _executionInfoRepository.GetOrAddAsync(
                operationName: OperationName, 
                operationId: command.OperationId,
                factory: () => new OperationExecutionInfo<DailyPnlOperationData>(
                    operationName: OperationName,
                    id: command.OperationId,
                    lastModified: _systemClock.UtcNow.UtcDateTime,
                    data: new DailyPnlOperationData
                    {
                        TradingDay = command.TradingDay.ValidateTradingDay(_log, nameof(StartDailyPnlProcessCommand)),
                        State = CommissionOperationState.Initiated,
                    }
                ));

            if (executionInfo?.Data?.State == CommissionOperationState.Initiated)
            {
                IReadOnlyList<IDailyPnlCalculation> calculatedPnLs = null;
                try
                {
                    calculatedPnLs = await _dailyPnlService.Calculate(command.OperationId, executionInfo.Data.TradingDay);
                }
                catch (Exception exception)
                {
                    publisher.PublishEvent(new DailyPnlsStartFailedEvent(
                        operationId: command.OperationId,
                        creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                        failReason: exception.Message
                    ));
                    await _log.WriteErrorAsync(nameof(DailyPnlCommandsHandler), nameof(Handle), exception, _systemClock.UtcNow.UtcDateTime);
                    return; //no retries
                }
                
                var pnlsToCharge = calculatedPnLs.Where(x => x.IsSuccess).ToList();

                publisher.PublishEvent(new DailyPnlsCalculatedEvent(
                    operationId: command.OperationId,
                    creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                    total: calculatedPnLs.Count,
                    failed: calculatedPnLs.Count(x => !x.IsSuccess)
                ));

                _chaosKitty.Meow(command.OperationId);

                foreach (var pnl in pnlsToCharge)
                {
                    //prepare state for sub operations
                    await _executionInfoRepository.GetOrAddAsync(
                        operationName: OperationName, 
                        operationId: pnl.Id,
                        factory: () => new OperationExecutionInfo<DailyPnlOperationData>(
                            operationName: OperationName,
                            id: pnl.Id,
                            lastModified: _systemClock.UtcNow.UtcDateTime,
                            data: new DailyPnlOperationData
                            {
                                TradingDay = executionInfo.Data.TradingDay,
                                State = CommissionOperationState.Initiated,
                            }
                        ));
                    
                    publisher.PublishEvent(new DailyPnlCalculatedInternalEvent(
                        operationId: pnl.Id,
                        creationTimestamp: _systemClock.UtcNow.DateTime,
                        accountId: pnl.AccountId,
                        positionId: pnl.PositionId,
                        assetPairId: pnl.Instrument,
                        pnl: pnl.Pnl,
                        tradingDay: pnl.TradingDay,
                        volume: pnl.Volume,
                        fxRate: pnl.FxRate,
                        rawTotalPnL: pnl.RawTotalPnl
                    ));
                }
            }
        }
        
        private async Task<CommandHandlingResult> Handle(ChargeDailyPnlTimeoutInternalCommand command,
            IEventPublisher publisher)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DailyPnlOperationData>(
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
                    var (total, failed, _) = await _dailyPnlService.GetOperationState(command.OperationId);

                    publisher.PublishEvent(new DailyPnlsChargedEvent(
                        operationId: command.OperationId,
                        creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                        total: total,
                        failed: failed
                    ));

                    return CommandHandlingResult.Ok();
                }
            }

            return CommandHandlingResult.Fail(_commissionServiceSettings.DailyPnlsRetryTimeout);
        }
    }
}