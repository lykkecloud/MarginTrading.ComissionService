// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Core.Domain.Abstractions;

namespace MarginTrading.CommissionService.Core.Services
{
    public interface IPositionReceiveService
    {
        Task<List<IOpenPosition>> GetByAccount(string accountId);
        Task<List<IOpenPosition>> GetByInstrument(string instrument);
    }
}