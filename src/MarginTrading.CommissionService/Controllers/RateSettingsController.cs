// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.MarginTrading.CommissionService.Contracts;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using MarginTrading.CommissionService.Core.Domain.Rates;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.CommissionService.Controllers
{
    [Authorize]
    [Route("api/rates")]
    [MiddlewareFilter(typeof(RequestLoggingPipeline))]
    public class RateSettingsController : Controller, IRateSettingsApi
    {
        private readonly IRateSettingsService _rateSettingsService;
        private readonly IConvertService _convertService;

        public RateSettingsController(
            IRateSettingsService rateSettingsService,
            IConvertService convertService)
        {
            _rateSettingsService = rateSettingsService;
            _convertService = convertService;
        }

        [ProducesResponseType(typeof(IReadOnlyList<OrderExecutionRateContract>), 200)]
        [ProducesResponseType(400)]
        [HttpGet("get-order-exec")]
        public async Task<IReadOnlyList<OrderExecutionRateContract>> GetOrderExecutionRates()
        {
            return (await _rateSettingsService.GetOrderExecutionRatesForApi())
                ?.Select(x => _convertService.Convert<OrderExecutionRate, OrderExecutionRateContract>(x)).ToList()
                   ?? new List<OrderExecutionRateContract>();
        }

        [ProducesResponseType(typeof(OrderExecutionRateContract), 200)]
        [ProducesResponseType(400)]
        [HttpGet("get-order-exec/{assetPairId}")]
        public async Task<OrderExecutionRateContract> GetOrderExecutionRate(string assetPairId)
        {
            var executionRate = await _rateSettingsService.GetOrderExecutionRate(assetPairId);

            if (executionRate == null)
                return null;

            return _convertService.Convert<OrderExecutionRate, OrderExecutionRateContract>(executionRate);
        }

        /// <summary>
        /// Replace order execution rates
        /// </summary>
        /// <param name="rates"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("replace-order-exec")]
        public async Task ReplaceOrderExecutionRates([FromBody] OrderExecutionRateContract[] rates)
        {
            if (rates == null || !rates.Any() || rates.Any(x => 
                    string.IsNullOrWhiteSpace(x.AssetPairId)
                    || string.IsNullOrWhiteSpace(x.CommissionAsset)))
            {
                throw new ArgumentNullException(nameof(rates));
            }

            await _rateSettingsService.ReplaceOrderExecutionRates(rates
                .Select(x => _convertService.Convert<OrderExecutionRateContract, OrderExecutionRate>(x))
                .ToList());
        }

        
        
        [ProducesResponseType(typeof(IReadOnlyList<OvernightSwapRateContract>), 200)]
        [ProducesResponseType(400)]
        [HttpGet("get-overnight-swap")]
        public async Task<IReadOnlyList<OvernightSwapRateContract>> GetOvernightSwapRates()
        {
            return (await _rateSettingsService.GetOvernightSwapRatesForApi())
                   ?.Select(x => _convertService.Convert<OvernightSwapRate, OvernightSwapRateContract>(x)).ToList()
                   ?? new List<OvernightSwapRateContract>();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("replace-overnight-swap")]
        public async Task ReplaceOvernightSwapRates([FromBody] OvernightSwapRateContract[] rates)
        {
            if (rates == null || !rates.Any() || rates.Any(x => 
                    string.IsNullOrWhiteSpace(x.AssetPairId)))
            {
                throw new ArgumentNullException(nameof(rates));
            }

            await _rateSettingsService.ReplaceOvernightSwapRates(rates
                .Select(x => _convertService.Convert<OvernightSwapRateContract, OvernightSwapRate>(x))
                .ToList());
        }

        
        
        [ProducesResponseType(typeof(OnBehalfRateContract), 200)]
        [ProducesResponseType(400)]
        [HttpGet("get-on-behalf")]
        public async Task<OnBehalfRateContract> GetOnBehalfRate()
        {
            var item = await _rateSettingsService.GetOnBehalfRate();
            return item == null ? null : _convertService.Convert<OnBehalfRate, OnBehalfRateContract>(item);
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("replace-on-behalf")]
        public async Task ReplaceOnBehalfRate([FromBody] OnBehalfRateContract rate)
        {
            if (string.IsNullOrWhiteSpace(rate.CommissionAsset))
            {
                throw new ArgumentNullException(nameof(rate.CommissionAsset));
            }

            await _rateSettingsService.ReplaceOnBehalfRate(
                _convertService.Convert<OnBehalfRateContract, OnBehalfRate>(rate));
        }
    }
}