using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Domain.Rates;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings.Rates;
using StackExchange.Redis;

namespace MarginTrading.CommissionService.Services
{
    /// <summary>
    /// Generic service facade, which may be used to store objects of type <typeparam name="T"/>
    /// and do CRUD operations on them. Hides blob storage with Redis cache (on hash set).
    /// When data is not found in cache and persistence layer, it may be initialized from defaults.
    /// Result = Cache ?? Blob ?? Defaults.
    /// </summary>
    public class GenericCrudServiceFacade<T> : IGenericCrudServiceFacade<T>
        where T: class, IKeyedObject
    {
        private readonly IMarginTradingBlobRepository _blobRepository;
        private readonly IDatabase _redisDatabase;
        private readonly ILog _log;

        private readonly string _blobContainer;
        private readonly string _storageKey;
        
        private readonly Func<string, T> _defaultRateSettings;
        private readonly Task _notificationHandler;
        private readonly Func<T, string> _serialize;
        private readonly Func<string, T> _deserialize;
        private readonly Func<string, bool> _defaultReplacementPredicate;
        private readonly Func<string, bool> _refreshFromBlobPredicate;

        public GenericCrudServiceFacade(IMarginTradingBlobRepository blobRepository, IDatabase redisDatabase, ILog log, 
            Func<string, T> defaultRateSettings, string blobContainer, string storageKey, 
            Task notificationHandler, 
            Func<T, string> serialize, Func<string, T> deserialize, 
            Func<string, bool> defaultReplacementPredicate = null, Func<string, bool> refreshFromBlobPredicate = null)
        {
            _blobRepository = blobRepository;
            _redisDatabase = redisDatabase;
            _log = log;
            _defaultRateSettings = defaultRateSettings;
            _blobContainer = blobContainer;
            _storageKey = storageKey;
            _notificationHandler = notificationHandler;
            _serialize = serialize;
            _deserialize = deserialize;
            _defaultReplacementPredicate = defaultReplacementPredicate ?? (s => true);
            _refreshFromBlobPredicate = refreshFromBlobPredicate ?? (s => true);
        }

        public async Task<T> Get(string key)
        {
            //first we try to grab from Redis
            var serializedData = await _redisDatabase.KeyExistsAsync(_storageKey)
                ? await _redisDatabase.HashGetAsync(_storageKey, key)
                : (RedisValue)string.Empty;
            var cachedData = serializedData.HasValue ? _deserialize(serializedData) : null;

            //now we try to refresh the cache from blob
            if (cachedData == null && _refreshFromBlobPredicate(key))
            {
                cachedData = (await RefreshRedisFromBlob())?.FirstOrDefault(x => x.Key == key);
                
                //if not data found in blob & redis - default values are applied
                if (cachedData == null && _defaultReplacementPredicate(key))
                {
                    await _log.WriteWarningAsync($"{nameof(GenericCrudServiceFacade<T>)}<{nameof(T)}>", nameof(Get),
                        $"No object found for key {key}. Using the default one.");

                    var rateFromDefault = _defaultRateSettings(key);
                    
                    await _redisDatabase.HashSetAsync(_storageKey, 
                        new [] {new HashEntry(key, _serialize(rateFromDefault))});
                    
                    return rateFromDefault;
                }
            }

            return cachedData;
        }

        public async Task<IReadOnlyList<T>> GetMany(string predicateKey = null)
        {
            IReadOnlyList<T> cachedData = await _redisDatabase.KeyExistsAsync(_storageKey)
                ? (await _redisDatabase.HashGetAllAsync(_storageKey)).Select(x => _deserialize(x.Value)).ToList()
                : new List<T>();

            // Refresh the data from the repo if it is absent in Redis
            if (cachedData.Count == 0 && (predicateKey == null || _refreshFromBlobPredicate(predicateKey)))
            {
                cachedData = await RefreshRedisFromBlob();
            }

            return cachedData
                .Where(x => predicateKey == null || x.GetFilterKey() == predicateKey)
                .ToList();
        }

        public async Task Replace(IReadOnlyList<T> objects)
        {
            await _blobRepository.MergeListAsync(
                blobContainer: _blobContainer,
                key: _storageKey,
                objects: objects.ToList(),
                selector: x => x.Key);
            
            await _redisDatabase.HashSetAsync(_storageKey, 
                objects.Select(x => new HashEntry(x.Key, _serialize(x))).ToArray());

            await _notificationHandler;
        }

        public async Task Delete(IReadOnlyList<T> objects)
        {
            await _blobRepository.RemoveFromListAsync(
                blobContainer: _blobContainer,
                key: _storageKey,
                objects: objects.ToList(),
                selector: x => x.Key);
            
            await _redisDatabase.HashDeleteAsync(_storageKey, objects.Select(x => (RedisValue)x.Key).ToArray());

            await _notificationHandler;
        }

        private async Task<IReadOnlyList<T>> RefreshRedisFromBlob()
        {
            var repoData =
                (await _blobRepository.ReadAsync<IEnumerable<T>>(blobContainer: _blobContainer, key: _storageKey))
                ?.ToList();

            if (repoData == null || repoData.Count == 0)
            {
                return null;
            }
            
            await _redisDatabase.HashSetAsync(_storageKey, 
                repoData.Select(x => new HashEntry(x.Key, _serialize(x))).ToArray());
            return repoData;
        }
    }
}