// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Autofac;
using Common.Log;
using MarginTrading.AssetService.Contracts;
using MarginTrading.AssetService.Contracts.ClientProfiles;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain.CacheModels;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services.Caches
{
    public class ClientProfileCache : IClientProfileCache, IStartable
    {
        private readonly IClientProfilesApi _clientProfilesApi;
        private readonly IConvertService _convertService;
        private readonly ILog _log;

        private Dictionary<string, ClientProfileCacheModel> _cache =
            new Dictionary<string, ClientProfileCacheModel>();

        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();

        public ClientProfileCache(
            IClientProfilesApi clientProfilesApi,
            IConvertService convertService,
            ILog log)
        {
            _clientProfilesApi = clientProfilesApi;
            _convertService = convertService;
            _log = log;
        }

        public void Start()
        {
            _lockSlim.EnterWriteLock();
            try
            {
                _log.WriteInfo(nameof(ClientProfileSettingsCache), nameof(Start),
                    "ClientProfile Cache init started.");

                var response = _clientProfilesApi.GetClientProfilesAsync().GetAwaiter()
                    .GetResult();

                _log.WriteInfo(nameof(ClientProfileSettingsCache), nameof(Start),
                    $"{response.ClientProfiles.Count} clientProfiles read.");

                _cache = response.ClientProfiles.ToDictionary(x => x.Id,
                    x => _convertService.Convert<ClientProfileContract, ClientProfileCacheModel>(x));
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public void AddOrUpdate(ClientProfileCacheModel clientProfile)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                _cache[clientProfile.Id] = clientProfile;
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public void Remove(ClientProfileCacheModel clientProfile)
        {
            _lockSlim.EnterWriteLock();
            try
            {
                var isInCache = _cache.ContainsKey(clientProfile.Id);
                if (isInCache)
                    _cache.Remove(clientProfile.Id);
            }
            finally
            {
                _lockSlim.ExitWriteLock();
            }
        }

        public string GetDefaultClientProfileId()
        {
            var defaultProfile = _cache.Values.FirstOrDefault(x => x.IsDefault);

            if (defaultProfile == null)
                throw new Exception("There is no default client profile in the system");

            return defaultProfile.Id;
        }
    }
}