namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICfdCalculatorService
    {

        decimal GetQuoteRateForQuoteAsset(string accountAssetId, string instrument, string legalEntity);

        decimal GetQuote(string asset1, string asset2, string legalEntity);

    }
}