using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.SettingsReader;
using MarginTrading.OvernightSwapService.Settings;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.OvernightSwapService.Infrastructure.Implementation
{
    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = {new StringEnumConverter()}
        };

        private readonly ILog _logger;

        private readonly ConcurrentDictionary<string, IStopable> _subscribers =
            new ConcurrentDictionary<string, IStopable>();

        private readonly ConcurrentDictionary<RabbitMqSubscriptionSettings, Lazy<IStopable>> _producers =
            new ConcurrentDictionary<RabbitMqSubscriptionSettings, Lazy<IStopable>>(
                new SubscriptionSettingsEqualityComparer());

        public RabbitMqService(ILog logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            foreach (var stoppable in _subscribers.Values)
                stoppable.Stop();
            foreach (var stoppable in _producers.Values)
                stoppable.Value.Stop();
        }

        public IRabbitMqSerializer<TMessage> GetJsonSerializer<TMessage>()
        {
            return new JsonMessageSerializer<TMessage>(Encoding.UTF8, JsonSerializerSettings);
        }

        public IRabbitMqSerializer<TMessage> GetMsgPackSerializer<TMessage>()
        {
            return new MessagePackMessageSerializer<TMessage>();
        }

        public IMessageProducer<TMessage> GetProducer<TMessage>(IReloadingManager<RabbitConnectionSettings> settings,
            bool isDurable, IRabbitMqSerializer<TMessage> serializer)
        {
            // on-the fly connection strings switch is not supported currently for rabbitMq
            var currSettings = settings.CurrentValue;
            var subscriptionSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = currSettings.ConnectionString,
                ExchangeName = currSettings.ExchangeName,
                IsDurable = isDurable,
            };

            return (IMessageProducer<TMessage>) _producers.GetOrAdd(subscriptionSettings, CreateProducer).Value;

            Lazy<IStopable> CreateProducer(RabbitMqSubscriptionSettings s)
            {
                // Lazy ensures RabbitMqPublisher will be created and started only once
                // https://andrewlock.net/making-getoradd-on-concurrentdictionary-thread-safe-using-lazy/
                return new Lazy<IStopable>(() => new RabbitMqPublisher<TMessage>(s).DisableInMemoryQueuePersistence()
                    .SetSerializer(serializer)
                    .SetLogger(_logger)
                    .Start());
            }
        }

        public void Subscribe<TMessage>(IReloadingManager<RabbitConnectionSettings> settings, bool isDurable,
            Func<TMessage, Task> handler)
        {
            // on-the fly connection strings switch is not supported currently for rabbitMq
            var currSettings = settings.CurrentValue;
            var subscriptionSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = currSettings.ConnectionString,
                QueueName =
                    $"{currSettings.ExchangeName}.{PlatformServices.Default.Application.ApplicationName}",
                ExchangeName = currSettings.ExchangeName,
                IsDurable = isDurable,
            };

            var rabbitMqSubscriber = new RabbitMqSubscriber<TMessage>(subscriptionSettings,
                    new DefaultErrorHandlingStrategy(_logger, subscriptionSettings))
                .SetMessageDeserializer(new JsonMessageDeserializer<TMessage>(JsonSerializerSettings))
                .Subscribe(handler)
                .SetLogger(_logger);

            if (!_subscribers.TryAdd(subscriptionSettings.QueueName, rabbitMqSubscriber))
            {
                throw new InvalidOperationException(
                    $"A subscriber for queue {subscriptionSettings.QueueName} was already initialized");
            }

            rabbitMqSubscriber.Start();
        }

        /// <remarks>
        ///     ReSharper auto-generated
        /// </remarks>
        private sealed class SubscriptionSettingsEqualityComparer : IEqualityComparer<RabbitMqSubscriptionSettings>
        {
            public bool Equals(RabbitMqSubscriptionSettings x, RabbitMqSubscriptionSettings y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.ConnectionString, y.ConnectionString) &&
                       string.Equals(x.ExchangeName, y.ExchangeName);
            }

            public int GetHashCode(RabbitMqSubscriptionSettings obj)
            {
                unchecked
                {
                    return ((obj.ConnectionString != null ? obj.ConnectionString.GetHashCode() : 0) * 397) ^
                           (obj.ExchangeName != null ? obj.ExchangeName.GetHashCode() : 0);
                }
            }
        }
    }
}