using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
	/// <summary>
	/// Take care of daily pnl calculation.
	/// </summary>
	public class DailyPnlService : IDailyPnlService
	{
		private readonly IPositionReceiveService _positionReceiveService;
		private readonly ILog _log;

		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

		public DailyPnlService(
			IPositionReceiveService positionReceiveService,
			ILog log)
		{
			_positionReceiveService = positionReceiveService;
			_log = log;
		}

		public async Task<IReadOnlyList<IDailyPnlCalculation>> Calculate(string operationId, DateTime tradingDay)
		{
			var openPositions = (await _positionReceiveService.GetActive()).ToList();

			await _semaphore.WaitAsync();

			var resultingCalculations = new List<IDailyPnlCalculation>();
			try
			{
				await _log.WriteInfoAsync(nameof(DailyPnlService), nameof(Calculate),
					$"Started, # of positions: {openPositions.Count}.", DateTime.UtcNow);

				foreach (var position in openPositions)
				{
					try
					{
						var calculation = await ProcessPosition(position, operationId, tradingDay);
						if(calculation != null)
							resultingCalculations.Add(calculation);
					}
					catch (Exception ex)
					{
						_log.WriteWarning(nameof(DailyPnlService), position?.ToJson(), "Error calculating PnL", ex);
					}
				}
				
				await _log.WriteInfoAsync(nameof(OvernightSwapService), nameof(Calculate),
					$"Finished, # of calculations: {resultingCalculations.Count}", DateTime.UtcNow);
			}
			finally
			{
				_semaphore.Release();
			}

			return resultingCalculations;
		}

		/// <summary>
		/// Calculate daily pnl for position.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="operationId"></param>
		/// <param name="tradingDay"></param>
		/// <returns></returns>
		private async Task<DailyPnlCalculation> ProcessPosition(IOpenPosition position,
			string operationId, DateTime tradingDay)
		{
			return new DailyPnlCalculation(
				operationId: operationId,
				accountId: position.AccountId,
				instrument: position.AssetPairId,
				tradingDay: tradingDay,
				volume: position.CurrentVolume,
				fxRate: position.FxRate,
				positionId: position.Id,
			    pnl: position.PnL - position.ChargedPnl);
		}
	}
}