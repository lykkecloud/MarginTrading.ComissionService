// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common;
using Lykke.Cqrs;
using Lykke.SettingsReader;
using MarginTrading.Backend.Contracts.Positions;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Caches;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.SettingsService.Contracts;
using MarginTrading.SettingsService.Contracts.AssetPair;
using Microsoft.Extensions.Internal;
using MoreLinq;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace MarginTrading.CommissionService.Services
{
	/// <inheritdoc />
	/// <summary>
	/// Take care of overnight swap calculation and charging.
	/// </summary>
	public class OvernightSwapService : IOvernightSwapService
	{
		private const string DistributedLockKey = "CommissionService:OvernightSwapProcess";
		
		private readonly ICommissionCalcService _commissionCalcService;
		private readonly IOvernightSwapHistoryRepository _overnightSwapHistoryRepository;
		private readonly IPositionReceiveService _positionReceiveService;
		private readonly ISystemClock _systemClock;
		private readonly ILog _log;
		private readonly IDatabase _database;
		private readonly CommissionServiceSettings _commissionServiceSettings;
		private readonly IInterestRatesCacheService _interestRatesCacheService;
		private readonly ITradingEngineSnapshotRepository _snapshotRepository;
		private readonly IConvertService _convertService;

		public OvernightSwapService(
			ICommissionCalcService commissionCalcService,
			IOvernightSwapHistoryRepository overnightSwapHistoryRepository,
			IPositionReceiveService positionReceiveService,
			ISystemClock systemClock,
			ILog log,
			IDatabase database,
			CommissionServiceSettings commissionServiceSettings,
			IInterestRatesCacheService interestRatesCacheService, 
			ITradingEngineSnapshotRepository snapshotRepository, 
			IConvertService convertService)
		{
			_commissionCalcService = commissionCalcService;
			_overnightSwapHistoryRepository = overnightSwapHistoryRepository;
			_positionReceiveService = positionReceiveService;
			_systemClock = systemClock;
			_log = log;
			_database = database;
			_commissionServiceSettings = commissionServiceSettings;
			_interestRatesCacheService = interestRatesCacheService;
			_snapshotRepository = snapshotRepository;
			_convertService = convertService;
		}

		/// <summary>
		/// Filter orders that are already calculated
		/// </summary>
		/// <returns></returns>
		private async Task<IReadOnlyList<IOpenPosition>> GetOrdersForCalculationAsync(DateTime tradingDay)
		{
			var openPositions = await _snapshotRepository.GetPositionsAsync(tradingDay);

			if (openPositions == null)
			{
				throw new InvalidOperationException(
					$"The positions are not available from snapshot data for {tradingDay.ToString(CultureInfo.InvariantCulture)}");
			}
			
			//prepare the list of positions
			var allLast = await _overnightSwapHistoryRepository.GetAsync(tradingDay, tradingDay.AddDays(1));

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
						$"Overnight swap calculation failed for some positions and they were closed before recalculation: {string.Join(", ", failedClosedOrders)}."),
					DateTime.UtcNow);
			}

			return filteredOrders.Select(x => _convertService.Convert<OpenPositionContract, OpenPosition>(x)).ToList();
		}

		public async Task<IReadOnlyList<IOvernightSwapCalculation>> Calculate(string operationId,
			DateTime creationTimestamp, int numberOfFinancingDays, int financingDaysPerYear, DateTime tradingDay)
		{
			if (!await _database.LockTakeAsync(DistributedLockKey, Environment.MachineName,
				_commissionServiceSettings.DistributedLockTimeout))
			{
				throw new Exception("Overnight swap calculation process is already in progress.");
			}

			var resultingCalculations = new List<IOvernightSwapCalculation>();
			try
			{
				var filteredPositions = await GetOrdersForCalculationAsync(tradingDay);

				await _log.WriteInfoAsync(nameof(OvernightSwapService), nameof(Calculate),
					$"Started, # of positions: {filteredPositions.Count}.", DateTime.UtcNow);

				// We need to re-init cache because new rates should have been uploaded according to the workflow
				_interestRatesCacheService.InitCache();
				
				foreach (var position in filteredPositions)
				{
					try
					{
						var calculation = await ProcessPosition(position, operationId, 
							numberOfFinancingDays, financingDaysPerYear, tradingDay);
						if (calculation != null)
						{
							resultingCalculations.Add(calculation);
						}
					}
					catch (Exception ex)
					{
						resultingCalculations.Add(await ProcessPosition(position, operationId, 
							numberOfFinancingDays, financingDaysPerYear, tradingDay, ex));
						await _log.WriteErrorAsync(nameof(OvernightSwapService), nameof(Calculate),
							$"Error calculating swaps for position: {position?.ToJson()}. Operation : {operationId}", ex);
					}
				}

				await _overnightSwapHistoryRepository.BulkInsertAsync(resultingCalculations);
				
				await _log.WriteInfoAsync(nameof(OvernightSwapService), nameof(Calculate),
					$"Finished, # of successful calculations: {resultingCalculations.Count(x => x.IsSuccess)}, # of failed: {resultingCalculations.Count(x => !x.IsSuccess)}.", DateTime.UtcNow);
			}
			finally
			{
				await _database.LockReleaseAsync(DistributedLockKey, Environment.MachineName);
			}

			return resultingCalculations;
		}

		/// <summary>
		/// Calculate overnight swap
		/// </summary>
		private async Task<IOvernightSwapCalculation> ProcessPosition(IOpenPosition position,
			string operationId, int numberOfFinancingDays, int financingDaysPerYear, DateTime tradingDay,
			Exception exception = null)
		{
			if (exception != null)
			{
				return new OvernightSwapCalculation(
					operationId: operationId,
					accountId: position.AccountId,
					instrument: position.AssetPairId,
					direction: position.Direction,
					time: _systemClock.UtcNow.DateTime,
					volume: position.CurrentVolume,
					swapValue: default,
					positionId: position.Id,
					details: null,
					tradingDay: tradingDay,
					isSuccess: false,
					exception: exception);
			}

			var (swap, details) = await _commissionCalcService.GetOvernightSwap(position.AccountId,
				position.AssetPairId, position.CurrentVolume, position.ClosePrice, position.FxRate, position.Direction,
				numberOfFinancingDays, financingDaysPerYear);

			return new OvernightSwapCalculation(
				operationId: operationId,
				accountId: position.AccountId,
				instrument: position.AssetPairId,
				direction: position.Direction,
				time: _systemClock.UtcNow.DateTime,
				volume: position.CurrentVolume,
				swapValue: swap,
				positionId: position.Id,
				details: details,
				tradingDay: tradingDay,
				isSuccess: true);
		}

		public async Task<int> SetWasCharged(string positionOperationId, bool type)
		{
			return await _overnightSwapHistoryRepository.SetWasCharged(positionOperationId, type);
		}

		public async Task<(int Total, int Failed, int NotProcessed)> GetOperationState(string id)
		{
			//may be position charge id or operation id
			var operationId = OvernightSwapCalculation.ExtractOperationId(id);

			return await _overnightSwapHistoryRepository.GetOperationState(operationId);
		}
	}
}