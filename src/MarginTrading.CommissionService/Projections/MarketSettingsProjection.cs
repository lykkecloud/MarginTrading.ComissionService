// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AssetService.Contracts.MarketSettings;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Projections
{
    public class MarketSettingsProjection
    {
        private readonly ICacheUpdater _cacheUpdater;

        public MarketSettingsProjection(ICacheUpdater cacheUpdater)
        {
            _cacheUpdater = cacheUpdater;
        }

        [UsedImplicitly]
        public async Task Handle(MarketSettingsChangedEvent @event)
        {
            _cacheUpdater.InitSchedules();
        }
    }
}