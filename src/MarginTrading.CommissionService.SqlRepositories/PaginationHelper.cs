// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.CommissionService.SqlRepositories
{
    public static class PaginationHelper
    {
        private const int MaxResults = 100;
        private const int UnspecifiedResults = 20;

        public static int GetTake(int? take)
        {
            return take == null
                ? UnspecifiedResults
                : Math.Min(take.Value, MaxResults);
        }
    }
}