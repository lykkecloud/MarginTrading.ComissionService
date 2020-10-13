// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain.CacheModels
{
    public class ClientProfileCacheModel
    {
        public string Id { get; set; }

        public string RegulatoryProfileId { get; set; }

        public bool IsDefault { get; set; }
    }
}