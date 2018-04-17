using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;
using MarginTrading.OvernightSwapService.Infrastructure.Implementation;
using MarginTrading.OvernightSwapService.Storage.Entities;
using MarginTrading.OvernightSwapService.Storage.Implementation;

namespace MarginTrading.OvernightSwapService.Storage
{
    public class AzureRepoFactories
    {
        public static class MarginTrading
        {
            public static MarginTradingBlobRepository CreateBlobRepository(IReloadingManager<string> connString)
            {
                return new MarginTradingBlobRepository(connString);
            }

            public static OvernightSwapStateRepository CreateOvernightSwapStateRepository(IReloadingManager<string> connString, ILog log)
            {
                return new OvernightSwapStateRepository(AzureTableStorage<OvernightSwapStateEntity>.Create(connString,
                    "OvernightSwapState", log));
            }

            public static OvernightSwapHistoryRepository CreateOvernightSwapHistoryRepository(IReloadingManager<string> connString, ILog log)
            {
                return new OvernightSwapHistoryRepository(AzureTableStorage<OvernightSwapHistoryEntity>.Create(connString,
                    "OvernightSwapHistory", log));
            }

            public static AccountAssetPairsRepository CreateAccountAssetsRepository(IReloadingManager<string> connString, ILog log)
            {
                return new AccountAssetPairsRepository(AzureTableStorage<AccountAssetPairEntity>.Create(connString,
                    "MarginTradingAccountAssets", log));
            }
        }
    }
}