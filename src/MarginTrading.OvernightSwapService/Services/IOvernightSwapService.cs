namespace MarginTrading.OvernightSwapService.Services
{
    /// <summary>
    /// Take care of overnight swap calculation and charging.
    /// </summary>
    public interface IOvernightSwapService
    {
        /// <summary>
        /// Scheduler entry point for overnight swaps calculation. Successfully calculated swaps are immediately charged.
        /// </summary>
        /// <returns></returns>
        void CalculateAndChargeSwaps();

        /// <summary>
        /// Fire at app start. Initialize cache from storage. Detect if calc was missed and invoke it if needed.
        /// </summary>
        void Start();
    }
}