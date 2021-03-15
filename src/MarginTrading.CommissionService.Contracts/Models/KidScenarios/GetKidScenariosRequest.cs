// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace MarginTrading.CommissionService.Contracts.Models.KidScenarios
{
    public class GetKidScenariosRequest
    {
        public List<string> Isins { get; set; }

        public int? Skip { get; set; }

        public int? Take { get; set; }
    }
}