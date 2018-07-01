using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarginTrading.Backend.Contracts;
using MarginTrading.Backend.Contracts.Positions;
using MarginTrading.CommissionService.Core.Domain;
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
        
        public async Task<IEnumerable<OpenPosition>> GetActive()
        {
            var positions = await _positionsApi.ListAsync();
            return positions.Select(x => _convertService.Convert<OpenPositionContract, OpenPosition>(x));
        }
    }
}