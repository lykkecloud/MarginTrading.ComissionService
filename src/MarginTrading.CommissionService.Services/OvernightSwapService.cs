using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common;
using Lykke.SettingsReader;
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
using Newtonsoft.Json;

namespace MarginTrading.CommissionService.Services
{
	/// <summary>
	/// Take care of overnight swap calculation and charging.
	/// </summary>
	public class OvernightSwapService : IOvernightSwapService
	{
		private readonly IAssetPairsApi _assetPairsApi;
		private readonly ICommissionCalcService _commissionCalcService;
		
		private readonly IOvernightSwapHistoryRepository _overnightSwapHistoryRepository;
		private readonly IPositionReceiveService _positionReceiveService;
		private readonly IThreadSwitcher _threadSwitcher;
		private readonly ISystemClock _systemClock;
		private readonly IConvertService _convertService;
		private readonly IReloadingManager<CommissionServiceSettings> _marginSettings;
		private readonly ILog _log;

		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

		private DateTime _currentStartTimestamp;

		public OvernightSwapService(
			IAssetPairsApi assetPairsApi,
			ICommissionCalcService commissionCalcService,
			
			IOvernightSwapHistoryRepository overnightSwapHistoryRepository,
			IPositionReceiveService positionReceiveService,
			IThreadSwitcher threadSwitcher,
			ISystemClock systemClock,
			IConvertService convertService,
			IReloadingManager<CommissionServiceSettings> marginSettings,
			ILog log)
		{
			_assetPairsApi = assetPairsApi;
			_commissionCalcService = commissionCalcService;
			
			_overnightSwapHistoryRepository = overnightSwapHistoryRepository;
			_positionReceiveService = positionReceiveService;
			_threadSwitcher = threadSwitcher;
			_systemClock = systemClock;
			_convertService = convertService;
			_marginSettings = marginSettings;
			_log = log;
		}

		/*public void Start()
		{
			//initialize cache from storage
			var savedState = _overnightSwapStateRepository.GetAsync().GetAwaiter().GetResult().ToList();
			_overnightSwapCache.Initialize(savedState.Select(OvernightSwapCalculation.Create));

			//start calculation
			CalculateAndChargeSwaps();
		}*/

		/// <summary>
		/// Filter orders that are already calculated
		/// </summary>
		/// <returns></returns>
		private async Task<IReadOnlyList<OpenPosition>> GetOrdersForCalculationAsync()
		{
			var openPositions = (await _positionReceiveService.GetActive()).ToList();
			
			//prepare the list of orders
			var lastInvocationTime = CalcLastInvocationTime();

			var allLast = await _overnightSwapHistoryRepository.GetAsync(lastInvocationTime, null);
			var calculatedIds = allLast.Where(x => x.IsSuccess).Select(x => x.PositionId).ToHashSet();
			//select only non-calculated positions, changed before current invocation time
			var filteredOrders = openPositions.Where(x => !calculatedIds.Contains(x.Id));

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
			
			return filteredOrders.ToList();
		}

		public async Task<IReadOnlyList<IOvernightSwapCalculation>> Calculate(string operationId, DateTime creationTimestamp)
		{
			_currentStartTimestamp = _systemClock.UtcNow.DateTime;

			var filteredPositions = await GetOrdersForCalculationAsync();
			
			//start calculation in a separate thread
			var resultingCalculations = new List<IOvernightSwapCalculation>();
			_threadSwitcher.SwitchThread(async () =>
			{
				await _semaphore.WaitAsync();

				try
				{
					await _log.WriteInfoAsync(nameof(OvernightSwapService), nameof(Calculate),
						$"Started, # of positions: {filteredPositions.Count}.", DateTime.UtcNow);

					var assetPairs = (await _assetPairsApi.List())
						.Select(x => _convertService.Convert<AssetPairContract, AssetPair>(x)).ToList();
					
					resultingCalculations.Clear();
					var streamCode = 0;
					foreach (var position in filteredPositions)
					{
						var concreteOperationId = $"{operationId}_{streamCode++}";
						try
						{
							var assetPair = assetPairs.First(x => x.Id == position.AssetPairId);
							var calculation = await ProcessPosition(position, assetPair, concreteOperationId);
							if(calculation != null)
								resultingCalculations.Add(calculation);
						}
						catch (Exception ex)
						{
							resultingCalculations.Add(await ProcessPosition(position, null, concreteOperationId, ex));
						}
					}
					
					await _log.WriteInfoAsync(nameof(OvernightSwapService), nameof(Calculate),
						$"Finished, # of successful calculations: {resultingCalculations.Count(x => x.IsSuccess)}, # of failed: {resultingCalculations.Count(x => !x.IsSuccess)}.", DateTime.UtcNow);
				}
				finally
				{
					_semaphore.Release();
				}
			});
			return resultingCalculations;
		}

		/// <summary>
		/// Calculate overnight swaps for account/instrument/direction order package.
		/// </summary>
		/// <param name="position"></param>
		/// <param name="assetPair"></param>
		/// <param name="operationId"></param>
		/// <param name="exception"></param>
		/// <returns></returns>
		private async Task<IOvernightSwapCalculation> ProcessPosition(IOpenPosition position, IAssetPair assetPair,
			string operationId, Exception exception = null)
		{
			var calculation = exception == null
				? new OvernightSwapCalculation(
					operationId: operationId,
					accountId: position.AccountId,
					instrument: position.AssetPairId,
					direction: position.Direction,
					time: _systemClock.UtcNow.DateTime,
					volume: position.CurrentVolume,
					swapValue: _commissionCalcService.GetOvernightSwap(position, assetPair),
					positionId: position.Id,
					isSuccess: true)
				: new OvernightSwapCalculation(
					operationId: operationId,
					accountId: position.AccountId,
					instrument: position.AssetPairId,
					direction: position.Direction,
					time: _systemClock.UtcNow.DateTime,
					volume: position.CurrentVolume,
					swapValue: default(decimal),
					positionId: position.Id,
					isSuccess: false,
					exception: exception);
			
			await _overnightSwapHistoryRepository.AddAsync(calculation);
			
			return calculation;
		}

		/// <summary>
		/// Return last invocation time.
		/// </summary>
		private DateTime CalcLastInvocationTime()
		{
			var dt = _currentStartTimestamp;
			(int Hours, int Minutes) settingsCalcTime = (_marginSettings.CurrentValue.EodSettings.EndOfDayTime.Hours,
				_marginSettings.CurrentValue.EodSettings.EndOfDayTime.Minutes);
			
			var result = new DateTime(dt.Year, dt.Month, dt.Day, settingsCalcTime.Hours, settingsCalcTime.Minutes, 0)
				.AddDays(dt.Hour > settingsCalcTime.Hours || (dt.Hour == settingsCalcTime.Hours && dt.Minute >= settingsCalcTime.Minutes) 
					? 0 : -1);
			return result;
		}
	}
}