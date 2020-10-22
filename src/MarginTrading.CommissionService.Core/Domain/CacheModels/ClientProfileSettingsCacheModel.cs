// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Domain.CacheModels
{
    public class ClientProfileSettingsCacheModel
    {
        public string RegulatoryProfileId { get; set; }
        public string RegulatoryTypeId { get; set; }
        public string ClientProfileId { get; set; }
        public string AssetTypeId { get; set; }
        public decimal Margin { get; set; }
        public decimal ExecutionFeesFloor { get; set; }
        public decimal ExecutionFeesCap { get; set; }
        public decimal ExecutionFeesRate { get; set; }
        public decimal FinancingFeesRate { get; set; }
        public decimal OnBehalfFee { get; set; }
        public bool IsAvailable { get; set; }
    }
}