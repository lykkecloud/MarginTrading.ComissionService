using System;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings.Rates;

namespace MarginTrading.CommissionService.Services
{
    public class CommissionCalcService : ICommissionCalcService
    {
        private readonly ICfdCalculatorService _cfdCalculatorService;
        private readonly DefaultRateSettings _defaultRateSettings;

        public CommissionCalcService(
            ICfdCalculatorService cfdCalculatorService,
            DefaultRateSettings defaultRateSettings)
        {
            _cfdCalculatorService = cfdCalculatorService;
            _defaultRateSettings = defaultRateSettings;
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

        /// <summary>
        /// Value must be charged as it is, without negation
        /// </summary>
        /// <param name="openPosition"></param>
        /// <param name="assetPair"></param>
        /// <returns></returns>
        public decimal GetOvernightSwap(IOpenPosition openPosition, IAssetPair assetPair)
        {
            var defaultSettings = _defaultRateSettings.DefaultOvernightSwapSettings;
            var volumeInAsset = _cfdCalculatorService.GetQuoteRateForQuoteAsset(defaultSettings.CommissionAsset,
                                    openPosition.AssetPairId, assetPair.LegalEntity)
                                * Math.Abs(openPosition.CurrentVolume);
            var basisOfCalc = - defaultSettings.FixRate
                - (openPosition.Direction == PositionDirection.Short ? defaultSettings.RepoSurchargePercent : 0)
                + (defaultSettings.VariableRateBase - defaultSettings.VariableRateQuote)
                              * (openPosition.Direction == PositionDirection.Long ? 1 : -1);
            return volumeInAsset * basisOfCalc / 365;
        }

        public decimal CalculateOrderExecutionCommission(string instrument, string legalEntity, decimal volume)
        {
            var defaultSettings = _defaultRateSettings.DefaultOrderExecutionSettings;

            var volumeInAsset = _cfdCalculatorService.GetQuoteRateForQuoteAsset(defaultSettings.CommissionAsset,
                                    instrument, legalEntity)
                                * Math.Abs(volume);
            
            var commission = Math.Min(
                defaultSettings.CommissionCap, 
                Math.Max(
                    defaultSettings.CommissionFloor,
                    defaultSettings.CommissionRate * volumeInAsset));

            return commission;
        }
    }
}