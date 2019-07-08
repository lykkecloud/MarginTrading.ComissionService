// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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
    public class DailyPnlListener : IDailyPnlListener
    {
        private readonly IDailyPnlService _dailyPnlService;
        private readonly ICqrsMessageSender _cqrsMessageSender;
        private readonly ISystemClock _systemClock;

        public DailyPnlListener(
            IDailyPnlService dailyPnlService,
            ICqrsMessageSender cqrsMessageSender,
            ISystemClock systemClock)
        {
            _dailyPnlService = dailyPnlService;
            _cqrsMessageSender = cqrsMessageSender;
            _systemClock = systemClock;
        }
        
        public async Task DailyPnlStateChanged(string operationId, bool chargedOrFailed)
        {
            if (await _dailyPnlService.SetWasCharged(operationId, chargedOrFailed) == 0)
            {
                return;
            }
            
            var (total, failed, notProcessed) = await _dailyPnlService.GetOperationState(operationId);

            if (total > 0 && notProcessed == 0)
            {
                _cqrsMessageSender.PublishEvent(new DailyPnlsChargedEvent(
                    operationId: DailyPnlCalculation.ExtractOperationId(operationId),
                    creationTimestamp: _systemClock.UtcNow.UtcDateTime,
                    total: total,
                    failed: failed
                ));
            }
        }
    }
}