﻿// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ICachedCalculation<out TResult>
    {
        TResult Get();
    }
}