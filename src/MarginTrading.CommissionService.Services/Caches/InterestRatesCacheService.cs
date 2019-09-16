// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using System.Linq;
using Common.Log;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Repositories;

namespace MarginTrading.CommissionService.Services.Caches
{
    public class InterestRatesCacheService : IInterestRatesCacheService
    {
        private readonly ILog _log;
        private readonly IInterestRatesRepository _interestRatesRepository;
        
        private ConcurrentDictionary<string, decimal> _cache = new ConcurrentDictionary<string, decimal>();
        
        public InterestRatesCacheService(ILog log,
            IInterestRatesRepository interestRatesRepository)
        {
            _log = log;
            _interestRatesRepository = interestRatesRepository;
        }
        
        public decimal GetRate(string id)
        {
            if (_cache.TryGetValue(id ?? string.Empty, out var rate))
            {
                return rate;
            }
            
            _log.WriteWarning(nameof(InterestRatesCacheService), nameof(GetRate), $"The interest rate with ID {id} was not found. Using 0 as default value.");

            return 0;
        }

        public void InitCache()
        {
            _log.WriteInfo(nameof(InterestRatesCacheService), nameof(InitCache), "Interest rates cache init started.");

            var rates = _interestRatesRepository.GetAllLatest().GetAwaiter().GetResult()
                .ToDictionary(x => x.AssetPairId, x => x.Rate);
            
            _cache = new ConcurrentDictionary<string, decimal>(rates);
            
            _log.WriteInfo(nameof(InterestRatesCacheService), nameof(InitCache), 
                $"{rates.Count} interest rates were cached");
        }
    }
}