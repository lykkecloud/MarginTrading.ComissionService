using System;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.SettingsReader.Attributes;
using MarginTrading.CommissionService.Core.Settings.Rates;

namespace MarginTrading.CommissionService.Core.Settings
{
    public class CommissionServiceSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
        public ServicesSettings Services { get; set; }
        public CqrsSettings Cqrs { get; set; }

        [Optional, CanBeNull]
        public ChaosSettings ChaosKitty { get; set; }
        
        public DefaultRateSettings DefaultRateSettings { get; set; }
        public EodSettings EodSettings { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public RedisSettings RedisSettings { get; set; }
        
        public int OvernightSwapsChargingTimeoutSec { get; set; }
        public int DailyPnlsChargingTimeoutSec { get; set; }
    }
}
