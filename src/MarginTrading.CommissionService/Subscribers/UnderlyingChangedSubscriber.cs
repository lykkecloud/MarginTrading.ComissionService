// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Common.Log;
using Lykke.Common;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Snow.Mdm.Contracts.Models.Events;
using MarginTrading.CommissionService.Services.Handlers;

namespace MarginTrading.CommissionService.Subscribers
{
    public class UnderlyingChangedSubscriber : IStartStop
    {
        private readonly UnderlyingChangedHandler _handler;
        private readonly RabbitMqSubscriptionSettings _settings;
        private readonly ILog _log;
        private RabbitMqSubscriber<UnderlyingChangedEvent> _subscriber;

        public UnderlyingChangedSubscriber(UnderlyingChangedHandler handler,
            RabbitMqSubscriptionSettings settings,
            ILog log)
        {
            _handler = handler;
            _settings = settings;
            _log = log;
        }

        public void Start()
        {
            _subscriber = new RabbitMqSubscriber<UnderlyingChangedEvent>(
                    _settings,
                    new DefaultErrorHandlingStrategy(_log, _settings))
                .SetMessageDeserializer(new MessagePackMessageDeserializer<UnderlyingChangedEvent>())
                .SetLogger(_log)
                .Subscribe(ProcessMessageAsync)
                .CreateDefaultBinding()
                .Start();
        }

        public void Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            if (_subscriber != null)
            {
                _subscriber.Stop();
                _subscriber.Dispose();
                _subscriber = null;
            }
        }

        private async Task ProcessMessageAsync(UnderlyingChangedEvent message)
        {
            await _handler.Handle(message);

            _log.WriteInfo(nameof(UnderlyingChangedSubscriber), nameof(ProcessMessageAsync),
                $"Handled event {nameof(UnderlyingChangedEvent)}. Event created at: {message.Timestamp.ToShortTimeString()}");
        }
    }
}