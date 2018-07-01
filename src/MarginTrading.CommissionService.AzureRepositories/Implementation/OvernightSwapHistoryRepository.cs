using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.CommissionService.AzureRepositories.Entities;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;

namespace MarginTrading.CommissionService.AzureRepositories.Implementation
{
    public class OvernightSwapHistoryRepository : IOvernightSwapHistoryRepository
    {
        private readonly INoSQLTableStorage<OvernightSwapEntity> _tableStorage;
		
        public OvernightSwapHistoryRepository(INoSQLTableStorage<OvernightSwapEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }
		
        public async Task AddAsync(IOvernightSwap obj)
        {
            var entity = OvernightSwapEntity.Create(obj);
            await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity, entity.Time);
        }

        public async Task<IEnumerable<IOvernightSwap>> GetAsync()
        {
            return await _tableStorage.GetDataAsync();
        }

        public async Task<IReadOnlyList<IOvernightSwap>> GetAsync(DateTime? @from, DateTime? to)
        {
            return (await _tableStorage.WhereAsync(AzureStorageUtils.QueryGenerator<OvernightSwapEntity>.RowKeyOnly
                    .BetweenQuery(from ?? DateTime.MinValue, to ?? DateTime.MaxValue, ToIntervalOption.IncludeTo)))
                .OrderByDescending(item => item.Time)
                .ToList();
        }

        public async Task<IReadOnlyList<IOvernightSwap>> GetAsync(string accountId, DateTime? @from, DateTime? to)
        {
            return (await _tableStorage.WhereAsync(accountId, from ?? DateTime.MinValue, to ?? DateTime.MaxValue, 
                    ToIntervalOption.IncludeTo))
                .OrderByDescending(item => item.Time).ToList();
        }

        public async Task DeleteAsync(IOvernightSwap obj)
        {
            await _tableStorage.DeleteAsync(OvernightSwapEntity.Create(obj));
        }
    }
}