// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.CommissionService.Core.Domain
{
    public class CommissionHistory
    {
        public string OrderId { get; set; }
        public decimal? Commission { get; set; }
        
        /// <summary>
        /// Obsolete: not used; we're saving variables for Product Cost calculation  instead because spread is not known during HandleOrderExecInternalCommand
        /// </summary>
        [Obsolete]
        public decimal? ProductCost { get; set; }

        /// <summary>
        /// Used to calculate Product Cost
        /// </summary>
        public ProductCostCalculationData ProductCostCalculationData { get; set; }
    }
}