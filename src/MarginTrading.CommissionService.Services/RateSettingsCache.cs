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
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain.Rates;
using MarginTrading.CommissionService.Core.Services;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MarginTrading.CommissionService.Services
{
public class RateSettingsCache : IRateSettingsCache
    {
        private readonly IDatabase _redisDatabase;
        private readonly ILog _log;

        private readonly IRateSettingsApi _rateSettingsApi;
        private readonly IClientProfileSettingsCache _clientProfileSettingsCache;
        private readonly IConvertService _convertService;
        private readonly IAccountRedisCache _accountsCache;

        public RateSettingsCache(IDatabase redisDatabase,
            ILog log,
            IRateSettingsApi rateSettingsApi,
            IClientProfileSettingsCache clientProfileSettingsCache,
            IConvertService convertService,
            IAccountRedisCache accountsCache)
        {
            _redisDatabase = redisDatabase;
            _log = log;
            _rateSettingsApi = rateSettingsApi;
            _clientProfileSettingsCache = clientProfileSettingsCache;
            _convertService = convertService;
            _accountsCache = accountsCache;
        }

        #region Overnight Swaps

        public async Task<OvernightSwapRate> GetOvernightSwapRate(string assetPairId)
        {
            //first we try to grab from Redis
            var serializedData = await _redisDatabase.KeyExistsAsync(GetKey(LykkeConstants.OvernightSwapKey))
                ? await _redisDatabase.HashGetAsync(GetKey(LykkeConstants.OvernightSwapKey), assetPairId)
                : (RedisValue) string.Empty;
            var cachedData = serializedData.HasValue ? Deserialize<OvernightSwapRate>(serializedData) : null;

            //now we try to refresh the cache from repository
            if (cachedData == null)
            {
                var repoData = await RefreshOvernightSwapRates();
                cachedData = repoData?.FirstOrDefault(x => x.AssetPairId == assetPairId);
                if (cachedData == null)
                {
                    await _log.WriteWarningAsync(nameof(RateSettingsCache), nameof(GetOvernightSwapRate),
                        $"No overnight swap rate for {assetPairId}. Should never reach here");
                }
            }

            return cachedData;
        }

        public async Task<IReadOnlyList<OvernightSwapRate>> GetOvernightSwapRatesForApi()
        {
            var cachedData = await _redisDatabase.KeyExistsAsync(GetKey(LykkeConstants.OvernightSwapKey))
                ? (await _redisDatabase.HashGetAllAsync(GetKey(LykkeConstants.OvernightSwapKey)))
                .Select(x => Deserialize<OvernightSwapRate>(x.Value)).ToList()
                : new List<OvernightSwapRate>();

            // Refresh the data from the repo if it is absent in Redis
            if (cachedData.Count == 0)
            {
                cachedData = await RefreshOvernightSwapRates();
            }

            return cachedData;
        }

        public async Task<List<OvernightSwapRate>> RefreshOvernightSwapRates()
        {
            var repoData = (await _rateSettingsApi.GetOvernightSwapRatesAsync())?
                .Select(rate => _convertService.Convert<OvernightSwapRateContract, OvernightSwapRate>(rate))
                .ToList();

            if (repoData != null && repoData.Count != 0)
            {
                await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OvernightSwapKey),
                    repoData.Select(x => new HashEntry(x.AssetPairId, Serialize(x))).ToArray());
            }

            return repoData;
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

        private string GetKey(string key)
        {
            return $"CommissionService:RateSettings:{key}";
        }
    }
}