// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using MarginTrading.CommissionService.Core.Settings;

namespace MarginTrading.CommissionService.Extensions
{
    public static class RabbitExtensions
    {
        public static RabbitSubscriptionSettings AppendToQueueName(this RabbitSubscriptionSettings settings, string value)
        {
            settings.QueueName = $"{settings.QueueName}-{value}";

            return settings;
        }
    }
}