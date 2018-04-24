using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.OvernightSwapService.Models.Abstractions;
using MarginTrading.OvernightSwapService.Storage.Entities;

namespace MarginTrading.OvernightSwapService.Storage.Implementation
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