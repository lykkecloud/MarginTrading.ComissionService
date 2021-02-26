// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain.CacheModels
{
    public class ClientProfileSettingsCacheModel
    {
        public string ClientProfileId { get; set; }
        public string AssetTypeId { get; set; }
        public decimal OnBehalfFee { get; set; }
    }
}