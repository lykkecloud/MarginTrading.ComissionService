// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.MarginTrading.CommissionService.Contracts.Commands;
using Lykke.MarginTrading.CommissionService.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.Backend.Contracts.Positions;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Events;
using MarginTrading.CommissionService.Core.Workflow.DailyPnl.Events;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Events;
using MarginTrading.CommissionService.Core.Workflow.OvernightSwap.Events;
using MarginTrading.CommissionService.Workflow.DailyPnl;
using MarginTrading.CommissionService.Workflow.OnBehalf;
using MarginTrading.CommissionService.Workflow.OvernightSwap;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Workflow.ChargeCommission
{
    internal class ChargeCommissionSaga
    {
        private readonly IOvernightSwapService _overnightSwapService;
        private readonly IDailyPnlService _dailyPnlService;
        private readonly ISystemClock _systemClock;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IChaosKitty _chaosKitty;
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly CommissionServiceSettings _commissionServiceSettings;

        public ChargeCommissionSaga(
            IOvernightSwapService overnightSwapService,
            IDailyPnlService dailyPnlService,
            ISystemClock systemClock,
            IOperationExecutionInfoRepository executionInfoRepository,
            IChaosKitty chaosKitty,
            CqrsContextNamesSettings contextNames,
            CommissionServiceSettings commissionServiceSettings)
        {
            _overnightSwapService = overnightSwapService;
            _dailyPnlService = dailyPnlService;
            _systemClock = systemClock;
            _executionInfoRepository = executionInfoRepository;
            _chaosKitty = chaosKitty;
            _contextNames = contextNames;
            _commissionServiceSettings = commissionServiceSettings;
        }

        /// <summary>
        /// Send charge command to AccountManagement service
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(OrderExecCommissionCalculatedInternalEvent evt, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<ExecutedOrderOperationData>(
                operationName: OrderExecCommissionCommandsHandler.OperationName,
                id: evt.OperationId
            );

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(CommissionOperationState.Initiated,
                CommissionOperationState.Calculated))
            {
                sender.SendCommand(new ChangeBalanceCommand(
                        operationId: evt.OperationId,
                        clientId: null,
                        accountId: evt.AccountId,
                        amount: -evt.Amount,
                        reasonType: GetReasonType(evt.CommissionType),
                        reason: evt.Reason,
                        auditLog: null,
                        eventSourceId: evt.OrderId,
                        assetPairId: evt.AssetPairId,
                        tradingDay: evt.TradingDay),
                    _contextNames.AccountsManagement);
                
                _chaosKitty.Meow(evt.OperationId);
                
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// Send charge command to AccountManagement service
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(OnBehalfCalculatedInternalEvent evt, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<OnBehalfOperationData>(
                operationName: OnBehalfCommandsHandler.OperationName,
                id: evt.OperationId
            );

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(CommissionOperationState.Initiated,
                CommissionOperationState.Calculated))
            {
                if (evt.Commission != 0)
                {
                    sender.SendCommand(new ChangeBalanceCommand(
                        operationId: evt.OperationId,
                        clientId: null,
                        accountId: evt.AccountId,
                        amount: -evt.Commission,
                        reasonType: AccountBalanceChangeReasonTypeContract.OnBehalf,
                        reason: $"OnBehalf commission for order #{evt.OrderId}",
                        auditLog: null,
                        eventSourceId: evt.OrderId,
                        assetPairId: evt.AssetPairId,
                        tradingDay: evt.TradingDay),
                    _contextNames.AccountsManagement);
                }
                
                _chaosKitty.Meow(evt.OperationId);

                await _executionInfoRepository.Save(executionInfo);
            }
        }
        
        /// <summary>
        /// Send charge command to AccountManagement service
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(OvernightSwapCalculatedInternalEvent evt, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<OvernightSwapOperationData>(
                operationName: OvernightSwapCommandsHandler.OperationName,
                id: evt.OperationId
            );
            
            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(CommissionOperationState.Initiated,
                CommissionOperationState.Calculated))
            {
                sender.SendCommand(new ChangeBalanceCommand(
                        operationId: evt.OperationId,
                        clientId: null,
                        accountId: evt.AccountId, 
                        amount: evt.SwapAmount,
                        reasonType: AccountBalanceChangeReasonTypeContract.Swap,
                        reason: $"OvernightSwap commission for position #{evt.PositionId}",
                        auditLog: evt.Details,
                        eventSourceId: evt.PositionId,
                        assetPairId: evt.AssetPairId,
                        tradingDay: evt.TradingDay),
                    _contextNames.AccountsManagement);
            
                _chaosKitty.Meow(evt.OperationId);
            
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// All swaps were calculated
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(OvernightSwapsCalculatedEvent evt, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<OvernightSwapOperationData>(
                operationName: OvernightSwapCommandsHandler.OperationName,
                id: evt.OperationId);

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(CommissionOperationState.Initiated,
                CommissionOperationState.Calculated))
            {
                var (total, _, notProcessed) = await _overnightSwapService.GetOperationState(evt.OperationId);

                if (total == 0)
                {
                    sender.SendCommand(new ChargeSwapsTimeoutInternalCommand
                    {
                        OperationId = evt.OperationId,
                        CreationTime = _systemClock.UtcNow.UtcDateTime,
                        TimeoutSeconds = 0,
                    }, _contextNames.CommissionService);
                }
                else if (notProcessed > 0)
                {
                    sender.SendCommand(new ChargeSwapsTimeoutInternalCommand
                    {
                        OperationId = evt.OperationId,
                        CreationTime = _systemClock.UtcNow.UtcDateTime,
                        TimeoutSeconds = _commissionServiceSettings.OvernightSwapsChargingTimeoutSec,
                    }, _contextNames.CommissionService);
                }
                
                await _executionInfoRepository.Save(executionInfo);
            }   
        }

        /// <summary>
        /// OvernightSwaps failed
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(OvernightSwapsStartFailedEvent evt, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<OvernightSwapOperationData>(
                operationName: OvernightSwapCommandsHandler.OperationName,
                id: evt.OperationId);

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(CommissionOperationState.Initiated,
                CommissionOperationState.Failed))
            {
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// OvernightSwaps succeeded
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(OvernightSwapsChargedEvent evt, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<OvernightSwapOperationData>(
                operationName: OvernightSwapCommandsHandler.OperationName,
                id: evt.OperationId);

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(CommissionOperationState.Calculated,
                CommissionOperationState.Succeeded))
            {
                await _executionInfoRepository.Save(executionInfo);
            }
        }
        
        /// <summary>
        /// Send charge command to AccountManagement service
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(DailyPnlCalculatedInternalEvent evt, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DailyPnlOperationData>(
                operationName: DailyPnlCommandsHandler.OperationName,
                id: evt.OperationId
            );
            
            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(CommissionOperationState.Initiated,
                CommissionOperationState.Calculated))
            {
                var metadata = new UnrealizedPnlMetadataContract {RawTotalPnl = evt.RawTotalPnL};
                
                sender.SendCommand(new ChangeBalanceCommand(
                        operationId: evt.OperationId,
                        clientId: null,
                        accountId: evt.AccountId,
                        amount: evt.Pnl,
                        reasonType: AccountBalanceChangeReasonTypeContract.UnrealizedDailyPnL,
                        reason: $"Daily Pnl for account {evt.AccountId}",
                        auditLog: metadata.ToJson(),
                        eventSourceId: evt.PositionId,
                        assetPairId: evt.AssetPairId,
                        tradingDay: evt.TradingDay),
                    _contextNames.AccountsManagement);
                
                _chaosKitty.Meow(evt.OperationId);
                
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// All DailyPnls were calculated
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(DailyPnlsCalculatedEvent evt, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DailyPnlOperationData>(
                operationName: DailyPnlCommandsHandler.OperationName,
                id: evt.OperationId);

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(CommissionOperationState.Initiated,
                CommissionOperationState.Calculated))
            {
                var (total, _, notProcessed) = await _dailyPnlService.GetOperationState(evt.OperationId);

                if (total == 0)
                {
                    sender.SendCommand(new ChargeDailyPnlTimeoutInternalCommand
                    {
                        OperationId = evt.OperationId,
                        CreationTime = _systemClock.UtcNow.UtcDateTime,
                        TimeoutSeconds = 0,
                    }, _contextNames.CommissionService);
                }
                else if (notProcessed > 0)
                {
                    sender.SendCommand(new ChargeDailyPnlTimeoutInternalCommand
                    {
                        OperationId = evt.OperationId,
                        CreationTime = _systemClock.UtcNow.UtcDateTime,
                        TimeoutSeconds = _commissionServiceSettings.DailyPnlsChargingTimeoutSec,
                    }, _contextNames.CommissionService);
                }
                
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// DailyPnls failed, save the state
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(DailyPnlsStartFailedEvent evt, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DailyPnlOperationData>(
                operationName: DailyPnlCommandsHandler.OperationName,
                id: evt.OperationId);

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(CommissionOperationState.Initiated,
                CommissionOperationState.Failed))
            {
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// DailyPnls succeeded, save the state
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(DailyPnlsChargedEvent evt, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DailyPnlOperationData>(
                operationName: DailyPnlCommandsHandler.OperationName,
                id: evt.OperationId);

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(CommissionOperationState.Calculated,
                CommissionOperationState.Succeeded))
            {
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        private AccountBalanceChangeReasonTypeContract GetReasonType(CommissionType evtCommissionType)
        {
            switch (evtCommissionType)
            {
                case CommissionType.OrderExecution: return AccountBalanceChangeReasonTypeContract.Commission;
                case CommissionType.OnBehalf: return AccountBalanceChangeReasonTypeContract.OnBehalf;
                case CommissionType.OvernightSwap: return AccountBalanceChangeReasonTypeContract.Swap;
                case CommissionType.UnrealizedDailyPnl: return AccountBalanceChangeReasonTypeContract.UnrealizedDailyPnL;
                default:
                    throw new ArgumentOutOfRangeException(nameof(evtCommissionType), evtCommissionType, null);
            }
        }
    }
}













