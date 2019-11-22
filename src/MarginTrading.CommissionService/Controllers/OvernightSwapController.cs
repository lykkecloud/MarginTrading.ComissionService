// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Lykke.Cqrs;
using Lykke.MarginTrading.CommissionService.Contracts;
using Lykke.MarginTrading.CommissionService.Contracts.Commands;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Controllers
{
    [Authorize]
    [Route("api/overnight-swap")]
    [Route("api/overnightswap")]
    [MiddlewareFilter(typeof(RequestLoggingPipeline))]
    public class OvernightSwapController : Controller, IOvernightSwapApi
    {
        private readonly ICqrsEngine _cqrsEngine;
        private readonly CqrsContextNamesSettings _cqrsContextNamesSettings;
        private readonly ISystemClock _systemClock;

        public OvernightSwapController(ICqrsEngine cqrsEngine,
            CqrsContextNamesSettings cqrsContextNamesSettings,
            ISystemClock systemClock)
        {
            _cqrsEngine = cqrsEngine;
            _cqrsContextNamesSettings = cqrsContextNamesSettings;
            _systemClock = systemClock;
        }

        [Route("start")]
        [HttpPost]
        public Task StartOvernightSwapProcess(string operationId, 
            int numberOfFinancingDays = 0, int financingDaysPerYear = 0, DateTime tradingDay = default)
        {
            tradingDay = tradingDay != default
                ? DateTime.SpecifyKind(tradingDay.Date, DateTimeKind.Utc)
                : _systemClock.UtcNow.Date;
            
            _cqrsEngine.SendCommand(
                new StartOvernightSwapsProcessCommand(
                    operationId.RequiredNotNullOrWhiteSpace(nameof(operationId)),
                    _systemClock.UtcNow.DateTime,
                    numberOfFinancingDays,
                    financingDaysPerYear,
                    tradingDay),
                _cqrsContextNamesSettings.CommissionService,
                _cqrsContextNamesSettings.CommissionService);
            
            return Task.CompletedTask;
        }
    }
}