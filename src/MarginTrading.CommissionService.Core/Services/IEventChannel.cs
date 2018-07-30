namespace MarginTrading.CommissionService.Core.Services
{
    public interface IEventChannel<TEventArgs>
    {
        void SendEvent(object sender, TEventArgs ea);
    }
}