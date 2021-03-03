// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Services
{
    public class BBVACostsAndChargesGenerationService : CostsAndChargesGenerationService
    {
        public BBVACostsAndChargesGenerationService(IQuoteCacheService quoteCacheService,
            ISystemClock systemClock,
            ICostsAndChargesRepository repository,
            ISharedCostsAndChargesRepository sharedRepository,
            IPositionReceiveService positionReceiveService,
            ITradingInstrumentsCache tradingInstrumentsCache,
            ICfdCalculatorService cfdCalculatorService,
            IAccountRedisCache accountRedisCache,
            IRateSettingsService rateSettingsCache,
            IInterestRatesCacheService interestRatesCacheService,
            IAssetPairsCache assetPairsCache,
            ITradingDaysInfoProvider tradingDaysInfoProvider,
            IBrokerSettingsService brokerSettingsService,
            CostsAndChargesDefaultSettings defaultSettings,
            CommissionServiceSettings settings)
            : base(quoteCacheService,
                systemClock,
                repository,
                sharedRepository,
                positionReceiveService,
                tradingInstrumentsCache,
                cfdCalculatorService,
                accountRedisCache,
                rateSettingsCache,
                interestRatesCacheService,
                assetPairsCache,
                tradingDaysInfoProvider,
                brokerSettingsService,
                defaultSettings,
                settings,
                10000,
                0)
        {
        }
    }
}