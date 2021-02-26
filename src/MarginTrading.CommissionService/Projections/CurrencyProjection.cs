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
        private readonly IRateSettingsCache _rateSettingsCache;

        public CurrencyProjection(IRateSettingsCache rateSettingsCache)
        {
            _rateSettingsCache = rateSettingsCache;
        }

        [UsedImplicitly]
        public Task Handle(CurrencyChangedEvent @event)
        {
            return _rateSettingsCache.ClearOvernightSwapRatesCache();
        }
    }
}