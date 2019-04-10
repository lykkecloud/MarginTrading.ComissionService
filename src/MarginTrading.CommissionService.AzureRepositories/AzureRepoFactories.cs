using AzureStorage.Tables;
using Common.Log;
using JetBrains.Annotations;
using Lykke.SettingsReader;
using MarginTrading.CommissionService.AzureRepositories.Entities;
using MarginTrading.CommissionService.AzureRepositories.Repositories;
using MarginTrading.CommissionService.Core.Repositories;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.AzureRepositories
{
    [UsedImplicitly]
    public class AzureRepoFactories
    {
        public static class MarginTrading
        {
            public static MarginTradingBlobRepository CreateBlobRepository(IReloadingManager<string> connString)
            {
                return new MarginTradingBlobRepository(connString);
            }

            public static OvernightSwapHistoryRepository CreateOvernightSwapHistoryRepository(
                IReloadingManager<string> connString, ILog log)
            {
                return new OvernightSwapHistoryRepository(AzureTableStorage<OvernightSwapEntity>.Create(connString,
                    "OvernightSwapHistory", log));
            }
            
            public static DailyPnlHistoryRepository CreateDailyPnlHistoryRepository(
                IReloadingManager<string> connString, ILog log)
            {
                return new DailyPnlHistoryRepository(AzureTableStorage<DailyPnlEntity>.Create(connString,
                    "DailyPnlHistory", log));
            }

            public static InterestRatesRepository CreateInterestRatesRepository(IReloadingManager<string> connString,
                ILog log)
            {
                return new InterestRatesRepository(AzureTableStorage<InterestRateEntity>.Create(connString,
                    "ClosingInterestRates", log));
            }

            public static OperationExecutionInfoRepository CreateOperationExecutionInfoRepository(
                IReloadingManager<string> connString, ILog log, ISystemClock systemClock)
            {
                return new OperationExecutionInfoRepository(connString, log, systemClock);
            }
        }
    }
}