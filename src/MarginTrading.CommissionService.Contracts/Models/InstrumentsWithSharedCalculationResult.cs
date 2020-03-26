// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Lykke.MarginTrading.CommissionService.Contracts.Models
{
    public class InstrumentsWithSharedCalculationResult
    {
        public List<string> InstrumentIds { get; set; }
    }
}