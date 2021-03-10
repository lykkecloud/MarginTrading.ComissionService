// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AssetService.Contracts.Currencies;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Projections
{
    public class CurrencyProjection
    {
        private readonly ICacheUpdater _cacheUpdater;

        public CurrencyProjection(ICacheUpdater cacheUpdater)
        {
            _cacheUpdater = cacheUpdater;
        }

        [UsedImplicitly]
        public Task Handle(CurrencyChangedEvent @event)
        {
            _cacheUpdater.InitOvernightSwapRates();
            
            return Task.CompletedTask;
        }
    }
}