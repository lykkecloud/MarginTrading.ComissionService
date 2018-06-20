using System;

namespace MarginTrading.OvernightSwapService.Settings
{
    public class MarginTradingOvernightSwapServiceSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
        public ServicesSettings Services { get; set; }
        
        public bool SendOvernightSwapEmails { get; set; }
        public TimeSpan OvernightSwapCalculationTime { get; set; }
    }
}
