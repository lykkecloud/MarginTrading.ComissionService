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

        public UnderlyingChangedHandler(ICacheUpdater cacheUpdater)
        {
            _cacheUpdater = cacheUpdater;
        }
        
        public async Task Handle(UnderlyingChangedEvent @event)
        {
            _cacheUpdater.InitTradingInstruments();
            _cacheUpdater.InitOvernightSwapRates();
        }
    }
}