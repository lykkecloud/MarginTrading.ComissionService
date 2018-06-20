using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.CommissionService.AzureRepositories.Entities;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;

namespace MarginTrading.CommissionService.AzureRepositories.Implementation
{
    public class AccountAssetPairsRepository : IAccountAssetPairsRepository
    {
        private readonly INoSQLTableStorage<AccountAssetPairEntity> _tableStorage;

        public AccountAssetPairsRepository(INoSQLTableStorage<AccountAssetPairEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }
        
        public async Task<IEnumerable<IAccountAssetPair>> GetAllAsync()
        {
            return await _tableStorage.GetDataAsync();
        }
    }
}