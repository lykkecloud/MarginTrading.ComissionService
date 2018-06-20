using Lykke.AzureStorage.Tables;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.AzureRepositories.Entities
{
   public class AccountAssetPairEntity : AzureTableEntity, IAccountAssetPair
    {
        public string TradingConditionId { get; set; }
        public string BaseAssetId { get; set; }
        public string Instrument => RowKey;
        public int LeverageInit { get; set; }
        public int LeverageMaintenance { get; set; }
        decimal IAccountAssetPair.SwapLong => (decimal) SwapLong;
        public double SwapLong { get; set; }
        decimal IAccountAssetPair.SwapShort => (decimal) SwapShort;
        public double SwapShort { get; set; }
        decimal IAccountAssetPair.OvernightSwapLong => (decimal) OvernightSwapLong;
        public double OvernightSwapLong { get; set; }
        decimal IAccountAssetPair.OvernightSwapShort => (decimal) OvernightSwapShort;
        public double OvernightSwapShort { get; set; }
        decimal IAccountAssetPair.CommissionLong => (decimal) CommissionLong;
        public double CommissionLong { get; set; }
        decimal IAccountAssetPair.CommissionShort => (decimal) CommissionShort;
        public double CommissionShort { get; set; }
        decimal IAccountAssetPair.CommissionLot => (decimal) CommissionLot;
        public double CommissionLot { get; set; }
        decimal IAccountAssetPair.DeltaBid => (decimal) DeltaBid;
        public double DeltaBid { get; set; }
        decimal IAccountAssetPair.DeltaAsk => (decimal) DeltaAsk;
        public double DeltaAsk { get; set; }
        decimal IAccountAssetPair.DealLimit => (decimal) DealLimit;
        public double DealLimit { get; set; }
        decimal IAccountAssetPair.PositionLimit => (decimal) PositionLimit;
        public double PositionLimit { get; set; }

        public static string GeneratePartitionKey(string tradingConditionId, string baseAssetId)
        {
            return $"{tradingConditionId}_{baseAssetId}";
        }

        public static string GenerateRowKey(string instrument)
        {
            return instrument;
        }

        public static AccountAssetPairEntity Create(IAccountAssetPair src)
        {
            return new AccountAssetPairEntity
            {
                PartitionKey = GeneratePartitionKey(src.TradingConditionId, src.BaseAssetId),
                RowKey = GenerateRowKey(src.Instrument),
                TradingConditionId = src.TradingConditionId,
                BaseAssetId = src.BaseAssetId,
                LeverageInit = src.LeverageInit,
                LeverageMaintenance = src.LeverageMaintenance,
                SwapLong = (double) src.SwapLong,
                SwapShort = (double) src.SwapShort,
                OvernightSwapLong = (double) src.OvernightSwapLong,
                OvernightSwapShort = (double) src.OvernightSwapShort,
                CommissionLong = (double) src.CommissionLong,
                CommissionShort = (double) src.CommissionShort,
                CommissionLot = (double) src.CommissionLot,
                DeltaBid = (double) src.DeltaBid,
                DeltaAsk = (double) src.DeltaAsk,
                DealLimit = (double) src.DealLimit,
                PositionLimit = (double) src.PositionLimit
            };
        }
    }
}