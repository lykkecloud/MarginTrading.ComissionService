using System;
using System.Threading.Tasks;
using Common;
using Lykke.RabbitMqBroker.Publisher;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IRabbitMqService
    {
        IMessageProducer<TMessage> GetProducer<TMessage>(string connectionString, string exchangeName,
            bool isDurable, IRabbitMqSerializer<TMessage> serializer);

        void Subscribe<TMessage>(string connectionString, string exchangeName, bool isDurable,
            Func<TMessage, Task> handler);

        IRabbitMqSerializer<TMessage> GetJsonSerializer<TMessage>();
        IRabbitMqSerializer<TMessage> GetMsgPackSerializer<TMessage>();
    }
}