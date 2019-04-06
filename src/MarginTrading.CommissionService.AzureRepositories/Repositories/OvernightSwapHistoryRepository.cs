using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using MarginTrading.CommissionService.AzureRepositories.Entities;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;

namespace MarginTrading.CommissionService.AzureRepositories.Repositories
{
    public class OvernightSwapHistoryRepository : IOvernightSwapHistoryRepository
    {
        private readonly INoSQLTableStorage<OvernightSwapEntity> _tableStorage;
		
        public OvernightSwapHistoryRepository(INoSQLTableStorage<OvernightSwapEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }
		
        public async Task AddAsync(IOvernightSwapCalculation obj)
        {
            var entity = OvernightSwapEntity.Create(obj);
            await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity, entity.Time);
        }

        public async Task BulkInsertAsync(List<IOvernightSwapCalculation> overnightSwapCalculations)
        {
            foreach (var overnightSwapCalculation in overnightSwapCalculations)
            {
                await AddAsync(overnightSwapCalculation);
            }
        }

        public async Task<IEnumerable<IOvernightSwapCalculation>> GetAsync()
        {
            return await _tableStorage.GetDataAsync();
        }

        public async Task<IReadOnlyList<IOvernightSwapCalculation>> GetAsync(DateTime? @from, DateTime? to)
        {
            return (await _tableStorage.WhereAsync(AzureStorageUtils.QueryGenerator<OvernightSwapEntity>.RowKeyOnly
                    .BetweenQuery(from ?? DateTime.MinValue, to ?? DateTime.MaxValue, ToIntervalOption.IncludeTo)))
                .OrderByDescending(item => item.Time)
                .ToList();
        }

        public async Task<IReadOnlyList<IOvernightSwapCalculation>> GetAsync(string accountId, DateTime? @from, DateTime? to)
        {
            return (await _tableStorage.WhereAsync(accountId, from ?? DateTime.MinValue, to ?? DateTime.MaxValue, 
                    ToIntervalOption.IncludeTo))
                .OrderByDescending(item => item.Time).ToList();
        }

        public async Task DeleteAsync(IOvernightSwapCalculation obj)
        {
            await _tableStorage.DeleteAsync(OvernightSwapEntity.Create(obj));
        }

        public async Task SetWasCharged(string positionOperationId, bool type)
        {
            var keys = OvernightSwapCalculation.ExtractKeysFromId(positionOperationId);
            var item = (await _tableStorage.GetDataAsync(x =>
                x.PositionId == keys.PositionId && x.OperationId == keys.OperationId)).First();
            await _tableStorage.ReplaceAsync(item, x =>
            {
                x.WasCharged = true;
                return x;
            });
        }

        public async Task<(int Total, int Failed, int NotProcessed)> GetOperationState(string operationId)
        {
            var items = await _tableStorage.GetDataAsync(x =>
                x.OperationId == operationId);

            return (items.Count, items.Count(x => !x.IsSuccess), items.Count(x => x == null));
        }
    }
}