// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using JetBrains.Annotations;
using MarginTrading.Backend.Contracts.Events;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Workflow
{
    /// <summary>
    /// Listens to <see cref="MarginEventMessage"/>s and creates BAFIN C&C files for it.
    /// </summary>
    [UsedImplicitly]
    public class AccountMarginEventsProjection
    {
        private readonly ICostsAndChargesGenerationService _costsAndChargesGenerationService;

        public AccountMarginEventsProjection(ICostsAndChargesGenerationService costsAndChargesGenerationService)
        {
            _costsAndChargesGenerationService = costsAndChargesGenerationService;
        }

        public async Task Handle(MarginEventMessage message)
        {
            await _costsAndChargesGenerationService.GenerateForAccount(message.AccountId, false);
        }
    }
}