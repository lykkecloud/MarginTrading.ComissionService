namespace MarginTrading.CommissionService.Core.Settings
{
    public class RabbitMqConsumersSettings
    {
        public RabbitConnectionSettings FxRateRabbitMqSettings { get; set; }
        
        public RabbitConnectionSettings OrderExecutedSettings { get; set; }
        
        public RabbitConnectionSettings SettingsChanged { get; set; }
    }
}
