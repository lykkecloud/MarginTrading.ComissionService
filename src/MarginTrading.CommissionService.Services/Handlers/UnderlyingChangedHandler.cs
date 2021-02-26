// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Lykke.Snow.Mdm.Contracts.Models.Events;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services.Handlers
{
    public class UnderlyingChangedHandler
    {
        private readonly ICacheUpdater _cacheUpdater;
        private readonly IRateSettingsCache _rateSettingsCache;

        public UnderlyingChangedHandler(ICacheUpdater cacheUpdater, IRateSettingsCache rateSettingsCache)
        {
            _cacheUpdater = cacheUpdater;
            _rateSettingsCache = rateSettingsCache;
        }
        
        public async Task Handle(UnderlyingChangedEvent @event)
        {
            _cacheUpdater.InitTradingInstruments();
            await _rateSettingsCache.ClearOvernightSwapRatesCache();
        }
    }
}