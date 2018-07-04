﻿using System.Text;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Blob;
using Common;
using Lykke.SettingsReader;
using MarginTrading.CommissionService.Core.Repositories;
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.AzureRepositories.Repositories
{
    public class MarginTradingBlobRepository : IMarginTradingBlobRepository
    {
        private readonly IBlobStorage _blobStorage;

        public MarginTradingBlobRepository(IReloadingManager<string> connectionString)
        {
            _blobStorage = AzureBlobStorage.Create(connectionString);
        }

        public T Read<T>(string blobContainer, string key)
        {
            if (_blobStorage.HasBlobAsync(blobContainer, key).Result)
            {
                var data = _blobStorage.GetAsync(blobContainer, key).Result.ToBytes();
                var str = Encoding.UTF8.GetString(data);

                return JsonConvert.DeserializeObject<T>(str);
            }

            return default(T);
        }


        public async Task<T> ReadAsync<T>(string blobContainer, string key)
        {
            if (_blobStorage.HasBlobAsync(blobContainer, key).Result)
            {
                var data = (await _blobStorage.GetAsync(blobContainer, key)).ToBytes();
                var str = Encoding.UTF8.GetString(data);

                return JsonConvert.DeserializeObject<T>(str);
            }

            return default(T);
        }

        public async Task Write<T>(string blobContainer, string key, T obj)
        {
            var data = JsonConvert.SerializeObject(obj).ToUtf8Bytes();
            await _blobStorage.SaveBlobAsync(blobContainer, key, data);
        }
    }
}