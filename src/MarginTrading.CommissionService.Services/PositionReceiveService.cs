// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.Backend.Contracts;
using MarginTrading.Backend.Contracts.Positions;
using MarginTrading.CommissionService.Core.Domain;
using MarginTrading.CommissionService.Core.Domain.Abstractions;
using MarginTrading.CommissionService.Core.Services;

namespace MarginTrading.CommissionService.Services
{
    public class PositionReceiveService : IPositionReceiveService
    {
        private readonly IConvertService _convertService;
        private readonly IPositionsApi _positionsApi;

        public PositionReceiveService(
            IConvertService convertService,
            IPositionsApi positionsApi)
        {
            _convertService = convertService;
            _positionsApi = positionsApi;
        }
        
        public async Task<List<IOpenPosition>> GetActive()
        {
            var positions = await _positionsApi.ListAsync();
            return positions.Select(x => (IOpenPosition) _convertService.Convert<OpenPositionContract, OpenPosition>(x))
                .ToList();
        }
        
        public async Task<List<IOpenPosition>> GetByAccount(string accountId)
        {
            var positions = await _positionsApi.ListAsync(accountId);
            return positions.Select(x => (IOpenPosition)_convertService.Convert<OpenPositionContract, OpenPosition>(x)).ToList();
        }
        
        public async Task<List<IOpenPosition>> GetByInstrument(string instrument)
        {
            var positions = await _positionsApi.ListAsync(assetPairId: instrument);
            return positions.Select(x => (IOpenPosition)_convertService.Convert<OpenPositionContract, OpenPosition>(x)).ToList();
        }
    }
}