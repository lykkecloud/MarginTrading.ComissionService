// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AssetService.Contracts.Enums;
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
            switch (@event.ChangeType)
            {
                case ChangeType.Creation:
                case ChangeType.Edition:
                    if (!@event.NewValue.IsStarted) return;
                    break;
                case ChangeType.Deletion:
                    if (!@event.OldValue.IsStarted) return;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            _cacheUpdater.InitAssetPairs();
            _cacheUpdater.InitAssets();
            _cacheUpdater.InitTradingInstruments();
            _cacheUpdater.InitOrderExecutionRates();
            _cacheUpdater.InitOvernightSwapRates();
        }
    }
}