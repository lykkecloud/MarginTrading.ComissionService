// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Lykke.Snow.Mdm.Contracts.Models.Events;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services.Handlers
{
    public class BrokerSettingsChangedHandler
    {
        private readonly ICacheUpdater _cacheUpdater;

        public BrokerSettingsChangedHandler(ICacheUpdater cacheUpdater)
        {
            _cacheUpdater = cacheUpdater;
        }

        public async Task Handle(BrokerSettingsChangedEvent @event)
        {
            _cacheUpdater.InitSchedules();
        }
    }
}