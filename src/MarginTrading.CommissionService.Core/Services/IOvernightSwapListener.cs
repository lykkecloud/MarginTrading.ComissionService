// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IOvernightSwapListener
    {
        Task OvernightSwapStateChanged(string operationId, bool chargedOrFailed);
    }
}