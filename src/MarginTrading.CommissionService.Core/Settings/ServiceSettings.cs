// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using Lykke.SettingsReader.Attributes;

namespace MarginTrading.CommissionService.Core.Settings
{
    public class ServiceSettings
    {
        [HttpCheck("/api/isalive")]
        public string Url { get; set; }
        
        [Optional]
        public string ApiKey { get; set; }
    }
}