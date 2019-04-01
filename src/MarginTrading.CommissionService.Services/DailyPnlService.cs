using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using MarginTrading.CommissionService.Core.Caches;
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
		private readonly IAccountRedisCache _accountRedisCache;
		private readonly IAssetsCache _assetsCache;
		private readonly ILog _log;

		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

		public DailyPnlService(
			IPositionReceiveService positionReceiveService,
			IAccountRedisCache accountRedisCache,
			IAssetsCache assetsCache,
			ILog log)
		{
			_positionReceiveService = positionReceiveService;
			_accountRedisCache = accountRedisCache;
			_assetsCache = assetsCache;
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

				var accounts = (await _accountRedisCache.GetAccounts()).ToDictionary(x => x.Id);

				foreach (var position in openPositions)
				{
					try
					{
						if (!accounts.TryGetValue(position.AccountId, out var account))
						{
							_log.Error(nameof(DailyPnlService), 
								new Exception($"Account {position.AccountId} does not exist in cache."));
						}
						var accuracy = _assetsCache.GetAccuracy(account?.BaseAssetId);
						
						var calculation = ProcessPosition(position, operationId, tradingDay, accuracy);
						
						if (calculation != null)
						{
							resultingCalculations.Add(calculation);
						}
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
		/// <param name="accuracy"></param>
		/// <returns></returns>
		private static DailyPnlCalculation ProcessPosition(IOpenPosition position,
			string operationId, DateTime tradingDay, int? accuracy)
		{
			var value = position.PnL - position.ChargedPnl;

			return new DailyPnlCalculation(
				operationId: operationId,
				accountId: position.AccountId,
				instrument: position.AssetPairId,
				tradingDay: tradingDay,
				volume: position.CurrentVolume,
				fxRate: position.FxRate,
				positionId: position.Id,
				pnl: accuracy.HasValue
					? Math.Round(value, accuracy.Value)
					: value
			);
		}
	}
}