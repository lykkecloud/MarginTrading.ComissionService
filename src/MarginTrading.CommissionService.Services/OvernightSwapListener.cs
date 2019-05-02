using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.MarginTrading.CommissionService.Contracts.Events;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Services;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Services
{
    [UsedImplicitly]
    public class OvernightSwapListener : IOvernightSwapListener
    {
        private readonly IOvernightSwapService _overnightSwapService;
        private readonly ICqrsMessageSender _cqrsMessageSender;
        private readonly ISystemClock _systemClock;

        public OvernightSwapListener(
            IOvernightSwapService overnightSwapService,
            ICqrsMessageSender cqrsMessageSender,
            ISystemClock systemClock)
        {
            _overnightSwapService = overnightSwapService;
            _cqrsMessageSender = cqrsMessageSender;
            _systemClock = systemClock;
        }
        
        public async Task OvernightSwapStateChanged(string operationId, bool chargedOrFailed)
        {
            if (await _overnightSwapService.SetWasCharged(operationId, chargedOrFailed) == 0)
            {
                return;
            }
            
            var (total, failed, notProcessed) = await _overnightSwapService.GetOperationState(operationId);

            if (total > 0 && notProcessed == 0)
            {
                _cqrsMessageSender.PublishEvent(new OvernightSwapsChargedEvent(
                    operationId: OvernightSwapCalculation.ExtractOperationId(operationId),
                    creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                    total: total,
                    failed: failed
                ));
            }
        }
    }
}