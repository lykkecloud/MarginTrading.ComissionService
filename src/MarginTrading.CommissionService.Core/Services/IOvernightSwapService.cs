using System.Threading.Tasks;

namespace MarginTrading.CommissionService.Core.Services
{
    /// <summary>
    /// Take care of overnight swap calculation and charging.
    /// </summary>
    public interface IOvernightSwapService
    {
        /// <summary>
        /// Entry point for overnight swaps calculation. Successfully calculated swaps are immediately charged.
        /// </summary>
        /// <returns></returns>
        Task CalculateAndChargeSwaps();
    }
}