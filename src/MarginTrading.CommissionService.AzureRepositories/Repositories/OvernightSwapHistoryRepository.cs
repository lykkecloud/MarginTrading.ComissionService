﻿using System;
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

        public async Task<bool> CheckOperationIsNew(string operationId)
        {
            return (await _tableStorage.GetDataAsync(x => x.OperationId == operationId)).Count == 0;
        }

        public async Task<bool> CheckPositionOperationIsNew(string positionOperationId)
        {//TODO very unoptimal. Optimize if Azure impl is used.
            return (await _tableStorage.GetDataAsync(x => x.Id == positionOperationId && x.WasCharged)).Count == 0;
        }

        public async Task DeleteAsync(IOvernightSwapCalculation obj)
        {
            await _tableStorage.DeleteAsync(OvernightSwapEntity.Create(obj));
        }

        public async Task SetWasCharged(string positionOperationId)
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
    }
}