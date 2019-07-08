// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using JetBrains.Annotations;

namespace Lykke.MarginTrading.CommissionService.Contracts.Api
{
    [UsedImplicitly] // from startup.cs only in release configuration
    public class ErrorResponse
    {
        [UsedImplicitly]
        public string ErrorMessage { get; set; }

        [UsedImplicitly]
        public string Details { get; set; }
    }
}