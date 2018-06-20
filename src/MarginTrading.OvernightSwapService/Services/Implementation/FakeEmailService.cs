using System.Threading.Tasks;
using MarginTrading.OvernightSwapService.Models;

namespace MarginTrading.OvernightSwapService.Services.Implementation
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