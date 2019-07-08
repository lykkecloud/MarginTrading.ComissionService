// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Settings
{
    public class RabbitMqSettings
    {
        public RabbitMqPublishersSettings Publishers { get; set; }
        public RabbitMqConsumersSettings Consumers { get; set; }
    }
}