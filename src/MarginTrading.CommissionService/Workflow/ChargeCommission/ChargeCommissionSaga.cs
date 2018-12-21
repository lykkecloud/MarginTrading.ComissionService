using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.MarginTrading.CommissionService.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Events;
using MarginTrading.CommissionService.Core.Workflow.DailyPnl.Events;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Events;
using MarginTrading.CommissionService.Core.Workflow.OvernightSwap.Events;
using MarginTrading.CommissionService.Workflow.OnBehalf;
using MarginTrading.CommissionService.Workflow.OvernightSwap;

namespace MarginTrading.CommissionService.Workflow.ChargeCommission
{
    internal class ChargeCommissionSaga
    {
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly IOvernightSwapService _overnightSwapService;
        private readonly ILog _log;
        private readonly IOperationExecutionInfoRepository _executionInfoRepository;
        private readonly IChaosKitty _chaosKitty;

        public ChargeCommissionSaga(CqrsContextNamesSettings contextNames,
            IOvernightSwapService overnightSwapService,
            ILog log,
            IOperationExecutionInfoRepository executionInfoRepository,
            IChaosKitty chaosKitty)
        {
            _contextNames = contextNames;
            _overnightSwapService = overnightSwapService;
            _log = log;
            _executionInfoRepository = executionInfoRepository;
            _chaosKitty = chaosKitty;
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
            if (!await _overnightSwapService.CheckPositionOperationIsNew(evt.OperationId))
            {
                _log.WriteInfo(nameof(ChargeCommissionSaga), nameof(Handle), 
                    $"Duplicate OvernightSwapCalculatedInternalEvent arrived with OperationId = {evt.OperationId}");
                return;
            }
            
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
            
            await _overnightSwapService.SetWasCharged(evt.OperationId);
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
                sender.SendCommand(new ChangeBalanceCommand(
                        operationId: evt.OperationId,
                        clientId: null,
                        accountId: evt.AccountId,
                        amount: evt.Pnl,
                        reasonType: AccountBalanceChangeReasonTypeContract.UnrealizedDailyPnL,
                        reason: $"Daily Pnl for account {evt.AccountId}",
                        auditLog: null,
                        eventSourceId: evt.PositionId,
                        assetPairId: evt.AssetPairId,
                        tradingDay: evt.TradingDay),
                    _contextNames.AccountsManagement);
                
                _chaosKitty.Meow(evt.OperationId);
                
                await _executionInfoRepository.Save(executionInfo);
            }
        }

        /// <summary>
        /// OvernightSwaps failed
        /// </summary>
        private async Task Handle(OvernightSwapsStartFailedEvent evt, ICommandSender sender)
        {
            //nothing to do
        }

        /// <summary>
        /// OvernightSwaps succeeded
        /// </summary>
        private async Task Handle(OvernightSwapsChargedEvent evt, ICommandSender sender)
        {
            //nothing to do
        }

        /// <summary>
        /// DailyPnls failed, save the state
        /// </summary>
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
        private async Task Handle(DailyPnlsChargedEvent evt, ICommandSender sender)
        {
            var executionInfo = await _executionInfoRepository.GetAsync<DailyPnlOperationData>(
                operationName: DailyPnlCommandsHandler.OperationName,
                id: evt.OperationId);

            if (executionInfo == null)
            {
                return;
            }

            if (executionInfo.Data.SwitchState(CommissionOperationState.Initiated,
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













