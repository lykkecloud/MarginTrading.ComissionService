using System;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Commands;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Commands;
using MarginTrading.CommissionService.Core.Workflow.ChargeCommission.Events;
using MarginTrading.CommissionService.Core.Workflow.DailyPnl.Events;
using MarginTrading.CommissionService.Core.Workflow.OnBehalf.Events;
using MarginTrading.CommissionService.Core.Workflow.OvernightSwap.Events;

namespace MarginTrading.CommissionService.Workflow.ChargeCommission
{
    internal class ChargeCommissionSaga
    {
        private readonly CqrsContextNamesSettings _contextNames;
        private readonly IAccountsApi _accountsApi;
        private readonly IOvernightSwapService _overnightSwapService;
        private readonly ILog _log;

        public ChargeCommissionSaga(CqrsContextNamesSettings contextNames,
            IAccountsApi accountsApi,
            IOvernightSwapService overnightSwapService,
            ILog log)
        {
            _contextNames = contextNames;
            _accountsApi = accountsApi;
            _overnightSwapService = overnightSwapService;
            _log = log;
        }

        /// <summary>
        /// Send charge command to AccountManagement service
        /// </summary>
        [UsedImplicitly]
        private void Handle(OrderExecCommissionCalculatedInternalEvent evt, ICommandSender sender)
        {
            //todo ensure operation idempotency
            //evt.OperationId
            
            sender.SendCommand(new ChangeBalanceCommand(
                operationId: evt.OperationId,
                clientId: null,
                accountId: evt.AccountId, 
                amount: - evt.Amount,
                reasonType: GetReasonType(evt.CommissionType), 
                reason: evt.Reason, 
                auditLog: null,
                eventSourceId: evt.OrderId,
                assetPairId: evt.AssetPairId),
                _contextNames.AccountsManagement);
        }

        /// <summary>
        /// Send charge command to AccountManagement service
        /// </summary>
        [UsedImplicitly]
        private void Handle(OnBehalfCalculatedInternalEvent evt, ICommandSender sender)
        {
            //todo ensure operation idempotency
            //evt.OperationId
            
            sender.SendCommand(new ChangeBalanceCommand(
                    operationId: evt.OperationId,
                    clientId: null,
                    accountId: evt.AccountId, 
                    amount: - evt.Commission,
                    reasonType: AccountBalanceChangeReasonTypeContract.OnBehalf, 
                    reason: nameof(OnBehalfCalculatedInternalEvent), 
                    auditLog: null,
                    eventSourceId: evt.OrderId,
                    assetPairId: evt.AssetPairId),
                _contextNames.AccountsManagement);
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
        private void Handle(DailyPnlCalculatedInternalEvent evt, ICommandSender sender)
        {
            //todo ensure operation idempotency
            //evt.OperationId
            
            sender.SendCommand(new ChangeBalanceCommand(
                    operationId: DailyPnlCalculation.GetId(evt.OperationId, evt.PositionId),
                    clientId: null,
                    accountId: evt.AccountId, 
                    amount: evt.Pnl,
                    reasonType: AccountBalanceChangeReasonTypeContract.UnrealizedDailyPnL, 
                    reason: nameof(DailyPnlCalculatedInternalEvent), 
                    auditLog: null,
                    eventSourceId: evt.PositionId,
                    assetPairId: evt.AssetPairId),
                _contextNames.AccountsManagement);
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













