// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AssetService.Contracts.ClientProfiles;
using MarginTrading.AssetService.Contracts.Enums;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain.CacheModels;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Projections
{
    public class ClientProfileProjection
    {
        private readonly ICacheUpdater _cacheUpdater;
        private readonly IClientProfileCache _clientProfileCache;
        private readonly IConvertService _convertService;

        public ClientProfileProjection(
            ICacheUpdater cacheUpdater,
            IClientProfileCache clientProfileCache,
            IConvertService convertService)
        {
            _cacheUpdater = cacheUpdater;
            _clientProfileCache = clientProfileCache;
            _convertService = convertService;
        }

        [UsedImplicitly]
        public async Task Handle(ClientProfileChangedEvent @event)
        {
            switch (@event.ChangeType)
            {
                case ChangeType.Creation:
                    UpdateCaches();
                    _clientProfileCache.AddOrUpdate(
                        _convertService.Convert<ClientProfileContract, ClientProfileCacheModel>(
                            @event.NewValue));
                    break;
                case ChangeType.Edition:
                    if (@event.OldValue.IsDefault != @event.NewValue.IsDefault) UpdateCaches();
                    
                    _clientProfileCache.AddOrUpdate(
                        _convertService.Convert<ClientProfileContract, ClientProfileCacheModel>(
                            @event.NewValue));
                    break;
                case ChangeType.Deletion:
                    _clientProfileCache.Remove(
                        _convertService.Convert<ClientProfileContract, ClientProfileCacheModel>(
                            @event.OldValue));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpdateCaches()
        {
            _cacheUpdater.InitTradingInstruments();
            _cacheUpdater.InitOrderExecutionRates();
            _cacheUpdater.InitOvernightSwapRates();
        }
    }
}