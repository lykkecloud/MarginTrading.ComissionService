using System;
using System.Linq;
using JetBrains.Annotations;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.CommissionService.Core.Domain.EventArgs;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Workflow
{
    [UsedImplicitly]
    internal class AccountListenerSaga
    {
        private readonly IEventChannel<DailyPnlChargedEventArgs> _dailyPnlChargedEventChannel;
        private readonly IEventChannel<OvernightSwapChargedEventArgs> _overnightSwapChargedEventChannel;
        
        public AccountListenerSaga(
            IEventChannel<DailyPnlChargedEventArgs> dailyPnlChargedEventChannel,
            IEventChannel<OvernightSwapChargedEventArgs> overnightSwapChargedEventChannel)
        {
            _dailyPnlChargedEventChannel = dailyPnlChargedEventChannel;
            _overnightSwapChargedEventChannel = overnightSwapChargedEventChannel;
        }

        /// <summary>
        /// Grab AccountChangedEvent's data for OvernightSwaps and DailyPnls
        /// </summary>
        [UsedImplicitly]
        private void Handle(AccountChangedEvent evt, ICommandSender sender)
        {
            if (evt.EventType != AccountChangedEventTypeContract.BalanceUpdated
                || evt.BalanceChange == null)
            {
                return;
            }

            switch (evt.BalanceChange.ReasonType)
            {
                case AccountBalanceChangeReasonTypeContract.Swap:
                    _overnightSwapChargedEventChannel.SendEvent(this, new OvernightSwapChargedEventArgs
                    {
                        OperationId = evt.BalanceChange.Id,
                    });
                    break;
                case AccountBalanceChangeReasonTypeContract.UnrealizedDailyPnL:
                    _dailyPnlChargedEventChannel.SendEvent(this, new DailyPnlChargedEventArgs
                    {
                        OperationId = evt.BalanceChange.Id,
                    });
                    break;
            }
        }
    }
}