// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Caches
{
    public interface IAssetsCache
    {
        void Initialize(Dictionary<string, Asset> data);

        int GetAccuracy(string id);

        string GetName(string id);
    }
}