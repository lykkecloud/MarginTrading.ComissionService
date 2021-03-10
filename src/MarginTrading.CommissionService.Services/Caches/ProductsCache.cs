using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MarginTrading.AssetService.Contracts;
using MarginTrading.AssetService.Contracts.Products;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain.CacheModels;
using MarginTrading.CommissionService.Core.Services;
using Microsoft.Extensions.Logging;

namespace MarginTrading.CommissionService.Services.Caches
{
    public class ProductsCache : IProductsCache
    {
        private readonly IProductsApi _productsApi;
        private readonly IConvertService _convertService;
        private readonly ILogger<ProductsCache> _logger;

        private ConcurrentDictionary<string, ProductCacheModel> _cache =
            new ConcurrentDictionary<string, ProductCacheModel>();
        public ProductsCache(IProductsApi productsApi, IConvertService convertService, ILogger<ProductsCache> logger)
        {
            _productsApi = productsApi;
            _convertService = convertService;
            _logger = logger;
        }

        public void Initialize()
        {
            var response = _productsApi.GetAllAsync(new GetProductsRequest()
            {
                Skip = 0,
                Take = 0,
            }).GetAwaiter().GetResult();
            var products = response.Products
                .Select(x => _convertService.Convert<ProductContract, ProductCacheModel>(x))
                .ToList();

            foreach (var product in products)
            {
                _cache.AddOrUpdate(product.ProductId, product, (key, oldValue) => product);
            }
        }

        public List<ProductCacheModel> GetAll()
        {
            return _cache.Values.ToList();
        }

        public ProductCacheModel GetById(string productId)
        {
            var isInCache = _cache.TryGetValue(productId, out var result);
            if (!isInCache)
                _logger.LogWarning(nameof(ProductsCache), nameof(GetById),
                    $"Product with id {productId} is missing in cache");

            return result;
        }

        public void AddOrUpdate(ProductCacheModel product)
        {
            _logger.LogInformation($"Product cache update: {product.ProductId}");
            _cache.AddOrUpdate(product.ProductId, product, (key, oldValue) => product);
        }

        public void Remove(ProductCacheModel product)
        {
            _cache.TryRemove(product.ProductId, out _);
        }

        public int GetAccuracy(string accountBaseAssetId)
        {
            return 2;
        }

        public string GetName(string productId)
        {
            var product = GetById(productId);
            if (product == null) return productId;

            return product.Name;
        }

        public string GetIsin(string productId, bool isLong)
        {
            var product = GetById(productId);
            if (product == null)
                throw new Exception($"Product with id {productId} not found in cache, cannot get isin");

            return isLong ? product.IsinLong : product.IsinShort;
        }
    }
}