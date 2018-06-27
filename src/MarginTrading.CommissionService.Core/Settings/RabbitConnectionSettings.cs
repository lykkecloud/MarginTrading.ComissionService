using Lykke.SettingsReader.Attributes;

namespace MarginTrading.CommissionService.Core.Settings
{
    public class RabbitConnectionSettings
    {
        [AmqpCheck]
        public string ConnectionString { get; set; }
        
        public string ExchangeName { get; set; }
        
        [Optional]
        public string RoutingKey { get; set; }
    }
}
