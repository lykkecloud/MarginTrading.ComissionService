using System.Threading.Tasks;
using MarginTrading.OvernightSwapService.Models;
using MarginTrading.OvernightSwapService.Services.Implementation;

namespace MarginTrading.OvernightSwapService.Services
{
    public interface IEmailService
    {
        Task SendOvernightSwapEmailAsync(string email, OvernightSwapNotification notification);
    }
}