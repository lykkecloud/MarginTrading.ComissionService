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

        public decimal GetQuoteRateForQuoteAsset(string accountAssetId, string assetPairId, string legalEntity)
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
    }
}