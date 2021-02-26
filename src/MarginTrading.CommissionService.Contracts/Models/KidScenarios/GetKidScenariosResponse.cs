// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace MarginTrading.CommissionService.Contracts.Models.KidScenarios
{
    public class GetKidScenariosResponse
    {
        public IReadOnlyList<KidScenarioContract> KidScenarios { get; set; }
    }
}