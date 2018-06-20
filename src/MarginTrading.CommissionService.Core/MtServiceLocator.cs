using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Core
{
    public class MtServiceLocator
    {
        public static IOvernightSwapService OvernightSwapService { get; set; }
    }
}