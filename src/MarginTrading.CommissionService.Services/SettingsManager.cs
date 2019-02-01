using System.Linq;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.SettingsService.Contracts;
using MarginTrading.SettingsService.Contracts.Asset;
using MarginTrading.SettingsService.Contracts.AssetPair;
using MarginTrading.SettingsService.Contracts.Enums;
using MarginTrading.SettingsService.Contracts.Messages;

namespace MarginTrading.CommissionService.Services
{
    [UsedImplicitly]
    public class SettingsManager : IStartable, ISettingsManager
    {
        private static readonly object Lock = new object();

        private readonly IAssetPairsInitializableCache _assetPairsCache;
        private readonly IAssetsCache _assetsCache;
        private readonly IAssetPairsApi _assetPairs;
        private readonly IAssetsApi _assetsApi;
        private readonly IConvertService _convertService;

        public SettingsManager(
            IAssetPairsInitializableCache assetPairsCache,
            IAssetsCache assetsCache,
            IAssetPairsApi assetPairs,
            IAssetsApi assetsApi,
            IConvertService convertService)
        {
            _assetPairsCache = assetPairsCache;
            _assetsCache = assetsCache;
            _assetPairs = assetPairs;
            _assetsApi = assetsApi;
            _convertService = convertService;
        }

        public void Start()
        {
            InitAssetPairs();
            InitAssets();
        }

        private void InitAssetPairs()
        {
            lock (Lock)
            {
                var pairs = _assetPairs.List().GetAwaiter().GetResult()
                    .ToDictionary(a => a.Id,
                        s => (IAssetPair) _convertService.Convert<AssetPairContract, AssetPair>(s));
                _assetPairsCache.InitPairsCache(pairs);
            }
        }
        
        private void InitAssets()
        {
            lock (Lock)
            {
                var assets = _assetsApi.List().GetAwaiter().GetResult()
                    .ToDictionary(x => x.Id,
                        s => _convertService.Convert<AssetContract, Asset>(s));
                _assetsCache.Initialize(assets);
            }
        }

        public Task HandleSettingsChanged(SettingsChangedEvent evt)
        {
            if (evt.SettingsType == SettingsTypeContract.AssetPair)
            {
                InitAssetPairs();
            }
            else if (evt.SettingsType == SettingsTypeContract.Asset)
            {
                InitAssets();
            }

            return Task.CompletedTask;
        }
    }
}