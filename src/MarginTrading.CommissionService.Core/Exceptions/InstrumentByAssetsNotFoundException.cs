// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System;

namespace MarginTrading.CommissionService.Core.Exceptions
{
    public class InstrumentByAssetsNotFoundException : Exception
    {
        public string Asset1 { get; private set; }
        public string Asset2 { get; private set; }

        public InstrumentByAssetsNotFoundException(string asset1, string asset2, string message) : base(message)
        {
            Asset1 = asset1;
            Asset2 = asset2;
        }
    }
}