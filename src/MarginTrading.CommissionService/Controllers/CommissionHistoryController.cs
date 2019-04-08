using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.MarginTrading.CommissionService.Contracts;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MarginTrading.CommissionService.Core.Extensions;

namespace MarginTrading.CommissionService.Controllers
{
	/// <inheritdoc cref="ICommissionHistoryApi" />
	/// Manages commission history
	[Route("api/commission")]
	public class CommissionHistoryController : Controller, ICommissionHistoryApi
	{
		private readonly IOvernightSwapHistoryRepository _overnightSwapHistoryRepository;
		private readonly IDailyPnlHistoryRepository _dailyPnlHistoryRepository;
		
		public CommissionHistoryController(
			IOvernightSwapHistoryRepository overnightSwapHistoryRepository,
			IDailyPnlHistoryRepository dailyPnlHistoryRepository)
		{
			_overnightSwapHistoryRepository = overnightSwapHistoryRepository;
			_dailyPnlHistoryRepository = dailyPnlHistoryRepository;
		}

		/// <inheritdoc />
		[Route("history")]
		[Obsolete]
		[ProducesResponseType(typeof(List<OvernightSwapHistoryContract>), 200)]
		[ProducesResponseType(400)]
		[HttpGet]
		public async Task<List<OvernightSwapHistoryContract>> GetOvernightSwapHistory(
			[FromQuery] DateTime from, [FromQuery] DateTime to)
		{
			return await GetOvernightSwapHistoryV2(from, to);
		}

		/// <inheritdoc />
		[Route("overnight-swap")]
		[ProducesResponseType(typeof(List<OvernightSwapHistoryContract>), 200)]
		[ProducesResponseType(400)]
		[HttpGet]
		public async Task<List<OvernightSwapHistoryContract>> GetOvernightSwapHistoryV2(
			[FromQuery] DateTime from, [FromQuery] DateTime to)
		{
			if (to < from)
				throw new Exception("'From' date must be before 'to' date.");
			
			var data = await _overnightSwapHistoryRepository.GetAsync(from, to);

			return data.Select(Convert).ToList();
		}
		
		/// <inheritdoc />
		[Route("daily-pnl")]
		[ProducesResponseType(typeof(List<DailyPnlHistoryContract>), 200)]
		[ProducesResponseType(400)]
		[HttpGet]
		public async Task<List<DailyPnlHistoryContract>> GetDailyPnlHistory(
			[FromQuery] DateTime @from, [FromQuery] DateTime to)
		{
			if (to < from)
				throw new Exception("'From' date must be before 'to' date.");
			
			var data = await _dailyPnlHistoryRepository.GetAsync(from, to);

			return data.Select(Convert).ToList();
		}

		private OvernightSwapHistoryContract Convert(IOvernightSwapCalculation overnightSwapCalculation)
		{
			return new OvernightSwapHistoryContract
			{
				Id = overnightSwapCalculation.Id,
				OperationId = overnightSwapCalculation.OperationId,
				AccountId = overnightSwapCalculation.AccountId,
				Instrument = overnightSwapCalculation.Instrument,
				Direction = overnightSwapCalculation.Direction?.ToType<PositionDirectionContract>(),
				Time = overnightSwapCalculation.Time,
				Volume = overnightSwapCalculation.Volume,
				SwapValue = overnightSwapCalculation.SwapValue,
				PositionId = overnightSwapCalculation.PositionId,
				Details = overnightSwapCalculation.Details,
				TradingDay = overnightSwapCalculation.TradingDay,
				IsSuccess = overnightSwapCalculation.IsSuccess,
				Exception = overnightSwapCalculation.Exception,
				WasCharged = overnightSwapCalculation.WasCharged,
			};
		}

		private DailyPnlHistoryContract Convert(IDailyPnlCalculation dailyPnlCalculation)
		{
			return new DailyPnlHistoryContract
			{
				Id = dailyPnlCalculation.Id,
				OperationId = dailyPnlCalculation.OperationId,
				AccountId = dailyPnlCalculation.AccountId,
				Instrument = dailyPnlCalculation.Instrument,
				Time = dailyPnlCalculation.Time,
				TradingDay = dailyPnlCalculation.TradingDay,
				Volume = dailyPnlCalculation.Volume,
				FxRate = dailyPnlCalculation.FxRate,
				PositionId = dailyPnlCalculation.PositionId,
				Pnl = dailyPnlCalculation.Pnl,
				WasCharged = dailyPnlCalculation.WasCharged,
			};
		}
	}
}