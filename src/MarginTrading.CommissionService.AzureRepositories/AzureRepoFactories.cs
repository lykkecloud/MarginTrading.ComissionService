using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.CommissionService.AzureRepositories.Entities;
using MarginTrading.CommissionService.AzureRepositories.Repositories;

namespace MarginTrading.CommissionService.AzureRepositories
{
    public class AzureRepoFactories
    {
        public static class MarginTrading
        {
            public static MarginTradingBlobRepository CreateBlobRepository(IReloadingManager<string> connString)
            {
                return new MarginTradingBlobRepository(connString);
            }

            public static OvernightSwapHistoryRepository CreateOvernightSwapHistoryRepository(IReloadingManager<string> connString, ILog log)
            {
                return new OvernightSwapHistoryRepository(AzureTableStorage<OvernightSwapEntity>.Create(connString,
                    "OvernightSwapHistory", log));
            }
        }
    }
}