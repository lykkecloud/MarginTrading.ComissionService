// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using MarginTrading.AssetService.Contracts;
using MarginTrading.AssetService.Contracts.Rates;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain.Rates;
using MarginTrading.CommissionService.Core.Exceptions;
using MarginTrading.CommissionService.Core.Services;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MarginTrading.CommissionService.Services
{
    public class RateSettingsService : IRateSettingsService, IRateSettingsCache
    {
        private readonly IDatabase _redisDatabase;
        private readonly ILog _log;

        private readonly IAssetPairsCache _assetPairsCache;
        private readonly IRateSettingsApi _rateSettingsApi;
        private readonly IClientProfileCache _clientProfileCache;
        private readonly IClientProfileSettingsCache _clientProfileSettingsCache;
        private readonly IConvertService _convertService;

        public RateSettingsService(IDatabase redisDatabase,
            ILog log,
            IAssetPairsCache assetPairsCache,
            IRateSettingsApi rateSettingsApi,
            IClientProfileCache clientProfileCache,
            IClientProfileSettingsCache clientProfileSettingsCache,
            IConvertService convertService)
        {
            _redisDatabase = redisDatabase;
            _log = log;
            _assetPairsCache = assetPairsCache;
            _rateSettingsApi = rateSettingsApi;
            _clientProfileCache = clientProfileCache;
            _clientProfileSettingsCache = clientProfileSettingsCache;
            _convertService = convertService;
        }

        #region Order Execution

        public async Task<IReadOnlyList<OrderExecutionRate>> GetOrderExecutionRates(
            IList<string> assetPairIds = null)
        {
            if (assetPairIds == null || !assetPairIds.Any())
                return await GetOrderExecutionAllRates();

            var result = new List<OrderExecutionRate>();
            foreach (var assetPair in assetPairIds)
            {
                try
                {
                    _assetPairsCache.GetAssetPairById(assetPair);
                }
                catch (AssetPairNotFoundException)
                {
                    _log.Warning($"Requested asset pair [{assetPair}] not found. Can't get an order execution rate.");
                    continue;
                }

                var rate = await GetOrderExecutionSingleRate(assetPair);
                result.Add(rate);
            }

            return result;
        }

        private async Task<OrderExecutionRate> GetOrderExecutionSingleRate(string assetPairId)
        {
            //first we try to grab from Redis
            var serializedData = await _redisDatabase.KeyExistsAsync(GetKey(LykkeConstants.OrderExecutionKey))
                ? await _redisDatabase.HashGetAsync(GetKey(LykkeConstants.OrderExecutionKey), assetPairId)
                : (RedisValue) string.Empty;
            var cachedData = serializedData.HasValue ? Deserialize<OrderExecutionRate>(serializedData) : null;

            //now we try to refresh the cache from repository
            if (cachedData == null)
            {
                var repoData = await RefreshOrderExecutionRates();
                cachedData = repoData?.FirstOrDefault(x => x.AssetPairId == assetPairId);
                if (cachedData == null)
                {
                    await _log.WriteWarningAsync(nameof(RateSettingsService), nameof(GetOrderExecutionSingleRate),
                        $"No order execution rate for {assetPairId}. Should never reach here");
                }
            }

            return cachedData;
        }

        private async Task<IReadOnlyList<OrderExecutionRate>> GetOrderExecutionAllRates()
        {
            var cachedData = await _redisDatabase.KeyExistsAsync(GetKey(LykkeConstants.OrderExecutionKey))
                ? (await _redisDatabase.HashGetAllAsync(GetKey(LykkeConstants.OrderExecutionKey)))
                .Select(x => Deserialize<OrderExecutionRate>(x.Value)).ToList()
                : new List<OrderExecutionRate>();

            // Refresh the data from the repo if it is absent in Redis
            if (cachedData.Count == 0)
            {
                cachedData = await RefreshOrderExecutionRates();
            }

            return cachedData;
        }

        public async Task<List<OrderExecutionRate>> RefreshOrderExecutionRates()
        {
            var repoData = (await _rateSettingsApi.GetOrderExecutionRatesAsync())?
                .Select(rate => _convertService.Convert<OrderExecutionRateContract, OrderExecutionRate>(rate))
                .ToList();

            if (repoData != null && repoData.Count != 0)
            {
                await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OrderExecutionKey),
                    repoData.Select(x => new HashEntry(x.AssetPairId, Serialize(x))).ToArray());
            }

            return repoData;
        }

        #endregion Order Execution

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
                    await _log.WriteWarningAsync(nameof(RateSettingsService), nameof(GetOvernightSwapRate),
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

        public OnBehalfRate GetOnBehalfRate(string assetType)
        {
            var defaultProfile = _clientProfileCache.GetDefaultClientProfileId();
            var clientProfileSettings = _clientProfileSettingsCache.GetByIds(defaultProfile, assetType);

            return new OnBehalfRate()
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