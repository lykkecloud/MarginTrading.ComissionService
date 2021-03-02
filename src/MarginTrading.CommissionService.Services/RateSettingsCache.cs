// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.AssetService.Contracts;
using MarginTrading.AssetService.Contracts.ClientProfileSettings;
using MarginTrading.AssetService.Contracts.Rates;
using MarginTrading.CommissionService.Core.Domain.Rates;
using MarginTrading.CommissionService.Core.Services;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MarginTrading.CommissionService.Services
{
    public class RateSettingsCache : IRateSettingsCache
    {
        private readonly IDatabase _redisDatabase;
        private readonly IServer _redisServer;
        private readonly IRateSettingsApi _rateSettingsApi;
        private readonly IClientProfileSettingsCache _clientProfileSettingsCache;
        private readonly IConvertService _convertService;
        private readonly IAccountRedisCache _accountsCache;
        private readonly ILog _log;

        public RateSettingsCache(IDatabase redisDatabase,
            ILog log,
            IRateSettingsApi rateSettingsApi,
            IClientProfileSettingsCache clientProfileSettingsCache,
            IConvertService convertService,
            IAccountRedisCache accountsCache,
            IServer redisServer)
        {
            _redisDatabase = redisDatabase;
            _log = log;
            _rateSettingsApi = rateSettingsApi;
            _clientProfileSettingsCache = clientProfileSettingsCache;
            _convertService = convertService;
            _accountsCache = accountsCache;
            _redisServer = redisServer;
        }

        #region Overnight Swaps

        public async Task<OvernightSwapRate> GetOvernightSwapRate(string assetPairId, string tradingConditionId)
        {
            //first we try to grab from Redis
            var key = GetKey(tradingConditionId);
            var serializedData = await _redisDatabase.KeyExistsAsync(key)
                ? await _redisDatabase.HashGetAsync(key, assetPairId)
                : (RedisValue) string.Empty;
            var cachedData = serializedData.HasValue ? Deserialize<OvernightSwapRate>(serializedData) : null;

            //now we try to refresh the cache from repository
            if (cachedData == null)
            {
                var repoData = await RefreshOvernightSwapRates(tradingConditionId);
                cachedData = repoData?.FirstOrDefault(x => x.AssetPairId == assetPairId);
                if (cachedData == null)
                {
                    await _log.WriteWarningAsync(nameof(RateSettingsCache), nameof(GetOvernightSwapRate),
                        $"No overnight swap rate for {assetPairId}. Should never reach here");
                }
            }

            return cachedData;
        }

        public async Task ClearOvernightSwapRatesCache()
        {
            var keys = _redisServer.Keys(pattern: GetKey("*")).ToArray();
            
            await _redisDatabase.KeyDeleteAsync(keys);

            await _log.WriteInfoAsync(
                nameof(RateSettingsCache),
                nameof(ClearOvernightSwapRatesCache),
                "Overnight swap rates cached has been cleared.");
        }

        private async Task<List<OvernightSwapRate>> RefreshOvernightSwapRates(string tradingConditionId)
        {
            var response = await _rateSettingsApi.GetOvernightSwapRatesAsync(tradingConditionId);
            var rates = response?.Select(x => _convertService.Convert<OvernightSwapRateContract, OvernightSwapRate>(x)).ToList();

            if (rates != null && rates.Any())
            {
                await _redisDatabase.HashSetAsync(GetKey(tradingConditionId),
                    rates.Select(x => new HashEntry(x.AssetPairId, Serialize(x))).ToArray());    
            }
            
            return rates;
        }

        #endregion Overnight Swaps

        #region On Behalf

        [ItemCanBeNull]
        public async Task<OnBehalfRate> GetOnBehalfRate(string accountId, string assetType)
        {
            var account = await _accountsCache.GetAccount(accountId);

            var clientProfileSettings = _clientProfileSettingsCache.GetByIds(account.TradingConditionId, assetType);
            
            return new OnBehalfRate
            {
                Commission = clientProfileSettings.OnBehalfFee,
            };
        }
        #endregion On Behalf

        private string Serialize<TMessage>(TMessage obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private TMessage Deserialize<TMessage>(string data)
        {
            return JsonConvert.DeserializeObject<TMessage>(data);
        }

        private string GetKey(string tradingConditionId)
        {
            return $"CommissionService:RateSettings:OvernightSwaps:{tradingConditionId}";
        }
    }
}