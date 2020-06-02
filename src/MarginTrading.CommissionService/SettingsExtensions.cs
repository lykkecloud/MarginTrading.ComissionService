// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using Lykke.Snow.Common.Startup.ApiKey;

namespace MarginTrading.CommissionService
{
    public static class SettingsExtensions
    {
        public static ClientSettings ToGeneric([NotNull] this Core.Settings.ClientSettings src) => new ClientSettings
        {
            ApiKey = src.ApiKey,
            ServiceUrl = src.ServiceUrl
        };
    }
}