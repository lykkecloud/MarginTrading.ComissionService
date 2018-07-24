using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
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
            //ensure operation idempotency
            var executionInfo = await _executionInfoRepository.GetAsync<ExecutedOrderOperationData>(
                operationName: OrderExecCommissionCommandsHandler.OperationName,
                id: evt.OperationId
            );

            if (SwitchState(executionInfo?.Data, CommissionOperationState.Started,
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
                        assetPairId: evt.AssetPairId),
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
            //ensure operation idempotency
            var executionInfo = await _executionInfoRepository.GetAsync<OnBehalfOperationData>(
                operationName: OnBehalfCommandsHandler.OperationName,
                id: evt.OperationId
            );
            
            if (SwitchState(executionInfo?.Data, CommissionOperationState.Started,
                CommissionOperationState.Calculated))
            {
                sender.SendCommand(new ChangeBalanceCommand(
                        operationId: evt.OperationId,
                        clientId: null,
                        accountId: evt.AccountId,
                        amount: -evt.Commission,
                        reasonType: AccountBalanceChangeReasonTypeContract.OnBehalf,
                        reason: nameof(OnBehalfCalculatedInternalEvent),
                        auditLog: null,
                        eventSourceId: evt.OrderId,
                        assetPairId: evt.AssetPairId),
                    _contextNames.AccountsManagement);
                
                _chaosKitty.Meow(evt.OperationId);

                await _executionInfoRepository.Save(executionInfo);
            }
        }
        
        /// <summary>
        /// Send charge command to AccountManagement service
        /// </summary>
        [UsedImplicitly]
        private void Handle(OvernightSwapCalculatedInternalEvent evt, ICommandSender sender)
        {
            if (!_overnightSwapService.CheckPositionOperationIsNew(evt.OperationId).GetAwaiter().GetResult())
            {
                _log.WriteInfo(nameof(ChargeCommissionSaga), nameof(Handle), 
                    $"Duplicate OvernightSwapCalculatedInternalEvent arrived with OperationId = {evt.OperationId}");
                return; //idempotency violated
            }
            
            sender.SendCommand(new ChangeBalanceCommand(
                    operationId: evt.OperationId,
                    clientId: null,
                    accountId: evt.AccountId, 
                    amount: evt.SwapAmount,
                    reasonType: AccountBalanceChangeReasonTypeContract.Swap, 
                    reason: nameof(OvernightSwapCalculatedInternalEvent), 
                    auditLog: null,
                    eventSourceId: evt.PositionId,
                    assetPairId: evt.AssetPairId),
                _contextNames.AccountsManagement);
            //todo what if Meow occurs here ?
            
            _overnightSwapService.SetWasCharged(evt.OperationId).GetAwaiter().GetResult();
        }
        
        /// <summary>
        /// Send charge command to AccountManagement service
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(DailyPnlCalculatedInternalEvent evt, ICommandSender sender)
        {
            //ensure operation idempotency
            var executionInfo = await _executionInfoRepository.GetAsync<DailyPnlOperationData>(
                operationName: DailyPnlCommandsHandler.OperationName,
                id: evt.OperationId
            );

            if (SwitchState(executionInfo?.Data, CommissionOperationState.Started,
                CommissionOperationState.Calculated))
            {
                sender.SendCommand(new ChangeBalanceCommand(
                        operationId: $"{evt.OperationId}_{evt.PositionId}",
                        clientId: null,
                        accountId: evt.AccountId,
                        amount: evt.Pnl,
                        reasonType: AccountBalanceChangeReasonTypeContract.UnrealizedDailyPnL,
                        reason: nameof(DailyPnlCalculatedInternalEvent),
                        auditLog: null,
                        eventSourceId: evt.PositionId,
                        assetPairId: evt.AssetPairId),
                    _contextNames.AccountsManagement);
                
                _chaosKitty.Meow(evt.OperationId);
                
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

        public static bool SwitchState(CommissionOperationData data, 
            CommissionOperationState expectedState, CommissionOperationState nextState)
        {
            if (data == null)
            {
                throw new InvalidOperationException("Operation execution data was not properly initialized.");
            }
            
            if (data.State < expectedState)
            {
                // Throws to retry and wait until the operation will be in the required state
                throw new InvalidOperationException(
                    $"Operation execution state can't be switched: {data.State} -> {nextState}. Waiting for the {expectedState} state.");
            }

            if (data.State > expectedState)
            {
                // Already in the next state, so this event can be just ignored
                return false;
            }

            data.State = nextState;

            return true;
        }
    }
}













