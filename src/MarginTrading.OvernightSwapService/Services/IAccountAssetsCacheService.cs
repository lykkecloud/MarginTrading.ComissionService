using System.Collections.Generic;
using MarginTrading.OvernightSwapService.Models.Abstractions;

namespace MarginTrading.OvernightSwapService.Services
{
    public interface IAccountAssetsCacheService
    {
        IAccountAssetPair GetAccountAsset(string tradingConditionId, string accountAssetId, string instrument);
        void InitAccountAssetsCache(List<IAccountAssetPair> accountAssets);
    }
}