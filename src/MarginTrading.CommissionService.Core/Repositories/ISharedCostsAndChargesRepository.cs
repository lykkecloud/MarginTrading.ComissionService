// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain;

namespace MarginTrading.CommissionService.Core.Repositories
{
    public interface ISharedCostsAndChargesRepository
    {
        Task SaveAsync(SharedCostsAndChargesCalculation calculation);
    }
}