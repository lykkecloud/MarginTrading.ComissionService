namespace MarginTrading.OvernightSwapService.Services
{
    public interface ICfdCalculatorService
    {
        decimal GetQuoteRateForBaseAsset(string accountAssetId, string instrument, string legalEntity, 
            bool metricIsPositive = true);
        decimal GetQuoteRateForQuoteAsset(string accountAssetId, string instrument, string legalEntity, 
            bool metricIsPositive = true);
    }
}