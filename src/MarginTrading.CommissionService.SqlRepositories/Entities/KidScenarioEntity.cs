// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.CommissionService.SqlRepositories.Entities
{
    public class KidScenarioEntity
    {
        public string Isin { get; set; }

        public decimal KidModerateScenario { get; set; }
        public decimal KidModerateScenarioAvreturn { get; set; }

        public DateTime Timestamp { get; set; }
    }
}