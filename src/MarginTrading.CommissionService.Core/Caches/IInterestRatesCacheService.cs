// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Caches
{
    public interface IInterestRatesCacheService
    {
        decimal GetRate(string id);

        void InitCache();
    }
}