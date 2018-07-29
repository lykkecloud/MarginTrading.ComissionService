namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICfdCalculatorService
    {
        decimal GetQuoteRateForBaseAsset(string accountAssetId, string instrument, string legalEntity, 
            bool metricIsPositive = true);
        decimal GetQuoteRateForQuoteAsset(string accountAssetId, string instrument, string legalEntity, 
            bool metricIsPositive = true);

        decimal GetQuote(string asset1, string asset2, string legalEntity);
    }
}