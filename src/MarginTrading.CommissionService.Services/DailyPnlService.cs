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
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Services
{
	/// <inheritdoc />
	/// <summary>
	/// Take care of daily pnl calculation.
	/// </summary>
	public class DailyPnlService : IDailyPnlService
	{
		private readonly IPositionReceiveService _positionReceiveService;
		private readonly IAccountRedisCache _accountRedisCache;
		private readonly IAssetsCache _assetsCache;
		private readonly IDailyPnlHistoryRepository _dailyPnlHistoryRepository;
		private readonly ISystemClock _systemClock;
		private readonly ILog _log;

		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

		public DailyPnlService(
			IPositionReceiveService positionReceiveService,
			IAccountRedisCache accountRedisCache,
			IAssetsCache assetsCache,
			IDailyPnlHistoryRepository dailyPnlHistoryRepository,
			ISystemClock systemClock,
			ILog log)
		{
			_positionReceiveService = positionReceiveService;
			_accountRedisCache = accountRedisCache;
			_assetsCache = assetsCache;
			_dailyPnlHistoryRepository = dailyPnlHistoryRepository;
			_systemClock = systemClock;
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
						
						var calculation = ProcessPosition(position, operationId, _systemClock.UtcNow.UtcDateTime, tradingDay, accuracy);
						
						if (calculation != null && calculation.Pnl != 0) // skip all zero pnls
						{
							resultingCalculations.Add(calculation);
						}
					}
					catch (Exception ex)
					{
						_log.WriteWarning(nameof(DailyPnlService), position?.ToJson(), "Error calculating PnL", ex);
					}
				}

				await _dailyPnlHistoryRepository.BulkInsertAsync(resultingCalculations);
				
				await _log.WriteInfoAsync(nameof(OvernightSwapService), nameof(Calculate),
					$"Finished, # of calculations: {resultingCalculations.Count}", DateTime.UtcNow);
			}
			finally
			{
				_semaphore.Release();
			}

			return resultingCalculations;
		}

		public async Task<int> SetWasCharged(string positionOperationId, bool type)
		{
			return await _dailyPnlHistoryRepository.SetWasCharged(positionOperationId, type);
		}

		public async Task<(int Total, int Failed, int NotProcessed)> GetOperationState(string id)
		{
			//may be position charge id or operation id
			var operationId = DailyPnlCalculation.ExtractOperationId(id);

			return await _dailyPnlHistoryRepository.GetOperationState(operationId);
		}

		/// <summary>
		/// Calculate daily pnl for position.
		/// </summary>
		private static DailyPnlCalculation ProcessPosition(IOpenPosition position,
			string operationId, DateTime now, DateTime tradingDay, int? accuracy)
		{
			var value = position.PnL - position.ChargedPnl;

			return new DailyPnlCalculation(
				operationId: operationId,
				accountId: position.AccountId,
				instrument: position.AssetPairId,
				time: now,
				tradingDay: tradingDay,
				volume: position.CurrentVolume,
				fxRate: position.FxRate,
				positionId: position.Id,
				pnl: accuracy.HasValue
					? Math.Round(value, accuracy.Value)
					: value,
				wasCharged: null
			);
		}
	}
}