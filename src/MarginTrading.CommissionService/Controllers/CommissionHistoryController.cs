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
	[Route("api/commission")]
	public class CommissionHistoryController : Controller, ICommissionHistoryApi
	{
		private readonly IOvernightSwapHistoryRepository _overnightSwapHistoryRepository;
		
		public CommissionHistoryController(
			IOvernightSwapHistoryRepository overnightSwapHistoryRepository)
		{
			_overnightSwapHistoryRepository = overnightSwapHistoryRepository;
		}

		/// <inheritdoc />
		[Route("history")]
		[ProducesResponseType(typeof(List<OvernightSwapHistoryContract>), 200)]
		[ProducesResponseType(400)]
		[HttpGet]
		public async Task<List<OvernightSwapHistoryContract>> GetOvernightSwapHistory(
			[FromQuery] DateTime from, [FromQuery] DateTime to)
		{
			if (to < from)
				throw new Exception("'From' date must be before 'to' date.");
			
			var data = await _overnightSwapHistoryRepository.GetAsync(from, to);

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
	}
}