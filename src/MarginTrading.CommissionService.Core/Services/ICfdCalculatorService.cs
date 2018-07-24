namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICfdCalculatorService
    {
        decimal GetQuoteRateForQuoteAsset(string accountAssetId, string instrument, string legalEntity);
    }
}