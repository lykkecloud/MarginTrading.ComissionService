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