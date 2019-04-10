using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using MarginTrading.AccountsManagement.Contracts.Events;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Workflow
{
    [UsedImplicitly]
    internal class AccountListenerSaga
    {
        private readonly IOvernightSwapListener _overnightSwapListener;
        private readonly IDailyPnlListener _dailyPnlListener;
        private readonly IChaosKitty _chaosKitty;
        
        public AccountListenerSaga(
            IOvernightSwapListener overnightSwapListener,
            IDailyPnlListener dailyPnlListener,
            IChaosKitty chaosKitty)
        {
            _overnightSwapListener = overnightSwapListener;
            _dailyPnlListener = dailyPnlListener;
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
                    await _dailyPnlListener.DailyPnlStateChanged(evt.OperationId, true);
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
            
            await _dailyPnlListener.DailyPnlStateChanged(evt.OperationId, false);
        }
    }
}