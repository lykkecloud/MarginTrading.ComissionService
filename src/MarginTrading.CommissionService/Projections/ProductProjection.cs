// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AssetService.Contracts.Products;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Projections
{
    public class ProductProjection
    {
        private readonly ICacheUpdater _cacheUpdater;

        public ProductProjection(ICacheUpdater cacheUpdater)
        {
            _cacheUpdater = cacheUpdater;
        }

        [UsedImplicitly]
        public async Task Handle(ProductChangedEvent @event)
        {
            _cacheUpdater.InitAssetPairs();
            _cacheUpdater.InitAssets();
            _cacheUpdater.InitTradingInstruments();
            _cacheUpdater.InitOrderExecutionRates();
            _cacheUpdater.InitOvernightSwapRates();
        }
    }
}