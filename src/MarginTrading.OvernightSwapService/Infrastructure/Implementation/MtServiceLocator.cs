using MarginTrading.OvernightSwapService.Services;

namespace MarginTrading.OvernightSwapService.Infrastructure.Implementation
{
    public class MtServiceLocator
    {
        public static IOvernightSwapService OvernightSwapService { get; set; }
    }
}