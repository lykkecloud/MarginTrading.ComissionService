// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Lykke.MarginTrading.CommissionService.Contracts.Models
{
    public enum CommissionTypeContract
    {
        /// <summary>
        /// Commision applied to order execution
        /// </summary>
        OrderExecution = 1,
        
        /// <summary>
        /// On behalf fee applied to order execution
        /// </summary>
        OnBehalf = 2,
        
        /// <summary>
        /// Overnight fees applied to open position at EOD
        /// </summary>
        OvernightSwap = 3,
        
        /// <summary>
        /// Unrealized daily PnL is applied to account at EOD
        /// </summary>
        UnrealizedPnl = 4,
    }
}