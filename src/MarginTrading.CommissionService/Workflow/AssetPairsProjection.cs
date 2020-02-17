using Common.Log;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.SettingsService.Contracts.AssetPair;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MarginTrading.CommissionService.Workflow
{
    /// <summary>
    /// Listens to <see cref="AssetPairChangedEvent"/>s and builds a projection inside of the
    /// <see cref="IAssetPairsCache"/>
    /// </summary>
    [UsedImplicitly]
    public class AssetPairsProjection
    {
        private readonly IAssetPairsCache _assetPairsCache;
        private readonly ILog _log;
        private readonly IConvertService _convertService;

        public AssetPairsProjection(
            IAssetPairsCache assetPairsCache,
            ILog log,
            IConvertService convertService)
        {
            _assetPairsCache = assetPairsCache;
            _log = log;
            _convertService = convertService;
        }

        [UsedImplicitly]
        public async Task Handle(AssetPairChangedEvent @event)
        {
            //deduplication is not required, it's ok if an object is updated multiple times
            if (@event.AssetPair?.Id == null)
            {
                await _log.WriteWarningAsync(nameof(AssetPairsProjection), nameof(Handle),
                    "AssetPairChangedEvent contained no asset pair id");
                return;
            }

            if (IsDelete(@event))
            {
                _assetPairsCache.Remove(@event.AssetPair.Id);
            }
            else
            {
                _assetPairsCache.AddOrUpdate(_convertService.Convert<AssetPairContract, AssetPair>(@event.AssetPair));
            }
        }

        private static bool IsDelete(AssetPairChangedEvent @event)
        {
            return @event.AssetPair.BaseAssetId == null || @event.AssetPair.QuoteAssetId == null;
        }
    }
}