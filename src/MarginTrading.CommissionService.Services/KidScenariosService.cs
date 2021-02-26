// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Snow.Common.Model;
using MarginTrading.CommissionService.Core.Domain.KidScenarios;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class KidScenariosService : IKidScenariosService
    {
        private readonly IKidScenariosRepository _repository;

        public KidScenariosService(IKidScenariosRepository repository)
        {
            _repository = repository;
        }

        public Task<Result<KidScenariosErrorCodes>> InsertAsync(KidScenario kidScenario)
            => _repository.InsertAsync(kidScenario);

        public Task<Result<KidScenariosErrorCodes>> UpdateAsync(KidScenario kidScenario)
            => _repository.UpdateAsync(kidScenario);

        public Task<Result<KidScenariosErrorCodes>> DeleteAsync(string isin)
            => _repository.DeleteAsync(isin);

        public Task<Result<KidScenario, KidScenariosErrorCodes>> GetByIdAsync(string isin)
            => _repository.GetByIdAsync(isin);

        public Task<Result<List<KidScenario>, KidScenariosErrorCodes>> GetAllAsync(string[] isins, int? skip, int? take)
            => _repository.GetAllAsync(isins, skip, take);
    }
}