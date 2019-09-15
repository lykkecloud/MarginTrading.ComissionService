// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Settings
{
    public class RabbitMqConsumersSettings
    {
        public RabbitConnectionSettings FxRateRabbitMqSettings { get; set; }
        
        public RabbitConnectionSettings QuotesRabbitMqSettings { get; set; }
        
        public RabbitConnectionSettings OrderExecutedSettings { get; set; }
        
        public RabbitConnectionSettings SettingsChanged { get; set; }
    }
}
