using System;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class CommissionService : ICommissionService
    {
        private readonly IAccountAssetsCacheService _accountAssetsCacheService;
        private readonly ICfdCalculatorService _cfdCalculatorService;
        private readonly IAssetsCache _assetsCache;

        public CommissionService(
            IAccountAssetsCacheService accountAssetsCacheService,
            ICfdCalculatorService cfdCalculatorService,
            IAssetsCache assetsCache)
        {
            _accountAssetsCacheService = accountAssetsCacheService;
            _cfdCalculatorService = cfdCalculatorService;
            _assetsCache = assetsCache;
        }

        private decimal CalculateSwaps(string accountAssetId, string instrument, DateTime? openDate, DateTime? closeDate,
            decimal volume, decimal swapRate, string legalEntity)
        {
            decimal result = 0;

            if (openDate.HasValue)
            {
                var close = closeDate ?? DateTime.UtcNow;
                var seconds = (decimal) (close - openDate.Value).TotalSeconds;

                const int secondsInYear = 31536000;
                var quote = _cfdCalculatorService.GetQuoteRateForBaseAsset(accountAssetId, instrument, legalEntity, 
                    volume * swapRate > 0);
                var swaps = quote * volume * swapRate * seconds / secondsInYear;
                result = Math.Round(swaps, _assetsCache.GetAssetAccuracy(accountAssetId));
            }

            return result;
        }

        public decimal GetOvernightSwap(IOrder order, decimal swapRate)
        {
            var openDate = DateTime.UtcNow;
            var closeDate = openDate.AddDays(1);
            return CalculateSwaps(order.AccountAssetId, order.Instrument, openDate, closeDate,
                Math.Abs(order.Volume), swapRate, order.LegalEntity);
        }
    }
}