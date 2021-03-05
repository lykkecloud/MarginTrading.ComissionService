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

namespace MarginTrading.CommissionService.Workflow
{
    public class ProductChangedProjection
    {
        private readonly IConvertService _convertService;
        private readonly IProductsCache _productsCache;
        private readonly ILogger<ProductChangedProjection> _logger;

        public ProductChangedProjection(
            IConvertService convertService,
            IProductsCache productsCache,
            ILogger<ProductChangedProjection> logger)
        {
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
                    _logger.LogInformation(
                        $"ProductChangedEvent received for productId: {@event.NewValue.ProductId}, upserting it in the product cache.");
                    _productsCache.AddOrUpdate(_convertService.Convert<ProductContract, ProductCacheModel>(@event.NewValue));
                    break;
                case ChangeType.Deletion:
                    _logger.LogInformation(
                        $"ProductChangedEvent with deleted product received for productId: {@event.OldValue.ProductId}, deleting it from the product cache.");
                    _productsCache.Remove(_convertService.Convert<ProductContract, ProductCacheModel>(@event.OldValue));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}