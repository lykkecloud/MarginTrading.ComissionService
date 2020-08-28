// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using MarginTrading.AssetService.Contracts;
using MarginTrading.AssetService.Contracts.Rates;
using MarginTrading.CommissionService.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.CommissionService.Controllers
{
    [Authorize]
    [Route("api/rates")]
    [MiddlewareFilter(typeof(RequestLoggingPipeline))]
    public class RateSettingsController : Controller
    {
        private readonly IRateSettingsApi _rateSettingsApi;

        public RateSettingsController(
            IRateSettingsApi rateSettingsApi)
        {
            _rateSettingsApi = rateSettingsApi;
        }

        [ProducesResponseType(typeof(IReadOnlyList<OrderExecutionRateContract>), 200)]
        [ProducesResponseType(400)]
        [HttpGet("get-order-exec")]
        public async Task<IReadOnlyList<OrderExecutionRateContract>> GetOrderExecutionRates()
        {
            return await _rateSettingsApi.GetOrderExecutionRatesAsync();
        }

        [ProducesResponseType(typeof(OrderExecutionRateContract), 200)]
        [ProducesResponseType(400)]
        [HttpGet("get-order-exec/{assetPairId}")]
        public async Task<OrderExecutionRateContract> GetOrderExecutionRate(string assetPairId)
        {
            return await _rateSettingsApi.GetOrderExecutionRateAsync(assetPairId);
        }

        [ProducesResponseType(typeof(IReadOnlyList<OrderExecutionRateContract>), 200)]
        [ProducesResponseType(400)]
        [HttpGet("get-order-exec/list")]
        public async Task<IReadOnlyList<OrderExecutionRateContract>> GetOrderExecutionRates([FromQuery] string[] assetPairIds)
        {
            return await _rateSettingsApi.GetOrderExecutionRatesAsync(assetPairIds);
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
            await _rateSettingsApi.ReplaceOrderExecutionRatesAsync(rates);
        }

        
        
        [ProducesResponseType(typeof(IReadOnlyList<OvernightSwapRateContract>), 200)]
        [ProducesResponseType(400)]
        [HttpGet("get-overnight-swap")]
        public async Task<IReadOnlyList<OvernightSwapRateContract>> GetOvernightSwapRates()
        {
            return await _rateSettingsApi.GetOvernightSwapRatesAsync();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("replace-overnight-swap")]
        public async Task ReplaceOvernightSwapRates([FromBody] OvernightSwapRateContract[] rates)
        {
            await _rateSettingsApi.ReplaceOvernightSwapRatesAsync(rates);
        }

        
        
        [ProducesResponseType(typeof(OnBehalfRateContract), 200)]
        [ProducesResponseType(400)]
        [HttpGet("get-on-behalf")]
        public async Task<OnBehalfRateContract> GetOnBehalfRate()
        {
            return await _rateSettingsApi.GetOnBehalfRateAsync();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [HttpPost("replace-on-behalf")]
        public async Task ReplaceOnBehalfRate([FromBody] OnBehalfRateContract rate)
        {
            await _rateSettingsApi.ReplaceOnBehalfRateAsync(rate);
        }
    }
}