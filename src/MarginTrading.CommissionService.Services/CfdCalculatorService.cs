using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class CfdCalculatorService : ICfdCalculatorService
    {
        private readonly IAssetPairsCache _assetPairsCache;
        private readonly IFxRateCacheService _fxRateCacheService;

        public CfdCalculatorService(
            IAssetPairsCache assetPairsCache,
            IFxRateCacheService fxRateCacheService)
        {
            _assetPairsCache = assetPairsCache;
            _fxRateCacheService = fxRateCacheService;
        }

        public decimal GetFxRateForAssetPair(string accountAssetId, string assetPairId, string legalEntity)
        {
            var assetPair = _assetPairsCache.GetAssetPairById(assetPairId);
            
            if (accountAssetId == assetPair.QuoteAssetId)
                return 1;

            var assetPairSubst = _assetPairsCache.FindAssetPair(assetPair.QuoteAssetId, accountAssetId, legalEntity);

            var rate = assetPairSubst.BaseAssetId == assetPair.QuoteAssetId
                ? _fxRateCacheService.GetQuote(assetPairSubst.Id).Ask
                : 1 / _fxRateCacheService.GetQuote(assetPairSubst.Id).Bid;
            
            return rate;
        }
        
        public decimal GetFxRate(string currency1, string currency2, string legalEntity)
        {
            if (currency1 == currency2)
                return 1;
            
            var assetPair = _assetPairsCache.FindAssetPair(currency1, currency2, legalEntity);

            var quote = _fxRateCacheService.GetQuote(assetPair.Id);

            return quote.Bid;
        }
    }
}