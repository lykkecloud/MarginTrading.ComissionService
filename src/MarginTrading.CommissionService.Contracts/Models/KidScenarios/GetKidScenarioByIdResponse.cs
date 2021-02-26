// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using MarginTrading.CommissionService.Contracts.Api;

namespace MarginTrading.CommissionService.Contracts.Models.KidScenarios
{
    public class GetKidScenarioByIdResponse : ErrorCodeResponse<KidScenariosErrorCodesContract>
    {
        public KidScenarioContract KidScenario { get; set; }
    }
}