// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Common.Log;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Services
{
    [UsedImplicitly]
    public class EventSender : IEventSender
    {
        private readonly ILog _log;
        private readonly ISystemClock _systemClock;

        public EventSender(
            IRabbitMqService rabbitMqService,
            ILog log,
            ISystemClock systemClock,
            RabbitMqSettings rabbitMqSettings)
        {
            _log = log;
            _systemClock = systemClock;
        }
    }
}