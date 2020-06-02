// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Settings
{
    public class ServicesSettings
    {
        public ServiceSettings Backend { get; set; }
        public ServiceSettings TradingHistory { get; set; }
        public ServiceSettings AccountManagement { get; set; }
        public ServiceSettings SettingsService { get; set; }
    }
}