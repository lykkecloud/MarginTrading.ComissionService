// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using MarginTrading.AssetService.Contracts.LegacyAsset;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class Asset
    {
        public Asset(string id, string name, int accuracy)
        {
            Id = id;
            Name = name;
            Accuracy = accuracy;
        }

        public string Id { get; }
        public string Name { get; }
        public int Accuracy { get; }
        
        public List<ClientProfile> AvailableClientProfiles { get; }
    }
}