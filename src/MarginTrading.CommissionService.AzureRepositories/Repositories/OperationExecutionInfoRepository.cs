using System;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.CommissionService.AzureRepositories.Entities;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MarginTrading.CommissionService.AzureRepositories.Repositories
{
    public class OperationExecutionInfoRepository : IOperationExecutionInfoRepository
    {
        private readonly INoSQLTableStorage<OperationExecutionInfoEntity> _tableStorage;
        private readonly ILog _log;
        private readonly bool _enableOperationsLogs = true;

        public OperationExecutionInfoRepository(IReloadingManager<string> connStr, ILog log)
        {
            _tableStorage = AzureTableStorage<OperationExecutionInfoEntity>.Create(
                connStr,
                "CommissionExecutionInfo",
                log);
            _log = log.CreateComponentScope(nameof(OperationExecutionInfoRepository));
        }
        
        public async Task<(bool existed, IOperationExecutionInfo<TData> data)> GetOrAddAsync<TData>(
            string operationName, string operationId, Func<IOperationExecutionInfo<TData>> factory) where TData : class
        {
            var existed = true;
            var entity = await _tableStorage.GetOrInsertAsync(
                partitionKey: OperationExecutionInfoEntity.GeneratePartitionKey(operationName),
                rowKey: OperationExecutionInfoEntity.GeneratePartitionKey(operationId),
                createNew: () =>
                {
                    existed = false;
                    return Convert(factory());
                });
                
            return (existed, Convert<TData>(entity));
        }

        public async Task<IOperationExecutionInfo<TData>> GetAsync<TData>(string operationName, string id)
            where TData : class
        {
            var obj = await _tableStorage.GetDataAsync(
                          OperationExecutionInfoEntity.GeneratePartitionKey(operationName),
                          OperationExecutionInfoEntity.GenerateRowKey(id)) ?? throw new InvalidOperationException(
                          $"Operation execution info for {operationName} #{id} not yet exists");
            
            return Convert<TData>(obj);
        }

        public async Task Save<TData>(IOperationExecutionInfo<TData> executionInfo) where TData : class
        {
            await _tableStorage.ReplaceAsync(Convert(executionInfo));
        }

        private static IOperationExecutionInfo<TData> Convert<TData>(OperationExecutionInfoEntity entity)
            where TData : class
        {
            return new OperationExecutionInfo<TData>(
                operationName: entity.OperationName,
                id: entity.Id,
                data: entity.Data is string dataStr
                    ? JsonConvert.DeserializeObject<TData>(dataStr)
                    : ((JToken) entity.Data).ToObject<TData>());
        }

        private static OperationExecutionInfoEntity Convert<TData>(IOperationExecutionInfo<TData> model)
            where TData : class
        {
            return new OperationExecutionInfoEntity
            {
                Id = model.Id,
                OperationName = model.OperationName,
                Data = model.Data.ToJson(),
            };
        }
    }
}