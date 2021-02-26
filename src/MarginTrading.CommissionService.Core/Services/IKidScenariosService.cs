// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Snow.Common.Model;
using MarginTrading.CommissionService.Core.Domain.KidScenarios;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IKidScenariosService
    {
        Task<Result<KidScenariosErrorCodes>> InsertAsync(KidScenario kidScenario);
        Task<Result<KidScenariosErrorCodes>> UpdateAsync(KidScenario kidScenario);
        Task<Result<KidScenariosErrorCodes>> DeleteAsync(string isin);
        Task<Result<KidScenario, KidScenariosErrorCodes>> GetByIdAsync(string isin);
        Task<Result<List<KidScenario>, KidScenariosErrorCodes>> GetAllAsync(string[] isins, int? skip, int? take);
    }
}