// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Settings
{
    public class OrderExecutionSettings
    {
        public decimal ExecutionFeesCap { get; set; }
        
        public decimal ExecutionFeesFloor { get; set; }
        
        public decimal ExecutionFeesRate { get; set; }
    }
}