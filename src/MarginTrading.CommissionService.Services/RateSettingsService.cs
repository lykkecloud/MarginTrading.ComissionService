using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Rates;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings.Rates;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MarginTrading.CommissionService.Services
{
    public class RateSettingsService : IRateSettingsService
    {
        private readonly IMarginTradingBlobRepository _blobRepository;
        private readonly IDatabase _redisDatabase;
        private readonly IEventSender _eventSender;
        private readonly ILog _log;
        private readonly DefaultRateSettings _defaultRateSettings;

        public const string TradingProfile = "";
        
        public RateSettingsService(
            IMarginTradingBlobRepository blobRepository,
            IDatabase redisDatabase,
            IEventSender eventSender,
            ILog log,
            DefaultRateSettings defaultRateSettings)
        {
            _blobRepository = blobRepository;
            _redisDatabase = redisDatabase;
            _eventSender = eventSender;
            _log = log;
            _defaultRateSettings = defaultRateSettings;
        }

        #region Order Execution

        public async Task<OrderExecutionRate> GetOrderExecutionRate(string tradingConditionId, string assetPairId)
        {
            var key = GetOrderExecId(tradingConditionId, assetPairId);
            //first we try to grab from Redis
            var serializedData = await _redisDatabase.KeyExistsAsync(GetKey(LykkeConstants.OrderExecutionKey))
                ? await _redisDatabase.HashGetAsync(GetKey(LykkeConstants.OrderExecutionKey), key)
                : (RedisValue)string.Empty;
            var cachedData = serializedData.HasValue ? Deserialize<OrderExecutionRate>(serializedData) : null;

            //now we try to refresh the cache from repository
            if (cachedData == null)
            {
                cachedData = (await RefreshOrderExecutionRedisFromRepo())?
                    .FirstOrDefault(x => x.TradingConditionId == tradingConditionId && x.AssetPairId == assetPairId);
                if (cachedData != null && IsTradingProfile(tradingConditionId))
                {
                    await _log.WriteWarningAsync(nameof(RateSettingsService), nameof(GetOrderExecutionRate),
                        $"No order execution rate for asset pair {assetPairId}. Using the default one.");

                    var rateFromDefault = OrderExecutionRate.FromDefault(
                        _defaultRateSettings.DefaultOrderExecutionSettings, tradingConditionId, assetPairId);
                    
                    await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OrderExecutionKey), 
                         new [] {new HashEntry(key, Serialize(rateFromDefault))});
                    
                    return rateFromDefault;
                }
            }

            return cachedData;
        }

        public async Task<IReadOnlyList<OrderExecutionRate>> GetOrderExecutionRatesForApi(string tradingConditionId = null)
        {
            var cachedData = await _redisDatabase.KeyExistsAsync(GetKey(LykkeConstants.OrderExecutionKey))
                ? (await _redisDatabase.HashGetAllAsync(GetKey(LykkeConstants.OrderExecutionKey)))
                   .Select(x => Deserialize<OrderExecutionRate>(x.Value)).ToList()
                : new List<OrderExecutionRate>();

            // Refresh the data from the repo if it is absent in Redis
            if (cachedData.Count == 0 && (tradingConditionId == null || IsTradingProfile(tradingConditionId)))
            {
                cachedData = await RefreshOrderExecutionRedisFromRepo();
            }

            return cachedData.Where(x => tradingConditionId == null || x.TradingConditionId == tradingConditionId).ToList();
        }

        private async Task<List<OrderExecutionRate>> RefreshOrderExecutionRedisFromRepo()
        {
            var repoData = (await _blobRepository.ReadAsync<IEnumerable<OrderExecutionRate>>(
                blobContainer: LykkeConstants.RateSettingsBlobContainer,
                key: LykkeConstants.OrderExecutionKey
            ))?.ToList();

            if (repoData == null || repoData.Count == 0)
            {
                return null;
            }
            
            await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OrderExecutionKey), 
                repoData.Select(x => new HashEntry(
                    GetOrderExecId(x.TradingConditionId, x.AssetPairId), Serialize(x))).ToArray());
            return repoData;
        }

        public async Task ReplaceOrderExecutionRates(List<OrderExecutionRate> rates)
        {
            rates = rates.Select(x =>
            {
                if (string.IsNullOrWhiteSpace(x.LegalEntity))
                {
                    x.LegalEntity = _defaultRateSettings.DefaultOrderExecutionSettings.LegalEntity;
                }
                return x;
            }).ToList();
            
            await _blobRepository.MergeListAsync(
                blobContainer: LykkeConstants.RateSettingsBlobContainer,
                key: LykkeConstants.OrderExecutionKey,
                objects: rates,
                selector: x => GetOrderExecId(x.TradingConditionId, x.AssetPairId));
            
            await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OrderExecutionKey), 
                rates.Select(x => new HashEntry(
                    GetOrderExecId(x.TradingConditionId, x.AssetPairId), Serialize(x))).ToArray());

            await _eventSender.SendRateSettingsChanged(CommissionType.OrderExecution);
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
                var repoData = await RefreshRedisFromRepo((List<OvernightSwapRate>)null);
                cachedData = repoData?.FirstOrDefault(x => x.AssetPairId == assetPairId);
                if (cachedData == null)
                {
                    await _log.WriteWarningAsync(nameof(RateSettingsService), nameof(GetOvernightSwapRate),
                        $"No overnight swap rate for {assetPairId}. Using the default one.");
                    
                    var rateFromDefault =
                        OvernightSwapRate.FromDefault(_defaultRateSettings.DefaultOvernightSwapSettings, assetPairId);
                    
                    await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OvernightSwapKey), 
                        new [] {new HashEntry(assetPairId, Serialize(rateFromDefault))});
                    
                    return rateFromDefault;
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
                cachedData = await RefreshRedisFromRepo(cachedData);
            }

            return cachedData;
        }

        private async Task<List<OvernightSwapRate>> RefreshRedisFromRepo(List<OvernightSwapRate> cachedData = null)
        {
            var repoData = (await _blobRepository.ReadAsync<IEnumerable<OvernightSwapRate>>(
                blobContainer: LykkeConstants.RateSettingsBlobContainer,
                key: LykkeConstants.OvernightSwapKey
            ))?.ToList();

            if (repoData != null && repoData.Count != 0)
            {
                await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OvernightSwapKey), 
                    repoData.Select(x => new HashEntry(x.AssetPairId, Serialize(x))).ToArray());
                cachedData = repoData;
            }

            return cachedData;
        }

        public async Task ReplaceOvernightSwapRates(List<OvernightSwapRate> rates)
        {
            await _blobRepository.MergeListAsync(
                blobContainer: LykkeConstants.RateSettingsBlobContainer,
                key: LykkeConstants.OvernightSwapKey,
                objects: rates,
                selector: x => x.AssetPairId);
            
            await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OvernightSwapKey), 
                rates.Select(x => new HashEntry(x.AssetPairId, Serialize(x))).ToArray());

            await _eventSender.SendRateSettingsChanged(CommissionType.OvernightSwap);
        }
        
        #endregion Overnight Swaps
        
        #region On Behalf

        public async Task<OnBehalfRate> GetOnBehalfRate(string tradingConditionId)
        {
            //first we try to grab from Redis
            var serializedData = await _redisDatabase.KeyExistsAsync(GetKey(LykkeConstants.OnBehalfKey))
                ? await _redisDatabase.HashGetAsync(GetKey(LykkeConstants.OnBehalfKey), tradingConditionId)
                : (RedisValue)string.Empty;
            var cachedData = serializedData.HasValue ? Deserialize<OnBehalfRate>(serializedData) : null;

            //now we try to refresh the cache from repository
            if (cachedData == null)
            {
                cachedData = (await RefreshOnBehalfRedisFromRepo())?
                    .FirstOrDefault(x => x.TradingConditionId == tradingConditionId);
                if (cachedData == null && IsTradingProfile(tradingConditionId))
                {
                    await _log.WriteWarningAsync(nameof(RateSettingsService), nameof(GetOnBehalfRate),
                        "No On Behalf rate. Using the default one.");

                    var rateFromDefault = OnBehalfRate.FromDefault(_defaultRateSettings.DefaultOnBehalfSettings, 
                        tradingConditionId);
                    
                    await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OnBehalfKey), 
                        new [] {new HashEntry(tradingConditionId, Serialize(rateFromDefault))});
                    
                    return rateFromDefault;
                }
            }

            return cachedData;
        }

        public async Task<IReadOnlyList<OnBehalfRate>> GetOnBehalfRates()
        {
            var cachedData = await _redisDatabase.KeyExistsAsync(GetKey(LykkeConstants.OnBehalfKey))
                ? (await _redisDatabase.HashGetAllAsync(GetKey(LykkeConstants.OnBehalfKey)))
                    .Select(x => Deserialize<OnBehalfRate>(x.Value)).ToList()
                : new List<OnBehalfRate>();

            // Refresh the data from the repo if it is absent in Redis
            if (cachedData.Count == 0)
            {
                cachedData = await RefreshOnBehalfRedisFromRepo();
            }

            return cachedData;
        }

        private async Task<List<OnBehalfRate>> RefreshOnBehalfRedisFromRepo()
        {
            var repoData = (await _blobRepository.ReadAsync<IEnumerable<OnBehalfRate>>(
                blobContainer: LykkeConstants.RateSettingsBlobContainer,
                key: LykkeConstants.OnBehalfKey
            ))?.ToList();

            if (repoData == null || repoData.Count == 0)
            {
                return null;
            }
            
            await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OnBehalfKey), 
                repoData.Select(x => new HashEntry(
                    x.TradingConditionId, Serialize(x))).ToArray());
            return repoData;
        }

        public async Task ReplaceOnBehalfRates(List<OnBehalfRate> rates)
        {
            rates = rates.Select(x =>
            {
                if (string.IsNullOrWhiteSpace(x.LegalEntity))
                {
                    x.LegalEntity = _defaultRateSettings.DefaultOnBehalfSettings.LegalEntity;
                }
                return x;
            }).ToList();
            
            await _blobRepository.MergeListAsync(
                blobContainer: LykkeConstants.RateSettingsBlobContainer,
                key: LykkeConstants.OnBehalfKey,
                objects: rates,
                selector: x => x.TradingConditionId);
            
            await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OnBehalfKey), 
                rates.Select(x => new HashEntry(x.TradingConditionId, Serialize(x))).ToArray());

            await _eventSender.SendRateSettingsChanged(CommissionType.OnBehalf);
        }
        
        #endregion On Behalf
        
        public static bool IsTradingProfile(string tradingConditionId) => tradingConditionId == TradingProfile;

        public static string GetOrderExecId(string tradingConditionId, string assetPairId) =>
            $"{tradingConditionId}_{assetPairId}";

        public static string GetKey(string key) => $"CommissionService:RateSettings:{key}";

        public static string Serialize<TMessage>(TMessage obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        public static TMessage Deserialize<TMessage>(string data)
        {
            return JsonConvert.DeserializeObject<TMessage>(data);
        }
    }
}