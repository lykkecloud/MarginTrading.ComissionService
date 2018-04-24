using System;
using FluentScheduler;
using MarginTrading.OvernightSwapService.Infrastructure;
using MarginTrading.OvernightSwapService.Infrastructure.Implementation;

namespace MarginTrading.OvernightSwapService.Scheduling
{
	/// <summary>
	/// Overnight swaps calculation job.
	/// Take into account, that scheduler might fire the job with delay of 100ms.
	/// </summary>
	public class OvernightSwapJob : IJob, IDisposable
	{

		public OvernightSwapJob()
		{
		}
		
		public void Execute()
		{
			MtServiceLocator.OvernightSwapService.CalculateAndChargeSwaps();
		}

		public void Dispose()
		{
		}
	}
}