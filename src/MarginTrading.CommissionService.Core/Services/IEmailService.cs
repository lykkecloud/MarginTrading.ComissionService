using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IEmailService
    {
        Task SendOvernightSwapEmailAsync(string email, OvernightSwapNotification notification);
    }
}