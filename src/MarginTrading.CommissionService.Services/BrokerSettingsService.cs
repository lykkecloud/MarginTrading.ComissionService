// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Lykke.Snow.Mdm.Contracts.Api;
using Lykke.Snow.Mdm.Contracts.Models.Contracts;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class BrokerSettingsService : IBrokerSettingsService
    {
        private readonly IBrokerSettingsApi _brokerSettingsApi;
        private readonly string _brokerId;

        private BrokerSettingsContract _brokerSettings;

        public BrokerSettingsService(IBrokerSettingsApi brokerSettingsApi, string brokerId)
        {
            _brokerSettingsApi = brokerSettingsApi;
            _brokerId = brokerId;
        }

        public async Task<string> GetSettlementCurrencyAsync()
        {
            var brokerSettings = await GetBrokerSettings();

            return brokerSettings.SettlementCurrency;
        }

        public async Task<bool> IsOrderDetailsReportEnabledAsync()
        {
            var brokerSettings = await GetBrokerSettings();

            return brokerSettings.OrderDetailsRequirementEnabled;
        }

        private async Task<BrokerSettingsContract> GetBrokerSettings()
        {
            if (_brokerSettings != null) return _brokerSettings;

            var response = await _brokerSettingsApi.GetByIdAsync(_brokerId);

            if (response.ErrorCode != BrokerSettingsErrorCodesContract.None)
                throw new Exception($"Missing broker settings for configured broker id: {_brokerId}");

            _brokerSettings = response.BrokerSettings;

            return response.BrokerSettings;
        }
    }
}