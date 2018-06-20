using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.CommissionService.AzureRepositories.Entities;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;

namespace MarginTrading.CommissionService.AzureRepositories.Implementation
{
    public class OvernightSwapStateRepository : IOvernightSwapStateRepository
    {
        private readonly INoSQLTableStorage<OvernightSwapStateEntity> _tableStorage;
		
        public OvernightSwapStateRepository(INoSQLTableStorage<OvernightSwapStateEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }
		
        public async Task AddOrReplaceAsync(IOvernightSwapState obj)
        {
            await _tableStorage.InsertOrReplaceAsync(OvernightSwapStateEntity.Create(obj));
        }

        public async Task<IEnumerable<IOvernightSwapState>> GetAsync()
        {
            return await _tableStorage.GetDataAsync();
        }

        public async Task DeleteAsync(IOvernightSwapState obj)
        {
            var entity = OvernightSwapStateEntity.Create(obj);
            await _tableStorage.DeleteIfExistAsync(entity.PartitionKey, entity.RowKey);
        }
    }
}