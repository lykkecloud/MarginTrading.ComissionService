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
        private string _settlementCurrency;

        private readonly IBrokerSettingsApi _brokerSettingsApi;
        private readonly string _brokerId;

        public BrokerSettingsService(IBrokerSettingsApi brokerSettingsApi, string brokerId)
        {
            _brokerSettingsApi = brokerSettingsApi;
            _brokerId = brokerId;
        }

        public async Task<string> GetSettlementCurrencyAsync()
        {
            if (!string.IsNullOrEmpty(_settlementCurrency))
                return _settlementCurrency;

            var response = await _brokerSettingsApi.GetByIdAsync(_brokerId);

            if(response.ErrorCode != BrokerSettingsErrorCodesContract.None)
                throw new Exception($"Missing broker settings for configured broker id: {_brokerId}");

            //Since settlement currency is readonly and won't be updated I don't think sync is needed
            _settlementCurrency = response.BrokerSettings.SettlementCurrency;

            return response.BrokerSettings.SettlementCurrency;
        }
    }
}