// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;
using Lykke.Snow.Common.Startup.ApiKey;

namespace MarginTrading.CommissionService.Core.Settings
{
    public class AppSettings
    {
        public CommissionServiceSettings CommissionService { get; set; }
        
        [Optional, CanBeNull]
        public ClientSettings CommissionServiceClient { get; set; } = new ClientSettings();
    }
}
