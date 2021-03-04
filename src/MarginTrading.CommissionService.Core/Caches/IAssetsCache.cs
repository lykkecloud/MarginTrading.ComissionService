// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using MarginTrading.AssetService.Contracts.LegacyAsset;
using Asset = MarginTrading.CommissionService.Core.Domain.Asset;

namespace MarginTrading.CommissionService.Core.Caches
{
    public interface IAssetsCache
    {
        void Initialize(Dictionary<string, Asset> data);

        int GetAccuracy(string id);

        string GetName(string id);

        ClientProfile GetClientProfile(string assetId, string clientProfileId);
        Asset GetAsset(string id);
    }
}