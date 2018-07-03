using Lykke.SettingsReader.Attributes;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Settings
{
    public class DbSettings
    {
        public StorageMode StorageMode { get; set; }
        
        public string StateConnString { get; set; }
        public string LogsConnString { get; set; }
    }
}
