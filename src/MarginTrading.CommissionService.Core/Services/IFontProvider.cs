// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IFontProvider
    {
        string OpenSansRegular { get; }
        string OpenSansBold { get; }
        string RalewayRegular { get; }
        string RalewayBold { get; }
    }
}