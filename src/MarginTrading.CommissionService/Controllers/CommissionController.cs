using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.CommissionService.Controllers
{
	[Authorize]
	[Route("api/commission")]
	public class CommissionController : Controller
	{
		private readonly IConvertService _convertService;
		
		private readonly IOvernightSwapHistoryRepository _overnightSwapHistoryRepository;
		
		public CommissionController(
			IConvertService convertService,
			IOvernightSwapHistoryRepository overnightSwapHistoryRepository)
		{
			_convertService = convertService;
			
			_overnightSwapHistoryRepository = overnightSwapHistoryRepository;
		}
		
		/// <summary>
		/// Retrieve overnight swap calculation history from storage between selected dates.
		/// </summary>
		/// <param name="from"></param>
		/// <param name="to"></param>
		/// <returns></returns>
		[Route("history")]
		[ProducesResponseType(typeof(IEnumerable<IOvernightSwap>), 200)]
		[ProducesResponseType(400)]
		[HttpPost]
		public async Task<IEnumerable<OvernightSwapHistoryContract>> GetOvernightSwapHistory(
			[FromQuery] DateTime from, [FromQuery] DateTime to)
		{
			if (to < from)
				throw new Exception("'From' date must be before 'to' date.");
			
			var data = await _overnightSwapHistoryRepository.GetAsync(from, to);

			return data.Select(x => _convertService.Convert<IOvernightSwap, OvernightSwapHistoryContract>(x));
		}

		/// <summary>
		/// Invoke recalculation of account/instrument/direction order packages that were not calculated successfully last time.
		/// </summary>
		/// <returns></returns>
		[Route("recalc.failed.orders")]
		[ProducesResponseType(200)]
		[ProducesResponseType(400)]
		[HttpPost]
		public Task RecalculateFailedOrders()
		{
			MtServiceLocator.OvernightSwapService.CalculateAndChargeSwaps();
			return Task.CompletedTask;
		}
	}
}