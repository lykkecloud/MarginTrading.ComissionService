// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Tracing;

namespace Lykke.MarginTrading.CommissionService.Contracts.Models
{
    public class SharedCostsAndChargesCalculationContract
    {
        public SharedCostsAndChargesCalculationError Error { get; set; }
    }
}