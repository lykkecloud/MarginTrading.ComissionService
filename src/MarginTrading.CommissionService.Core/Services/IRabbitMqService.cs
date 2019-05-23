using System;
using System.Threading.Tasks;
using Common;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using MarginTrading.CommissionService.Core.Settings;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IRabbitMqService
    {
        IMessageProducer<TMessage> GetProducer<TMessage>(RabbitConnectionSettings settings,
            bool isDurable, IRabbitMqSerializer<TMessage> serializer);

        void Subscribe<TMessage>(RabbitConnectionSettings settings, bool isDurable, Func<TMessage, Task> handler,
            IMessageDeserializer<TMessage> deserializer,
            string instanceId = null);

        IRabbitMqSerializer<TMessage> GetJsonSerializer<TMessage>();
        IRabbitMqSerializer<TMessage> GetMsgPackSerializer<TMessage>();
        IMessageDeserializer<TMessage> GetJsonDeserializer<TMessage>();
        IMessageDeserializer<TMessage> GetMsgPackDeserializer<TMessage>();
    }
}