using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class FakeEmailService : IEmailService
    {
        //TODO provide external impl of email sender via messages
        public Task SendOvernightSwapEmailAsync(string email, OvernightSwapNotification notification)
        {
            return Task.CompletedTask;
        }
    }
}