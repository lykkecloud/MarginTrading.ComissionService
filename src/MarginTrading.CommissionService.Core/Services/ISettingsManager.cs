// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using MarginTrading.SettingsService.Contracts.Messages;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface ISettingsManager
    {
        Task HandleSettingsChanged(SettingsChangedEvent evt);
    }
}