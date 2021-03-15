// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.AssetService.Contracts.ClientProfileSettings;
using MarginTrading.AssetService.Contracts.Enums;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Projections
{
    public class ClientProfileSettingsProjection
    {
        private readonly ICacheUpdater _cacheUpdater;
        private readonly IClientProfileSettingsCache _clientProfileSettingsCache;

        public ClientProfileSettingsProjection(ICacheUpdater cacheUpdater,
            IClientProfileSettingsCache clientProfileSettingsCache)
        {
            _cacheUpdater = cacheUpdater;
            _clientProfileSettingsCache = clientProfileSettingsCache;
        }

        [UsedImplicitly]
        public Task Handle(ClientProfileSettingsChangedEvent @event)
        {
            _cacheUpdater.InitTradingInstruments();
            _cacheUpdater.InitOvernightSwapRates();

            switch (@event.ChangeType)
            {
                case ChangeType.Creation:
                    _clientProfileSettingsCache.AddOrUpdate(@event.NewValue);
                    break;
                case ChangeType.Edition:
                    _clientProfileSettingsCache.AddOrUpdate(@event.NewValue);
                    break;
                case ChangeType.Deletion:
                    _clientProfileSettingsCache.Remove(@event.OldValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return Task.CompletedTask;
        }
    }
}