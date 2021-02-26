// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AssetService.Contracts.ClientProfileSettings;
using MarginTrading.AssetService.Contracts.Enums;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain.CacheModels;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Projections
{
    public class ClientProfileSettingsProjection
    {
        private readonly ICacheUpdater _cacheUpdater;
        private readonly IClientProfileSettingsCache _clientProfileSettingsCache;
        private readonly IConvertService _convertService;
        private readonly IRateSettingsCache _rateSettingsCache;

        public ClientProfileSettingsProjection(ICacheUpdater cacheUpdater,
            IClientProfileSettingsCache clientProfileSettingsCache,
            IConvertService convertService,
            IRateSettingsCache rateSettingsCache)
        {
            _cacheUpdater = cacheUpdater;
            _clientProfileSettingsCache = clientProfileSettingsCache;
            _convertService = convertService;
            _rateSettingsCache = rateSettingsCache;
        }

        [UsedImplicitly]
        public async Task Handle(ClientProfileSettingsChangedEvent @event)
        {
            _cacheUpdater.InitTradingInstruments();
            await _rateSettingsCache.ClearOvernightSwapRatesCache();

            switch (@event.ChangeType)
            {
                case ChangeType.Creation:
                    _clientProfileSettingsCache.AddOrUpdate(
                        _convertService.Convert<ClientProfileSettingsContract, ClientProfileSettingsCacheModel>(
                            @event.NewValue));
                    break;
                case ChangeType.Edition:
                    _clientProfileSettingsCache.AddOrUpdate(
                        _convertService.Convert<ClientProfileSettingsContract, ClientProfileSettingsCacheModel>(
                            @event.NewValue));
                    break;
                case ChangeType.Deletion:
                    _clientProfileSettingsCache.Remove(
                        _convertService.Convert<ClientProfileSettingsContract, ClientProfileSettingsCacheModel>(
                            @event.OldValue));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}