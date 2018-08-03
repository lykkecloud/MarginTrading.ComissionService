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

        public RateSettingsService(
            IMarginTradingBlobRepository blobRepository,
            IDatabase redisDatabase,
            IEventSender eventSender,
            ILog log)
        {
            _blobRepository = blobRepository;
            _redisDatabase = redisDatabase;
            _eventSender = eventSender;
            _log = log;
        }

        #region Order Execution
        
        public async Task<IReadOnlyList<OrderExecutionRate>> GetOrderExecutionRates()
        {
            var cachedData = (await _redisDatabase.HashGetAllAsync(GetKey(LykkeConstants.OrderExecutionKey)))
                .Select(x => Deserialize<OrderExecutionRate>(x.Value)).ToList();

            // Refresh the data from the repo if it is absent in Redis
            if (cachedData.Count == 0)
            {
                var repoData = (await _blobRepository.ReadAsync<IEnumerable<OrderExecutionRate>>(
                    blobContainer: LykkeConstants.RateSettingsBlobContainer,
                    key: LykkeConstants.OrderExecutionKey
                ))?.ToList();

                if (repoData != null && repoData.Count != 0)
                {
                    await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OrderExecutionKey), 
                        repoData.Select(x => new HashEntry(x.AssetPairId, Serialize(x))).ToArray());
                    cachedData = repoData;
                }
            }

            return cachedData;
        }

        public async Task ReplaceOrderExecutionRates(List<OrderExecutionRate> rates)
        {
            await _blobRepository.WriteAsync(
                blobContainer: LykkeConstants.RateSettingsBlobContainer,
                key: LykkeConstants.OrderExecutionKey,
                obj: rates);
            
            await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OrderExecutionKey), 
                rates.Select(x => new HashEntry(x.AssetPairId, Serialize(x))).ToArray());

            await _eventSender.SendRateSettingsChanged(CommissionType.OrderExecution);
        }
        
        #endregion Order Execution

        #region Overnight Swaps
        
        public async Task<IReadOnlyList<OvernightSwapRate>> GetOvernightSwapRates()
        {
            var cachedData = (await _redisDatabase.HashGetAllAsync(GetKey(LykkeConstants.OvernightSwapKey)))
                .Select(x => Deserialize<OvernightSwapRate>(x.Value)).ToList();

            // Refresh the data from the repo if it is absent in Redis
            if (cachedData.Count == 0)
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
            }

            return cachedData;
        }

        public async Task ReplaceOvernightSwapRates(List<OvernightSwapRate> rates)
        {
            await _blobRepository.WriteAsync(
                blobContainer: LykkeConstants.RateSettingsBlobContainer,
                key: LykkeConstants.OvernightSwapKey,
                obj: rates);
            
            await _redisDatabase.HashSetAsync(GetKey(LykkeConstants.OvernightSwapKey), 
                rates.Select(x => new HashEntry(x.AssetPairId, Serialize(x))).ToArray());

            await _eventSender.SendRateSettingsChanged(CommissionType.OvernightSwap);
        }
        
        #endregion Overnight Swaps
        
        #region On Behalf
        
        public async Task<OnBehalfRate> GetOnBehalfRate()
        {
            var cachedStr = await _redisDatabase.StringGetAsync(GetKey(LykkeConstants.OnBehalfKey));
            var cachedData = string.IsNullOrEmpty(cachedStr) ? null : Deserialize<OnBehalfRate>(cachedStr);

            // Refresh the data from the repo if it is absent in Redis
            if (cachedData == null)
            {
                var repoData = await _blobRepository.ReadAsync<OnBehalfRate>(
                    blobContainer: LykkeConstants.RateSettingsBlobContainer,
                    key: LykkeConstants.OnBehalfKey
                );

                if (repoData != null)
                {
                    await _redisDatabase.StringSetAsync(GetKey(LykkeConstants.OnBehalfKey), Serialize(repoData));
                    cachedData = repoData;
                }
            }

            return cachedData;
        }

        public async Task ReplaceOnBehalfRate(OnBehalfRate rate)
        {
            await _blobRepository.WriteAsync(
                blobContainer: LykkeConstants.RateSettingsBlobContainer,
                key: LykkeConstants.OnBehalfKey,
                obj: rate);
            
            await _redisDatabase.StringSetAsync(GetKey(LykkeConstants.OnBehalfKey), Serialize(rate));

            await _eventSender.SendRateSettingsChanged(CommissionType.OnBehalf);
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
            return $"RateSettings:{key}";
        }
    }
}