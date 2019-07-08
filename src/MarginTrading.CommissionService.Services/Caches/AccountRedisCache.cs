// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using MarginTrading.AccountsManagement.Contracts;
using MarginTrading.AccountsManagement.Contracts.Models;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MarginTrading.CommissionService.Services.Caches
{
    public class AccountRedisCache : IAccountRedisCache
    {
        private readonly IDatabase _redisDatabase;
        private readonly IAccountsApi _accountsApi;
        private readonly IConvertService _convertService;
        private readonly ILog _log;

        public AccountRedisCache(
            IDatabase redisDatabase,
            IAccountsApi accountsApi,
            IConvertService convertService,
            ILog log)
        {
            _redisDatabase = redisDatabase;
            _accountsApi = accountsApi;
            _convertService = convertService;
            _log = log;
        }
        
        public async Task<Account> GetAccount(string id)
        {
            //first we try to grab from Redis
            var serializedData = await _redisDatabase.KeyExistsAsync(GetKey())
                ? await _redisDatabase.HashGetAsync(GetKey(), id)
                : (RedisValue)string.Empty;
            var cachedData = serializedData.HasValue ? Deserialize<Account>(serializedData) : null;
            
            //now we try to refresh the cache from repository
            if (cachedData == null)
            {//todo now it is only used for account asset. possibly it might be needed to subscribe on account changed events..
               cachedData = (await RefreshRedisFromApi((List<Account>)null)).FirstOrDefault(x => x.Id == id);
            }

            return cachedData;
        }
        
        public async Task<IReadOnlyList<Account>> GetAccounts()
        {
            var cachedData = await _redisDatabase.KeyExistsAsync(GetKey())
                ? (await _redisDatabase.HashGetAllAsync(GetKey()))
                    .Select(x => Deserialize<Account>(x.Value)).ToList()
                : new List<Account>();

            // Refresh the data from the repo if it is absent in Redis
            if (cachedData.Count == 0)
            {
                cachedData = await RefreshRedisFromApi(cachedData);
            }

            return cachedData;
        }
        
        private async Task<List<Account>> RefreshRedisFromApi(List<Account> cachedData = null)
        {
            var repoData = (await _accountsApi.List()).Select(_convertService.Convert<AccountContract, Account>).ToList();

            if (repoData.Count != 0)
            {
                await _redisDatabase.HashSetAsync(GetKey(), 
                    repoData.Select(x => new HashEntry(x.Id, Serialize(x))).ToArray());
                cachedData = repoData;
            }

            return cachedData;
        }

        private string Serialize<TMessage>(TMessage obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        private TMessage Deserialize<TMessage>(string data)
        {
            return JsonConvert.DeserializeObject<TMessage>(data);
        }

        private string GetKey()
        {
            return $"CommissionService:{LykkeConstants.AccountsKey}";
        }
    }
}