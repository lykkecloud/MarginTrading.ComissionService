using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Settings;
using Microsoft.Extensions.Configuration;
using Lykke.SettingsReader;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain.Rates;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Services;
using MarginTrading.CommissionService.SqlRepositories.Repositories;
using MoreLinq;
using StackExchange.Redis;

namespace Migration
{
    internal static class Program
    {
        private const string MigrationsContainer = "Migrations";
        private const string MigrationsKey = "CommissionService";
        
        private static async Task Main(string[] args)
        {
            var operationId = args.FirstOrDefault(x => x.Length > 0) ?? Guid.NewGuid().ToString();

            try
            {
                await Migrate(operationId);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"[ERR] {exception.Message}: {exception.StackTrace}");
            }
        }

        private static async Task Migrate(string operationId)
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            var appSettings = configuration.LoadSettings<AppSettings>().CurrentValue;
            var blobRepo = new SqlBlobRepository(appSettings.CommissionService.Db.StateConnString);
            var redisDatabase = ConnectionMultiplexer.Connect(
                appSettings.CommissionService.RedisSettings.Configuration).GetDatabase();
            
            var migrationState = await blobRepo.ReadAsync<List<Migration>>(MigrationsContainer, MigrationsKey) ?? new List<Migration>();
            var lastState = migrationState.Any() ? migrationState.MaxBy(x => x.Time) : null;
            var migration = new Migration
            {
                Id = operationId,
                Time = DateTime.UtcNow,
                State = MigrationState.Started,
            };
            
            if (lastState != null && lastState.Id == operationId)
            {
                if (lastState.State != MigrationState.Finished)
                {
                    Console.WriteLine($"[WRN] Last migration #{operationId} {lastState.Time:s} was not successful! The process goes forward.");
                }
                else
                {
                    Console.WriteLine($"[WRN] Migration #{operationId} {lastState.Time:s} already finished!");
                    Environment.Exit(1);
                }
            }
            else
            {
                migrationState.Add(migration);
                await blobRepo.WriteAsync(MigrationsContainer, MigrationsKey, migrationState);
            }

            var (orderExecutionRates, onBehalfRates) = await PrepareData(blobRepo);
            
            //migrate SQL
            await blobRepo.WriteAsync(LykkeConstants.RateSettingsBlobContainer, LykkeConstants.OrderExecutionKey,
                orderExecutionRates);
            await blobRepo.WriteAsync(LykkeConstants.RateSettingsBlobContainer, LykkeConstants.OnBehalfKey, onBehalfRates);

            //migrate Redis
            await redisDatabase.KeyDeleteAsync(RateSettingsService.GetKey(LykkeConstants.OrderExecutionKey));
            await redisDatabase.HashSetAsync(RateSettingsService.GetKey(LykkeConstants.OrderExecutionKey),
                orderExecutionRates.Select(x => new HashEntry(
                    RateSettingsService.GetOrderExecId(x.TradingConditionId, x.AssetPairId),
                    RateSettingsService.Serialize(x))).ToArray());
            
            await redisDatabase.KeyDeleteAsync(RateSettingsService.GetKey(LykkeConstants.OnBehalfKey));
            await redisDatabase.HashSetAsync(RateSettingsService.GetKey(LykkeConstants.OnBehalfKey),
                onBehalfRates.Select(x => new HashEntry(x.TradingConditionId,
                    RateSettingsService.Serialize(x))).ToArray());

            migration.Time = DateTime.UtcNow;
            migration.State = MigrationState.Finished;
            migrationState.RemoveAll(x => x.Id == operationId);
            migrationState.Add(migration);
            await blobRepo.WriteAsync(MigrationsContainer, MigrationsKey, migrationState);
        }

        private static async Task<(List<OrderExecutionRate>, List<OnBehalfRate>)> PrepareData(SqlBlobRepository blobRepo)
        {
            var orderExecData = (await blobRepo.ReadAsync<IEnumerable<OrderExecutionRate>>(
                blobContainer: LykkeConstants.RateSettingsBlobContainer,
                key: LykkeConstants.OrderExecutionKey
            ))?.ToList();
            var oldOnBehalf = await blobRepo.ReadAsync<OnBehalfRate>(
                blobContainer: LykkeConstants.RateSettingsBlobContainer,
                key: LykkeConstants.OnBehalfKey
            );
            var onBehalfData = new List<OnBehalfRate>();
            if (oldOnBehalf != null)
            {
                onBehalfData.Add(oldOnBehalf);
            }

            foreach (var orderExecutionRate in orderExecData ?? new List<OrderExecutionRate>())
            {
                orderExecutionRate.TradingConditionId = string.Empty;
            }

            foreach (var onBehalfRate in onBehalfData)
            {
                onBehalfRate.TradingConditionId = string.Empty;
            }

            return (orderExecData, onBehalfData);
        }
    }
}