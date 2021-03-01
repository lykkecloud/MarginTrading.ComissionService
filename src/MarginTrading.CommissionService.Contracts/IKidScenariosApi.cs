// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.CommissionService.Contracts.Api;
using MarginTrading.CommissionService.Contracts.Models.KidScenarios;
using Refit;

namespace MarginTrading.CommissionService.Contracts
{
    public interface IKidScenariosApi
    {
        [Post("/api/kid-scenarios")]
        Task<ErrorCodeResponse<KidScenariosErrorCodesContract>> AddAsync([Body] AddKidScenarioRequest request);

        [Put("/api/kid-scenarios/{isin}")]
        Task<ErrorCodeResponse<KidScenariosErrorCodesContract>> UpdateAsync(string isin,
            [Body] UpdateKidScenarioRequest request);

        [Delete("/api/kid-scenarios/{isin}")]
        Task<ErrorCodeResponse<KidScenariosErrorCodesContract>> DeleteAsync(string isin);

        [Get("/api/kid-scenarios/{isin}")]
        Task<GetKidScenarioByIdResponse> GetByIdAsync(string isin);

        [Post("/api/kid-scenarios/list")]
        Task<GetKidScenariosResponse> GetAllAsync([Body] GetKidScenariosRequest request);
    }
}