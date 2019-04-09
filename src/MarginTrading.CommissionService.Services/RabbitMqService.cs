using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.SettingsReader;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MarginTrading.CommissionService.Services
{
    public class RabbitMqService : IRabbitMqService, IDisposable
    {
        private readonly ILog _logger;
        private readonly IConsole _consoleWriter;

        private readonly ConcurrentDictionary<(RabbitMqSubscriptionSettings, int), IStopable> _subscribers =
            new ConcurrentDictionary<(RabbitMqSubscriptionSettings, int), IStopable>(new SubscriptionSettingsWithNumberEqualityComparer());

        private readonly ConcurrentDictionary<RabbitMqSubscriptionSettings, IStopable> _producers =
            new ConcurrentDictionary<RabbitMqSubscriptionSettings, IStopable>(
                new SubscriptionSettingsEqualityComparer());

        public RabbitMqService(ILog logger, IConsole consoleWriter)
        {
            _logger = logger;
            _consoleWriter = consoleWriter;
        }

        public void Dispose()
        {
            foreach (var stoppable in _subscribers.Values)
                stoppable.Stop();
            foreach (var stoppable in _producers.Values)
                stoppable.Stop();
        }

        public IRabbitMqSerializer<TMessage> GetJsonSerializer<TMessage>()
        {
            return new JsonMessageSerializer<TMessage>();
        }

        public IRabbitMqSerializer<TMessage> GetMsgPackSerializer<TMessage>()
        {
            return new MessagePackMessageSerializer<TMessage>();
        }
        
        public IMessageDeserializer<TMessage> GetJsonDeserializer<TMessage>()
        {
            return new DeserializerWithErrorLogging<TMessage>(_logger);
        }

        public IMessageDeserializer<TMessage> GetMsgPackDeserializer<TMessage>()
        {
            return new MessagePackMessageDeserializer<TMessage>();
        }

        public IMessageProducer<TMessage> GetProducer<TMessage>(RabbitConnectionSettings settings,
            bool isDurable, IRabbitMqSerializer<TMessage> serializer)
        {
            // on-the fly connection strings switch is not supported currently for rabbitMq
            var subscriptionSettings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = settings.ConnectionString,
                ExchangeName = settings.ExchangeName,
                RoutingKey = settings.RoutingKey ?? string.Empty,
                IsDurable = isDurable,
            };

            return (IMessageProducer<TMessage>) _producers.GetOrAdd(subscriptionSettings, CreateProducer);

            IStopable CreateProducer(RabbitMqSubscriptionSettings s)
            {
                var publisher = new RabbitMqPublisher<TMessage>(s);

                publisher.DisableInMemoryQueuePersistence();

                return publisher
                    .SetSerializer(serializer)
                    .SetLogger(_logger)
                    .SetConsole(_consoleWriter)
                    .Start();
            }
        }

        public void Subscribe<TMessage>(RabbitConnectionSettings settings, bool isDurable,
            Func<TMessage, Task> handler, IMessageDeserializer<TMessage> deserializer)
        {
            var consumerCount = settings.ConsumerCount <= 0 ? 1 : settings.ConsumerCount;
            
            foreach (var consumerNumber in Enumerable.Range(1, consumerCount))
            {
                var subscriptionSettings = new RabbitMqSubscriptionSettings
                {
                    ConnectionString = settings.ConnectionString,
                    QueueName = QueueHelper.BuildQueueName(settings.ExchangeName, null),
                    ExchangeName = settings.ExchangeName,
                    IsDurable = isDurable,
                };
                
                var rabbitMqSubscriber = new RabbitMqSubscriber<TMessage>(subscriptionSettings,
                        new DefaultErrorHandlingStrategy(_logger, subscriptionSettings))
                    .SetMessageDeserializer(deserializer)
                    .Subscribe(handler)
                    .SetLogger(_logger);

                if (!_subscribers.TryAdd((subscriptionSettings, consumerNumber), rabbitMqSubscriber))
                {
                    throw new InvalidOperationException(
                        $"Subscriber #{consumerNumber} for queue {subscriptionSettings.QueueName} was already initialized");
                }

                rabbitMqSubscriber.Start();
            }
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
        
        /// <remarks>
        ///     ReSharper auto-generated
        /// </remarks>
        private sealed class SubscriptionSettingsWithNumberEqualityComparer : IEqualityComparer<(RabbitMqSubscriptionSettings, int)>
        {
            public bool Equals((RabbitMqSubscriptionSettings, int) x, (RabbitMqSubscriptionSettings, int) y)
            {
                if (ReferenceEquals(x.Item1, y.Item1) && x.Item2 == y.Item2) return true;
                if (ReferenceEquals(x.Item1, null)) return false;
                if (ReferenceEquals(y.Item1, null)) return false;
                if (x.Item1.GetType() != y.Item1.GetType()) return false;
                return string.Equals(x.Item1.ConnectionString, y.Item1.ConnectionString)
                       && string.Equals(x.Item1.ExchangeName, y.Item1.ExchangeName)
                       && x.Item2 == y.Item2;
            }

            public int GetHashCode((RabbitMqSubscriptionSettings, int) obj)
            {
                unchecked
                {
                    return ((obj.Item1.ConnectionString != null ? obj.Item1.ConnectionString.GetHashCode() : 0) * 397) ^
                           (obj.Item1.ExchangeName != null ? obj.Item1.ExchangeName.GetHashCode() : 0);
                }
            }
        }
    }
}