// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Services
{
    public class BBVACostsAndChargesGenerationService : CostsAndChargesGenerationService
    {
        private readonly IProductsCache _productsCache;
        private readonly IKidScenariosService _kidScenariosService;

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
            CommissionServiceSettings settings,
            IProductsCache productsCache,
            IKidScenariosService kidScenariosService)
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
            _productsCache = productsCache;
            _kidScenariosService = kidScenariosService;
        }

        protected override async Task<CostsAndChargesCalculation> GetCalculationAsync(string instrument, 
            OrderDirection direction, 
            string baseAssetId, 
            string tradingConditionId,
            string legalEntity, 
            decimal? anticipatedExecutionPrice = null)
        {
            var calculation =  await base.GetCalculationAsync(instrument, direction, baseAssetId, tradingConditionId, legalEntity, anticipatedExecutionPrice);
            
            var product = _productsCache.GetById(instrument);
            var isin = direction == OrderDirection.Buy ? product.IsinLong : product.IsinShort;
            var kidScenario = await _kidScenariosService.GetByIdAsync(isin);
            if (kidScenario.IsFailed
                || !kidScenario.Value.KidModerateScenario.HasValue
                || !kidScenario.Value.KidModerateScenarioAvreturn.HasValue)
                throw new Exception(
                    $"KID scenario not found or null for isin {isin} and calculation {calculation.Id}");
            
            var theoreticalNetReturn = kidScenario.Value.KidModerateScenario.Value +
                                       calculation.TotalCosts.ValueInEur;
            
            calculation.KidScenario = new CostsAndChargesValue(kidScenario.Value.KidModerateScenario.Value,
                kidScenario.Value.KidModerateScenarioAvreturn.Value);
            
            calculation.TheoreticalNetReturn = new CostsAndChargesValue(theoreticalNetReturn,
                theoreticalNetReturn / calculation.Volume);
            
            calculation.RoundValues(2);
            return calculation;
        }
    }
}