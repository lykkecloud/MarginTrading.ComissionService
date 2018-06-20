using System.Collections.Generic;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IAccountAssetsCacheService
    {
        IAccountAssetPair GetAccountAsset(string tradingConditionId, string accountAssetId, string instrument);
        void InitAccountAssetsCache(List<IAccountAssetPair> accountAssets);
    }
}