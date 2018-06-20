using Lykke.SettingsReader.Attributes;

namespace MarginTrading.CommissionService.Core.Settings
{
    public class DbSettings
    {
        [AzureBlobCheck]
        public string StateConnString { get; set; }
        
        [AzureTableCheck]
        public string LogsConnString { get; set; }
        
        [AzureTableCheck]
        public string MarginTradingConnString { get; set; }
    }
}
