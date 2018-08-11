using System.Threading.Tasks;
using Lykke.Cqrs;
using Lykke.MarginTrading.CommissionService.Contracts;
using Lykke.MarginTrading.CommissionService.Contracts.Commands;
using MarginTrading.CommissionService.Core.Extensions;
using MarginTrading.CommissionService.Core.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Internal;

namespace MarginTrading.CommissionService.Controllers
{
    [Route("api/overnightswap")]
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
        public Task StartOvernightSwapProcess(string operationId, int numberOfFinancingDays, int financingDaysPerYear)
        {
            _cqrsEngine.SendCommand(
                new StartOvernightSwapsProcessCommand(
                    operationId.RequiredNotNullOrWhiteSpace(nameof(operationId)),
                    _systemClock.UtcNow.DateTime,
                    numberOfFinancingDays.RequiredGreaterThan(0, nameof(numberOfFinancingDays)),
                    financingDaysPerYear.RequiredGreaterThan(0, nameof(financingDaysPerYear))),
                _cqrsContextNamesSettings.CommissionService,
                _cqrsContextNamesSettings.CommissionService);
            
            return Task.CompletedTask;
        }
    }
}