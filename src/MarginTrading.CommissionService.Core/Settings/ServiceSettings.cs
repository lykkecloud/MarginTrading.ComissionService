using Lykke.SettingsReader.Attributes;

namespace MarginTrading.CommissionService.Core.Settings
{
    public class ServiceSettings
    {
        [HttpCheck("/api/isalive")]
        public string Url { get; set; }
        
        [Optional]
        public string ApiKey { get; set; }
    }
}