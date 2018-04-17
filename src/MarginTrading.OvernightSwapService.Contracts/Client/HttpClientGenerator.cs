using JetBrains.Annotations;
using Refit;

namespace MarginTrading.OvernightSwapService.Contracts.Client
{
    [PublicAPI]
    public class HttpClientGenerator
    {
        private readonly string _url;
        private readonly RefitSettings _refitSettings;

        public HttpClientGenerator(string url, string userAgent)
        {
            _url = url;
            var httpMessageHandler = new UserAgentHttpClientHandler(userAgent);
            _refitSettings = new RefitSettings {HttpMessageHandlerFactory = () => httpMessageHandler};
        }

        public TApiInterface Generate<TApiInterface>()
        {
            return RestService.For<TApiInterface>(_url, _refitSettings);
        }
    }
}