using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Lykke.MarginTrading.CommissionService.Contracts;
using Lykke.MarginTrading.CommissionService.Contracts.Models;
using MarginTrading.CommissionService.Core;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Rates;
using MarginTrading.CommissionService.Core.Repositories;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Swagger;

namespace MarginTrading.CommissionService.Controllers
{
    [Authorize]
    [Route("api/rates")]
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

        [ProducesResponseType(typeof(OrderExecutionRateContract), 200)]
        [ProducesResponseType(400)]
        [Description("Get order execution rate. If accountId not set, or it's order exec rates are not set then Trading Profile value is returned.")]
        [HttpGet("get-order-exec/{assetPairId}")]
        public async Task<OrderExecutionRateContract> GetOrderExecutionRate(
            [FromRoute] string assetPairId, [FromQuery] string accountId = "")
        {
            var profileId = string.IsNullOrWhiteSpace(accountId) ? RateSettingsService.TradingProfile : accountId;
            var data = await _rateSettingsService.GetOrderExecutionRate(profileId, assetPairId);

            if (data == null && profileId != RateSettingsService.TradingProfile)
            {
                data = await _rateSettingsService.GetOrderExecutionRate(
                    RateSettingsService.TradingProfile, assetPairId);
            }
            
            return data == null ? null : Map(data);
        }

        [ProducesResponseType(typeof(IReadOnlyList<OrderExecutionRateContract>), 200)]
        [ProducesResponseType(400)]
        [Description("Get order execution rates")]
        [HttpGet("get-order-exec")]
        public async Task<IReadOnlyList<OrderExecutionRateContract>> GetOrderExecutionRates()
        {
            return (await _rateSettingsService.GetOrderExecutionRates())
                   ?.Select(Map).ToList() ?? new List<OrderExecutionRateContract>();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Description("Insert or update existing order execution rates")]
        [HttpPost("replace-order-exec")]
        public async Task ReplaceOrderExecutionRates([FromBody] OrderExecutionRateContract[] rates)
        {
            if (rates == null || !rates.Any())
            {
                throw new ArgumentNullException(nameof(rates));
            }

            if (rates.Any(x => string.IsNullOrWhiteSpace(x.AssetPairId)
                               || string.IsNullOrWhiteSpace(x.CommissionAsset)))
            {
                throw new ArgumentException("Wrong parameters: AssetPairId and CommissionAsset must be set", nameof(rates));
            }

            await _rateSettingsService.ReplaceOrderExecutionRates(rates.Select(Map).ToList());
        }


        [ProducesResponseType(typeof(IReadOnlyList<OvernightSwapRateContract>), 200)]
        [ProducesResponseType(400)]
        [Description("Get overnight swap rates")]
        [HttpGet("get-overnight-swap")]
        public async Task<IReadOnlyList<OvernightSwapRateContract>> GetOvernightSwapRates()
        {
            return (await _rateSettingsService.GetOvernightSwapRatesForApi())
                   ?.Select(x => _convertService.Convert<OvernightSwapRate, OvernightSwapRateContract>(x)).ToList()
                   ?? new List<OvernightSwapRateContract>();
        }

        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Description("Insert or update existing overnight swap rates")]
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
        [Description("Get on behalf rate. If accountId not set Trading Profile value is returned.")]
        [HttpGet("get-on-behalf")]
        public async Task<OnBehalfRateContract> GetOnBehalfRate([FromQuery] string accountId = "")
        {
            var item = await _rateSettingsService.GetOnBehalfRate(
                string.IsNullOrWhiteSpace(accountId) ? RateSettingsService.TradingProfile : accountId);
            return item == null ? null : Map(item);
        }

        [ProducesResponseType(typeof(IReadOnlyList<OnBehalfRateContract>), 200)]
        [ProducesResponseType(400)]
        [Description("Get all on behalf rates")]
        [HttpGet("get-all-on-behalf")]
        public async Task<IReadOnlyList<OnBehalfRateContract>> GetOnBehalfRates()
        {
            var items = await _rateSettingsService.GetOnBehalfRates();
            return items.Select(Map).ToList();
        }

        [Obsolete("Use replace-all-on-behalf")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Description("Insert or update existing on behalf rate")]
        [HttpPost("replace-on-behalf")]
        public async Task ReplaceOnBehalfRate([FromBody] OnBehalfRateContract rate)
        {
            if (string.IsNullOrWhiteSpace(rate?.CommissionAsset))
            {
                throw new ArgumentNullException(nameof(rate.CommissionAsset));
            }

            var toReplace = Map(rate);

            var all = (await _rateSettingsService.GetOnBehalfRates()).ToList();
            all.RemoveAll(x => x.TradingConditionId == toReplace.TradingConditionId);
            all.Add(toReplace);

            await _rateSettingsService.ReplaceOnBehalfRates(all);
        }
        
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [Description("Insert or update all on behalf rates")]
        [HttpPost("replace-all-on-behalf")]
        public async Task ReplaceAllOnBehalfRate([FromBody] List<OnBehalfRateContract> rates)
        {
            foreach (var rate in rates)
            {
                if (string.IsNullOrWhiteSpace(rate?.CommissionAsset))
                {
                    throw new ArgumentNullException(nameof(rate.CommissionAsset), $"{nameof(rate.CommissionAsset)} must be set");
                }
            }

            await _rateSettingsService.ReplaceOnBehalfRates(rates.Select(Map).ToList());
        }

        private OrderExecutionRateContract Map(OrderExecutionRate orderExecutionRate) => new OrderExecutionRateContract
        {
            TradingConditionId = orderExecutionRate.TradingConditionId,
            AssetPairId = orderExecutionRate.AssetPairId,
            CommissionCap = orderExecutionRate.CommissionCap,
            CommissionFloor = orderExecutionRate.CommissionFloor,
            CommissionRate = orderExecutionRate.CommissionRate,
            CommissionAsset = orderExecutionRate.CommissionAsset,
            LegalEntity = orderExecutionRate.LegalEntity,
        };

        private OrderExecutionRate Map(OrderExecutionRateContract orderExecutionRate) => new OrderExecutionRate
        {
            TradingConditionId = string.IsNullOrWhiteSpace(orderExecutionRate.TradingConditionId)
                ? RateSettingsService.TradingProfile
                : orderExecutionRate.TradingConditionId,
            AssetPairId = orderExecutionRate.AssetPairId,
            CommissionCap = orderExecutionRate.CommissionCap,
            CommissionFloor = orderExecutionRate.CommissionFloor,
            CommissionRate = orderExecutionRate.CommissionRate,
            CommissionAsset = orderExecutionRate.CommissionAsset,
            LegalEntity = orderExecutionRate.LegalEntity,
        };

        private OnBehalfRateContract Map(OnBehalfRate onBehalfRate) => new OnBehalfRateContract
        {
            TradingConditionId = onBehalfRate.TradingConditionId,
            Commission = onBehalfRate.Commission,
            CommissionAsset = onBehalfRate.CommissionAsset,
            LegalEntity = onBehalfRate.LegalEntity,
        };

        private OnBehalfRate Map(OnBehalfRateContract onBehalfRate) => new OnBehalfRate
        {
            TradingConditionId = string.IsNullOrWhiteSpace(onBehalfRate.TradingConditionId)
                ? RateSettingsService.TradingProfile
                : onBehalfRate.TradingConditionId,
            Commission = onBehalfRate.Commission,
            CommissionAsset = onBehalfRate.CommissionAsset,
            LegalEntity = onBehalfRate.LegalEntity,
        };
    }
}