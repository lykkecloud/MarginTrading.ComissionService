using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.OvernightSwapService.Models.Abstractions;
using MarginTrading.OvernightSwapService.Storage.Entities;

namespace MarginTrading.OvernightSwapService.Storage.Implementation
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