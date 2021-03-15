// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AssetService.Contracts.Enums;
using MarginTrading.AssetService.Contracts.Products;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain.CacheModels;
using MarginTrading.CommissionService.Core.Services;
using Microsoft.Extensions.Logging;

namespace MarginTrading.CommissionService.Projections
{
    public class ProductProjection
    {
        private readonly ICacheUpdater _cacheUpdater;
        private readonly IConvertService _convertService;
        private readonly IProductsCache _productsCache;
        private readonly ILogger<ProductProjection> _logger;
        private readonly IRateSettingsCache _rateSettingsCache;

        public ProductProjection(ICacheUpdater cacheUpdater,
            IConvertService convertService,
            IProductsCache productsCache,
            IRateSettingsCache rateSettingsCache,
            ILogger<ProductProjection> logger)
        {
            _cacheUpdater = cacheUpdater;
            _rateSettingsCache = rateSettingsCache;
            _convertService = convertService;
            _productsCache = productsCache;
            _logger = logger;
        }

        [UsedImplicitly]
        public async Task Handle(ProductChangedEvent @event)
        {
            switch (@event.ChangeType)
            {
                case ChangeType.Creation:
                case ChangeType.Edition:
                    if (!@event.NewValue.IsStarted) return;
                    _logger.LogInformation(
                        $"ProductChangedEvent received for productId: {@event.NewValue.ProductId}, upserting it in the product cache.");
                    _productsCache.AddOrUpdate(_convertService.Convert<ProductContract, ProductCacheModel>(@event.NewValue));
                    break;
                case ChangeType.Deletion:
                    if (!@event.OldValue.IsStarted) return;
                    _productsCache.Remove(_convertService.Convert<ProductContract, ProductCacheModel>(@event.OldValue));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _cacheUpdater.InitAssetPairs();
            _cacheUpdater.InitTradingInstruments();
            _cacheUpdater.InitOvernightSwapRates();
        }
    }
}