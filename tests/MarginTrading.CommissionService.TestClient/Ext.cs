// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.HttpClientGenerator;

namespace MarginTrading.CommissionService.TestClient
{
    public static class Ext
    {
        public static HttpClientGeneratorBuilder WithOptionalApiKey(this HttpClientGeneratorBuilder builder, string apiKey)
        {
            return !string.IsNullOrWhiteSpace(apiKey) ? builder.WithApiKey(apiKey) : builder;
        }
    }
}