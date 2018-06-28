using System;
using JetBrains.Annotations;

namespace MarginTrading.CommissionService.Core.Domain.Rates
{
    public class OrderExecution
    {
        public string AssetPairId { get; }
        
        public decimal CommissionCap { get; }
        
        public decimal CommissionFloor { get; }
        
        public decimal CommissionRate { get; }
        
        public string CommissionAsset { get; }

        public OrderExecution(string assetPairId, decimal commissionCap, decimal commissionFloor, 
            decimal commissionRate, [NotNull] string commissionAsset)
        {
            AssetPairId = assetPairId ?? throw new ArgumentNullException(nameof(assetPairId));
            CommissionCap = commissionCap;
            CommissionFloor = commissionFloor;
            CommissionRate = commissionRate;
            CommissionAsset = commissionAsset ?? throw new ArgumentNullException(nameof(commissionAsset));
        }
    }
}