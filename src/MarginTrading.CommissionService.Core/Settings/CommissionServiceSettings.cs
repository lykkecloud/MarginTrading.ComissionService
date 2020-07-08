// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.SettingsReader.Attributes;
using MarginTrading.CommissionService.Core.Settings.Rates;

namespace MarginTrading.CommissionService.Core.Settings
{
    [UsedImplicitly]
    public class CommissionServiceSettings
    {
        public DbSettings Db { get; set; }
        
        public RabbitMqSettings RabbitMq { get; set; }
        
        public ServicesSettings Services { get; set; }
        
        public CqrsSettings Cqrs { get; set; }

        [Optional, CanBeNull]
        public ChaosSettings ChaosKitty { get; set; }
        
        public DefaultRateSettings DefaultRateSettings { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public RedisSettings RedisSettings { get; set; }
        
        public int OvernightSwapsChargingTimeoutSec { get; set; }

        [Optional] 
        public TimeSpan OvernightSwapsRetryTimeout { get; set; } = TimeSpan.FromMinutes(1);
        
        public int DailyPnlsChargingTimeoutSec { get; set; }

        [Optional] 
        public TimeSpan DailyPnlsRetryTimeout { get; set; } = TimeSpan.FromMinutes(1);
        
        [Optional]
        public bool UseSerilog { get; set; }

        [Optional]
        public TimeSpan DistributedLockTimeout { get; set; } = TimeSpan.FromHours(12);

        [Optional]
        public string InstanceId { get; set; }
        
        public SignatureSettings SignatureSettings { get; set; }

        public RequestLoggerSettings RequestLoggerSettings { get; set; }

        [Optional]
        public CostsAndChargesDefaultSettings CostsAndChargesDefaults { get; set; } =
            new CostsAndChargesDefaultSettings();
        
        public ReportSettings ReportSettings { get; set; }
        
        [Optional]
        public string AppInsightsInstrumentationKey { get; set; }
    }
}
