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

        public RateSettingsService(IDatabase redisDatabase,
            ILog log,
            IAssetPairsCache assetPairsCache,
            IRateSettingsApi rateSettingsApi)
        {
            _redisDatabase = redisDatabase;
            _log = log;
            _assetPairsCache = assetPairsCache;
            _rateSettingsApi = rateSettingsApi;
        }

        #region Order Execution

        public async Task<IReadOnlyList<OrderExecutionRateContract>> GetOrderExecutionRates(
            IList<string> assetPairIds = null)
        {
            if (assetPairIds == null || !assetPairIds.Any())
                return await GetOrderExecutionAllRates();

            var result = new List<OrderExecutionRateContract>();
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

        private async Task<OrderExecutionRateContract> GetOrderExecutionSingleRate(string assetPairId)
        {
            //first we try to grab from Redis
            var serializedData = await _redisDatabase.KeyExistsAsync(GetKey(LykkeConstants.OrderExecutionKey))
                ? await _redisDatabase.HashGetAsync(GetKey(LykkeConstants.OrderExecutionKey), assetPairId)
                : (RedisValue) string.Empty;
            var cachedData = serializedData.HasValue ? Deserialize<OrderExecutionRateContract>(serializedData) : null;

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

        private async Task<IReadOnlyList<OrderExecutionRateContract>> GetOrderExecutionAllRates()
        {
            var cachedData = await _redisDatabase.KeyExistsAsync(GetKey(LykkeConstants.OrderExecutionKey))
                ? (await _redisDatabase.HashGetAllAsync(GetKey(LykkeConstants.OrderExecutionKey)))
                .Select(x => Deserialize<OrderExecutionRateContract>(x.Value)).ToList()
                : new List<OrderExecutionRateContract>();

            // Refresh the data from the repo if it is absent in Redis
            if (cachedData.Count == 0)
            {
                cachedData = await RefreshOrderExecutionRates();
            }

            return cachedData;
        }

        public async Task<List<OrderExecutionRateContract>> RefreshOrderExecutionRates()
        {
            var repoData = (await _rateSettingsApi.GetOrderExecutionRatesAsync())?.ToList();

            if (repoData != null && repoData.Count != 0)
            {
                await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OrderExecutionKey),
                    repoData.Select(x => new HashEntry(x.AssetPairId, Serialize(x))).ToArray());
            }

            return repoData;
        }

        public async Task ReplaceOrderExecutionRates(List<OrderExecutionRateContract> rates)
        {
            await _rateSettingsApi.ReplaceOrderExecutionRatesAsync(rates.ToArray());
            var updatedRates =
                await _rateSettingsApi.GetOrderExecutionRatesAsync(rates.Select(rate => rate.AssetPairId).ToArray());

            await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OrderExecutionKey),
                updatedRates.Select(x => new HashEntry(x.AssetPairId, Serialize(x))).ToArray());
        }

        #endregion Order Execution

        #region Overnight Swaps

        public async Task<OvernightSwapRateContract> GetOvernightSwapRate(string assetPairId)
        {
            //first we try to grab from Redis
            var serializedData = await _redisDatabase.KeyExistsAsync(GetKey(LykkeConstants.OvernightSwapKey))
                ? await _redisDatabase.HashGetAsync(GetKey(LykkeConstants.OvernightSwapKey), assetPairId)
                : (RedisValue) string.Empty;
            var cachedData = serializedData.HasValue ? Deserialize<OvernightSwapRateContract>(serializedData) : null;

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

        public async Task<IReadOnlyList<OvernightSwapRateContract>> GetOvernightSwapRatesForApi()
        {
            var cachedData = await _redisDatabase.KeyExistsAsync(GetKey(LykkeConstants.OvernightSwapKey))
                ? (await _redisDatabase.HashGetAllAsync(GetKey(LykkeConstants.OvernightSwapKey)))
                .Select(x => Deserialize<OvernightSwapRateContract>(x.Value)).ToList()
                : new List<OvernightSwapRateContract>();

            // Refresh the data from the repo if it is absent in Redis
            if (cachedData.Count == 0)
            {
                cachedData = await RefreshOvernightSwapRates();
            }

            return cachedData;
        }

        public async Task<List<OvernightSwapRateContract>> RefreshOvernightSwapRates()
        {
            var repoData = (await _rateSettingsApi.GetOvernightSwapRatesAsync())?.ToList();

            if (repoData != null && repoData.Count != 0)
            {
                await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OvernightSwapKey),
                    repoData.Select(x => new HashEntry(x.AssetPairId, Serialize(x))).ToArray());
            }

            return repoData;
        }

        public async Task ReplaceOvernightSwapRates(List<OvernightSwapRateContract> rates)
        {
            await _rateSettingsApi.ReplaceOvernightSwapRatesAsync(rates.ToArray());
            var updatedRates =
                await _rateSettingsApi.GetOvernightSwapRatesAsync(rates.Select(rate => rate.AssetPairId).ToArray());

            await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OvernightSwapKey),
                updatedRates.Select(x => new HashEntry(x.AssetPairId, Serialize(x))).ToArray());
        }

        #endregion Overnight Swaps

        #region On Behalf

        public async Task<OnBehalfRateContract> GetOnBehalfRate()
        {
            //first we try to grab from Redis
            var serializedData = await _redisDatabase.KeyExistsAsync(GetKey(LykkeConstants.OnBehalfKey))
                ? await _redisDatabase.StringGetAsync(GetKey(LykkeConstants.OnBehalfKey))
                : (RedisValue) string.Empty;
            var cachedData = serializedData.HasValue ? Deserialize<OnBehalfRateContract>(serializedData) : null;

            //now we try to refresh the cache from repository
            if (cachedData == null)
            {
                cachedData = await RefreshOnBehalfRate();
                if (cachedData == null)
                {
                    await _log.WriteWarningAsync(nameof(RateSettingsService), nameof(GetOnBehalfRate),
                        $"No OnBehalf rate saved, should never reach here");
                }
            }

            return cachedData;
        }

        public async Task<OnBehalfRateContract> RefreshOnBehalfRate()
        {
            var repoData = await _rateSettingsApi.GetOnBehalfRateAsync();
            if (repoData != null)
            {
                await _redisDatabase.StringSetAsync(GetKey(LykkeConstants.OnBehalfKey), Serialize(repoData));
            }

            return repoData;
        }

        public async Task ReplaceOnBehalfRate(OnBehalfRateContract rate)
        {
            await _rateSettingsApi.ReplaceOnBehalfRateAsync(rate);
            var updatedRate = _rateSettingsApi.GetOnBehalfRateAsync();

            await _redisDatabase.StringSetAsync(GetKey(LykkeConstants.OnBehalfKey), Serialize(updatedRate));
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