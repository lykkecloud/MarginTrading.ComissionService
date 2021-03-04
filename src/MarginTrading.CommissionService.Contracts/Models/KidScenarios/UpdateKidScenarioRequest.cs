// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Contracts.Models.KidScenarios
{
    public class UpdateKidScenarioRequest
    {
        public decimal? KidModerateScenario { get; set; }
        
        public decimal? KidModerateScenarioAvreturn { get; set; }
    }
}