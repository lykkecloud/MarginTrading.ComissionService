namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICfdCalculatorService
    {

        decimal GetFxRateForAssetPair(string accountAssetId, string instrument, string legalEntity);

        decimal GetFxRate(string asset1, string asset2, string legalEntity);

    }
}