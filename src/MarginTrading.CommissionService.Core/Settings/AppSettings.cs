// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace MarginTrading.CommissionService.Core.Settings
{
    [UsedImplicitly]
    public class AppSettings
    {
        public CommissionServiceSettings CommissionService { get; set; }
        
        [Optional]
        public ClientSettings CommissionServiceClient { get; set; } = new ClientSettings();
    }
}
