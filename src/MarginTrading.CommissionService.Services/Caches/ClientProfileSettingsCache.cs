// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using Common.Log;
using MarginTrading.AssetService.Contracts;
using MarginTrading.AssetService.Contracts.ClientProfileSettings;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain.CacheModels;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services.Caches
{
    public class ClientProfileSettingsCache : IClientProfileSettingsCache, IStartable
    {
        private readonly IClientProfileSettingsApi _clientProfileSettingsApi;
        private readonly IConvertService _convertService;
        private readonly ILog _log;

        private Dictionary<string, ClientProfileSettingsCacheModel> _cache =
            new Dictionary<string, ClientProfileSettingsCacheModel>();

        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();

        public ClientProfileSettingsCache(
            IClientProfileSettingsApi clientProfileSettingsApi,
            IConvertService convertService,
            ILog log)
        {
            _clientProfileSettingsApi = clientProfileSettingsApi;
            _convertService = convertService;
            _log = log;
        }

        public void Start()
        {
            _lockSlim.EnterWriteLock();
            try
            {
                _log.WriteInfo(nameof(ClientProfileSettingsCache), nameof(Start),
                    "ClientProfileSettings Cache init started.");

                var response = _clientProfileSettingsApi.GetClientProfileSettingsByRegulationAsync().GetAwaiter()
                    .GetResult();

                _log.WriteInfo(nameof(ClientProfileSettingsCache), nameof(Start),
                    $"{response.ClientProfileSettings.Count} clientProfileSettings read.");

                _cache = response.ClientProfileSettings.ToDictionary(GetKey,
                    x => _convertService.Convert<ClientProfileSettingsContract, ClientProfileSettingsCacheModel>(x));
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public void AddOrUpdate(ClientProfileSettingsCacheModel clientProfileSettings)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                _cache[GetKey(clientProfileSettings)] = clientProfileSettings;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public void Remove(ClientProfileSettingsCacheModel clientProfileSettings)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                var isInCache = _cache.ContainsKey(GetKey(clientProfileSettings));
                if (isInCache)
                    _cache.Remove(clientProfileSettings.ClientProfileId);
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public ClientProfileSettingsCacheModel GetByIds(string profileId, string assetType)
        {
            var isInCache =
                _cache.TryGetValue(GetKey(profileId, assetType), out var result);

            if (!isInCache) 
                _log.WriteWarning(nameof(ClientProfileSettingsCache), nameof(GetByIds), $"Cannot find clientProfileSettings in cache by profile id: {profileId} and asset type: {assetType}");

            return result;
        }

        private string GetKey(ClientProfileSettingsContract clientProfileSettings)
        {
            return GetKey(clientProfileSettings.ClientProfileId, clientProfileSettings.AssetTypeId);
        }

        private string GetKey(ClientProfileSettingsCacheModel clientProfileSettings)
        {
            return GetKey(clientProfileSettings.ClientProfileId, clientProfileSettings.AssetTypeId);
        }

        private string GetKey(string clientProfileId, string assetType)
        {
            return $"{clientProfileId}_{assetType}";
        }
    }
}