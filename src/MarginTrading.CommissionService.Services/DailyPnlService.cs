// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using MarginTrading.Backend.Contracts.Positions;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using Microsoft.Extensions.Internal;
using MoreLinq;
using StackExchange.Redis;

namespace MarginTrading.CommissionService.Services
{
	/// <inheritdoc />
	/// <summary>
	/// Take care of daily pnl calculation.
	/// </summary>
	public class DailyPnlService : IDailyPnlService
	{
		private const string DistributedLockKey = "CommissionService:DailyPnlProcess";
		
		private readonly IPositionReceiveService _positionReceiveService;
		private readonly IAccountRedisCache _accountRedisCache;
		private readonly IAssetsCache _assetsCache;
		private readonly IDailyPnlHistoryRepository _dailyPnlHistoryRepository;
		private readonly ISystemClock _systemClock;
		private readonly ILog _log;
		private readonly IDatabase _database;
		private readonly CommissionServiceSettings _commissionServiceSettings;
		private readonly ITradingEngineSnapshotRepository _snapshotRepository;
		private readonly IConvertService _convertService;

		public DailyPnlService(
			IPositionReceiveService positionReceiveService,
			IAccountRedisCache accountRedisCache,
			IAssetsCache assetsCache,
			IDailyPnlHistoryRepository dailyPnlHistoryRepository,
			ISystemClock systemClock,
			ILog log,
			IDatabase database,
			CommissionServiceSettings commissionServiceSettings, 
			ITradingEngineSnapshotRepository snapshotRepository, 
			IConvertService convertService)
		{
			_positionReceiveService = positionReceiveService;
			_accountRedisCache = accountRedisCache;
			_assetsCache = assetsCache;
			_dailyPnlHistoryRepository = dailyPnlHistoryRepository;
			_systemClock = systemClock;
			_log = log;
			_database = database;
			_commissionServiceSettings = commissionServiceSettings;
			_snapshotRepository = snapshotRepository;
			_convertService = convertService;
		}
		
		/// <summary>
		/// Filter orders that are already calculated
		/// </summary>
		/// <returns></returns>
		private async Task<IReadOnlyList<IOpenPosition>> GetOrdersForCalculationAsync(DateTime tradingDay)
		{
			var now = _systemClock.UtcNow.UtcDateTime;
			if (tradingDay < now.Date.AddDays(-1))
			{
				await _log.WriteWarningAsync(
					nameof(OvernightSwapService),
					nameof(GetOrdersForCalculationAsync),
					$"Daily Pnl Calculation for tradingDay: {tradingDay:d} has been done already, therefore skipping recalculations.",
					DateTime.UtcNow);

				return new List<OpenPosition>();
			}

			var openPositions = await _snapshotRepository.GetPositionsAsync(tradingDay);

			if (openPositions == null)
			{
				throw new InvalidOperationException(
					$"The positions are not available from snapshot data for {tradingDay.ToString(CultureInfo.InvariantCulture)}");
			}
			
			//prepare the list of orders. Explicit end of the day is ok for DateTime From by requirements.
			var allLast = await _dailyPnlHistoryRepository.GetAsync(tradingDay, null);

			var calculatedIds = allLast.Where(x => x.IsSuccess).Select(x => x.PositionId).ToHashSet();
			//select only non-calculated positions, changed before current invocation time
			var filteredOrders = openPositions.Where(x => !calculatedIds.Contains(x.Id) 
			                                              && x.OpenTimestamp.Date <= tradingDay.Date);

			//detect orders for which last calculation failed and it was closed
			var failedClosedOrders = allLast.Where(x => !x.IsSuccess)
				.Select(x => x.PositionId)
				.Except(openPositions.Select(y => y.Id)).ToList();
			if (failedClosedOrders.Any())
			{
				await _log.WriteErrorAsync(nameof(OvernightSwapService), nameof(GetOrdersForCalculationAsync), new Exception(
						$"Daily PnL calculation failed for some positions and they were closed before recalculation: {string.Join(", ", failedClosedOrders)}."),
					DateTime.UtcNow);
			}

			return filteredOrders.Select(x => _convertService.Convert<OpenPositionContract, OpenPosition>(x)).ToList();
		}

		public async Task<IReadOnlyList<IDailyPnlCalculation>> Calculate(string operationId, DateTime tradingDay)
		{
			if (!await _database.LockTakeAsync(DistributedLockKey, Environment.MachineName,
				_commissionServiceSettings.DistributedLockTimeout))
			{
				throw new Exception("Daily PnL calculation process is already in progress.");
			}

			var resultingCalculations = new List<IDailyPnlCalculation>();
			try
			{
				var openPositions = await GetOrdersForCalculationAsync(tradingDay);
				
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
						
						if (calculation != null)
						{
							resultingCalculations.Add(calculation);
						}
					}
					catch (Exception ex)
					{
						resultingCalculations.Add(ProcessPosition(position, operationId, 
							_systemClock.UtcNow.UtcDateTime, tradingDay, int.MaxValue, ex));
						await _log.WriteErrorAsync(nameof(DailyPnlService), nameof(Calculate),
							$"Error calculating PnL for position: {position?.ToJson()}. Operation : {operationId}", ex);
					}
				}

				await _dailyPnlHistoryRepository.BulkInsertAsync(resultingCalculations);
				
				await _log.WriteInfoAsync(nameof(OvernightSwapService), nameof(Calculate),
					$"Finished, # of calculations: {resultingCalculations.Count}", DateTime.UtcNow);
			}
			finally
			{
				await _database.LockReleaseAsync(DistributedLockKey, Environment.MachineName);
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
		private static DailyPnlCalculation ProcessPosition(IOpenPosition position, string operationId, DateTime now, 
			DateTime tradingDay, int accuracy, Exception exception = null)
		{
			if (exception != null)
			{
				return new DailyPnlCalculation(
					operationId: operationId,
					accountId: position.AccountId,
					instrument: position.AssetPairId,
					time: now,
					tradingDay: tradingDay,
					volume: position.CurrentVolume,
					fxRate: position.FxRate,
					positionId: position.Id,
					pnl: 0,
					rawTotalPnl: 0,
					isSuccess: false,
					exception: exception
				);
			}

			var rawTotalPnl = (position.ClosePrice - position.OpenPrice) * position.CurrentVolume * position.FxRate;
			var unrealizedPnl = Math.Round(rawTotalPnl, accuracy) - Math.Round(position.ChargedPnl, accuracy);
			
			return new DailyPnlCalculation(
				operationId: operationId,
				accountId: position.AccountId,
				instrument: position.AssetPairId,
				time: now,
				tradingDay: tradingDay,
				volume: position.CurrentVolume,
				fxRate: position.FxRate,
				positionId: position.Id,
				pnl: unrealizedPnl,
				rawTotalPnl: rawTotalPnl,
				isSuccess: true
			);
		}
	}
}