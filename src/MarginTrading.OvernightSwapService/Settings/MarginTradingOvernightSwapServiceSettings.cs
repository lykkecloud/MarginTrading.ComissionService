namespace MarginTrading.OvernightSwapService.Settings
{
    internal class MarginTradingOvernightSwapServiceSettings
    {
        public DbSettings Db { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
    }
}
