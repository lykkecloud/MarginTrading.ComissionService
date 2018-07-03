using System.Linq;
using System.Threading.Tasks;
using Autofac;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.SettingsService.Contracts;
using MarginTrading.SettingsService.Contracts.AssetPair;
using MarginTrading.SettingsService.Contracts.Enums;
using MarginTrading.SettingsService.Contracts.Messages;

namespace MarginTrading.CommissionService.Services
{
    [UsedImplicitly]
    public class AssetPairsManager : IStartable, IAssetPairsManager
    {
        private static readonly object InitAssetPairsLock = new object();

        private readonly IAssetPairsInitializableCache _assetPairsCache;
        private readonly IAssetPairsApi _assetPairs;
        private readonly IConvertService _convertService;

        public AssetPairsManager(IAssetPairsInitializableCache assetPairsCache,
            IAssetPairsApi assetPairs,
            IConvertService convertService)
        {
            _assetPairsCache = assetPairsCache;
            _assetPairs = assetPairs;
            _convertService = convertService;
        }

        public void Start()
        {
            InitAssetPairs();
        }

        public void InitAssetPairs()
        {
            lock (InitAssetPairsLock)
            {
                var pairs = _assetPairs.List().GetAwaiter().GetResult()
                    .ToDictionary(a => a.Id,
                        s => (IAssetPair) _convertService.Convert<AssetPairContract, AssetPair>(s));
                _assetPairsCache.InitPairsCache(pairs);
            }
        }

        public Task HandleSettingsChanged(SettingsChangedEvent evt)
        {
            if (evt.SettingsType == SettingsTypeContract.AssetPair)
            {
                InitAssetPairs();
            }

            return Task.CompletedTask;
        }
    }
}