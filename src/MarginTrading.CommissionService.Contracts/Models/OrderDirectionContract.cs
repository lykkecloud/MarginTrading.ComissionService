// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace Lykke.MarginTrading.CommissionService.Contracts.Models
{
    public enum OrderDirectionContract
    {
        /// <summary>
        /// Order to buy the quoting asset of a pair
        /// </summary>
        Buy = 1,
        
        /// <summary>
        /// Order to sell the quoting asset of a pair
        /// </summary>
        Sell = 2
    }
}