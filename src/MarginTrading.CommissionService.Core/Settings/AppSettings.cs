using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;
using Lykke.Snow.Common.Startup.ApiKey;

namespace MarginTrading.CommissionService.Core.Settings
{
    public class AppSettings
    {
        public CommissionServiceSettings CommissionService { get; set; }
        
        [Optional, CanBeNull]
        public ClientSettings CommissionServiceClient { get; set; } = new ClientSettings();
    }
}
