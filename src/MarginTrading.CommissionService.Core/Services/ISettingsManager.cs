using System.Threading.Tasks;
using MarginTrading.SettingsService.Contracts.Messages;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ISettingsManager
    {
        Task HandleSettingsChanged(SettingsChangedEvent evt);
    }
}