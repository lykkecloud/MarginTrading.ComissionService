using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
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
        private readonly IOvernightSwapListener _overnightSwapListener;
        private readonly IEventChannel<DailyPnlChargedEventArgs> _dailyPnlChargedEventChannel;

        private readonly IChaosKitty _chaosKitty;
        
        public AccountListenerSaga(
            IOvernightSwapListener overnightSwapListener,
            IEventChannel<DailyPnlChargedEventArgs> dailyPnlChargedEventChannel,
            IChaosKitty chaosKitty)
        {
            _overnightSwapListener = overnightSwapListener;
            _dailyPnlChargedEventChannel = dailyPnlChargedEventChannel;
            _chaosKitty = chaosKitty;
        }

        /// <summary>
        /// Grab AccountChangedEvent's data for OvernightSwaps and DailyPnls
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AccountChangedEvent evt, ICommandSender sender)
        {
            if (evt.EventType != AccountChangedEventTypeContract.BalanceUpdated
                || evt.BalanceChange == null)
            {
                return;
            }

            switch (evt.BalanceChange.ReasonType)
            {
                case AccountBalanceChangeReasonTypeContract.Swap:
                    await _overnightSwapListener.OvernightSwapStateChanged(evt.OperationId, true);
                    break;
                case AccountBalanceChangeReasonTypeContract.UnrealizedDailyPnL:
                    _dailyPnlChargedEventChannel.SendEvent(this, new DailyPnlChargedEventArgs
                    {
                        OperationId = evt.BalanceChange.Id,
                    });
                    break;
            }
            
            _chaosKitty.Meow(evt.BalanceChange.Id);
        }

        /// <summary>
        /// Grab AccountBalanceChangeFailedEvents for OvernightSwapListener
        /// </summary>
        [UsedImplicitly]
        private async Task Handle(AccountBalanceChangeFailedEvent evt, ICommandSender sender)
        {
            await _overnightSwapListener.OvernightSwapStateChanged(evt.OperationId, false);
            
            _chaosKitty.Meow(evt.OperationId);
        }
    }
}