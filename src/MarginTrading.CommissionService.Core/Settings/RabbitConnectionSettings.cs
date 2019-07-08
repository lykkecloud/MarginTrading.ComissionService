// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

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

        [Optional] 
        public int ConsumerCount { get; set; } = 1;
    }
}
