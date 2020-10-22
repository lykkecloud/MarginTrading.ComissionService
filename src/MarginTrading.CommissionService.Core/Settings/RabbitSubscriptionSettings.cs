// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.RabbitMqBroker.Subscriber;

namespace MarginTrading.CommissionService.Core.Settings
{
    public class RabbitSubscriptionSettings
    {
        public string RoutingKey { get; set; }
        public bool IsDurable { get; set; }
        public string ExchangeName { get; set; }
        public string QueueName { get; set; }
        public string ConnectionString { get; set; }

        public static implicit operator RabbitMqSubscriptionSettings(RabbitSubscriptionSettings subscriptionSettings)
        {
            return new RabbitMqSubscriptionSettings
            {
                RoutingKey = subscriptionSettings.RoutingKey,
                IsDurable = subscriptionSettings.IsDurable,
                ExchangeName = subscriptionSettings.ExchangeName,
                QueueName = subscriptionSettings.QueueName,
                ConnectionString = subscriptionSettings.ConnectionString
            };
        }
    }
}