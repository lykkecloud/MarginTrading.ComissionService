using System;
using System.Collections.Generic;
using System.Linq;
using Lykke.Messaging;
using Lykke.Messaging.Contract;

namespace MarginTrading.CommissionService.Services
{
    public sealed class DefaultHeadersMessagingEngine : IMessagingEngine
    {
        private readonly IMessagingEngine messagingEngine;
        private readonly IDictionary<string, string> defaultHeaders;

        public DefaultHeadersMessagingEngine(IMessagingEngine messagingEngine, IDictionary<string, string> defaultHeaders)
        {
            this.messagingEngine = messagingEngine;
            this.defaultHeaders = defaultHeaders;
        }

        public ISerializationManager SerializationManager => this.messagingEngine.SerializationManager;

        public void AddProcessingGroup(string name, ProcessingGroupInfo info)
        {
            this.messagingEngine.AddProcessingGroup(name, info);
        }

        public Destination CreateTemporaryDestination(string transportId, string processingGroup)
        {
            return this.messagingEngine.CreateTemporaryDestination(transportId, processingGroup);
        }

        public void Dispose()
        {
            this.messagingEngine.Dispose();
        }

        public bool GetProcessingGroupInfo(string name, out ProcessingGroupInfo groupInfo)
        {
            return this.messagingEngine.GetProcessingGroupInfo(name, out groupInfo);
        }

        public string GetStatistics()
        {
            return this.messagingEngine.GetStatistics();
        }

        public IDisposable RegisterHandler<TRequest, TResponse>(Func<TRequest, TResponse> handler, Endpoint endpoint) where TResponse : class
        {
            return this.messagingEngine.RegisterHandler(handler, endpoint);
        }

        public void Send<TMessage>(TMessage message, Endpoint endpoint, string processingGroup = null, Dictionary<string, string> headers = null)
        {
            var messageHeaders = headers ?? new Dictionary<string, string>();
            messageHeaders = messageHeaders.Concat(defaultHeaders).ToDictionary(x => x.Key, x => x.Value);

            this.messagingEngine.Send(message, endpoint, processingGroup, messageHeaders);
        }

        public void Send<TMessage>(TMessage message, Endpoint endpoint, int ttl, string processingGroup = null, Dictionary<string, string> headers = null)
        {
            var messageHeaders = headers ?? new Dictionary<string, string>();
            messageHeaders = messageHeaders.Concat(defaultHeaders).ToDictionary(x => x.Key, x => x.Value);

            this.messagingEngine.Send(message, endpoint, ttl, processingGroup, messageHeaders);
        }

        public void Send(object message, Endpoint endpoint, string processingGroup = null, Dictionary<string, string> headers = null)
        {
            var messageHeaders = headers ?? new Dictionary<string, string>();
            messageHeaders = messageHeaders.Concat(defaultHeaders).ToDictionary(x => x.Key, x => x.Value);

            this.messagingEngine.Send(message, endpoint, processingGroup, messageHeaders);
        }

        public TResponse SendRequest<TRequest, TResponse>(TRequest request, Endpoint endpoint, long timeout = 30000)
        {
            return this.messagingEngine.SendRequest<TRequest, TResponse>(request, endpoint, timeout);
        }

        public IDisposable SendRequestAsync<TRequest, TResponse>(TRequest request, Endpoint endpoint, Action<TResponse> callback, Action<Exception> onFailure, long timeout = 30000, string processingGroup = null)
        {
            return this.messagingEngine.SendRequestAsync(request, endpoint, callback, onFailure, timeout, processingGroup);
        }

        public IDisposable Subscribe<TMessage>(Endpoint endpoint, Action<TMessage> callback)
        {
            return this.messagingEngine.Subscribe(endpoint, callback);
        }

        public IDisposable Subscribe<TMessage>(Endpoint endpoint, CallbackDelegate<TMessage> callback, string processingGroup = null, int priority = 0)
        {
            return this.messagingEngine.Subscribe(endpoint, callback, processingGroup, priority);
        }

        public IDisposable Subscribe(Endpoint endpoint, Action<object> callback, Action<string> unknownTypeCallback, params Type[] knownTypes)
        {
            return this.messagingEngine.Subscribe(endpoint, callback, unknownTypeCallback, knownTypes);
        }

        public IDisposable Subscribe(Endpoint endpoint, Action<object> callback, Action<string> unknownTypeCallback, string processingGroup, int priority = 0, params Type[] knownTypes)
        {
            return this.messagingEngine.Subscribe(endpoint, callback, unknownTypeCallback, processingGroup, priority, knownTypes);
        }

        public IDisposable Subscribe(Endpoint endpoint, CallbackDelegate<object> callback, Action<string, AcknowledgeDelegate> unknownTypeCallback, params Type[] knownTypes)
        {
            return this.messagingEngine.Subscribe(endpoint, callback, unknownTypeCallback, knownTypes);
        }

        public IDisposable Subscribe(Endpoint endpoint, CallbackDelegate<object> callback, Action<string, AcknowledgeDelegate> unknownTypeCallback, string processingGroup, int priority = 0, params Type[] knownTypes)
        {
            return this.messagingEngine.Subscribe(endpoint, callback, unknownTypeCallback, processingGroup, priority, knownTypes);
        }

        public IDisposable SubscribeOnTransportEvents(TransportEventHandler handler)
        {
            return this.messagingEngine.SubscribeOnTransportEvents(handler);
        }

        public bool VerifyEndpoint(Endpoint endpoint, EndpointUsage usage, bool configureIfRequired, out string error)
        {
            return this.messagingEngine.VerifyEndpoint(endpoint, usage, configureIfRequired, out error);
        }
    }
}