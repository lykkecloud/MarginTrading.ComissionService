using System;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class CommissionCalcService : ICommissionCalcService
    {
        private readonly ICfdCalculatorService _cfdCalculatorService;

        public CommissionCalcService(
            ICfdCalculatorService cfdCalculatorService)
        {
            _cfdCalculatorService = cfdCalculatorService;
        }

        private decimal CalculateSwaps(string accountAssetId, string instrument, DateTime? openDate, DateTime? closeDate,
            decimal volume, decimal swapRate, string legalEntity)
        {
            decimal result = 0;

//            if (openDate.HasValue)
//            {
//                var close = closeDate ?? DateTime.UtcNow;
//                var seconds = (decimal) (close - openDate.Value).TotalSeconds;
//
//                const int secondsInYear = 31536000;
//                var quote = _cfdCalculatorService.GetQuoteRateForBaseAsset(accountAssetId, instrument, legalEntity, 
//                    volume * swapRate > 0);
//                var swaps = quote * volume * swapRate * seconds / secondsInYear;
//                result = Math.Round(swaps, _assetsCache.GetAssetAccuracy(accountAssetId));
//            }

            return result;
        }

        public decimal GetOvernightSwap(IOpenPosition openPosition, decimal swapRate)
        {
            var openDate = DateTime.UtcNow;
            var closeDate = openDate.AddDays(1);
//            return CalculateSwaps(openPosition.AccountAssetId, openPosition.Instrument, openDate, closeDate,
//                Math.Abs(openPosition.Volume), swapRate, openPosition.LegalEntity);
            return 0;
        }
    }
}