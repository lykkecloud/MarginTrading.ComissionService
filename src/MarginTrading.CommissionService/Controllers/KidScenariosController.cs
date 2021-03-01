// Copyright (c) 2019 Lykke Corp.
// See the LICENSE file in the project root for more information.

using System.Linq;
using System.Net;
using System.Threading.Tasks;
using MarginTrading.CommissionService.Contracts;
using MarginTrading.CommissionService.Contracts.Api;
using MarginTrading.CommissionService.Contracts.Models.KidScenarios;
using MarginTrading.CommissionService.Core.Domain.KidScenarios;
using MarginTrading.CommissionService.Core.Services;
using MarginTrading.CommissionService.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarginTrading.CommissionService.Controllers
{
    [Authorize]
    [Route("api/kid-scenarios")]
    [MiddlewareFilter(typeof(RequestLoggingPipeline))]
    public class KidScenariosController : ControllerBase, IKidScenariosApi
    {
        private readonly IKidScenariosService _kidScenariosService;
        private readonly IConvertService _convertService;

        public KidScenariosController(IKidScenariosService kidScenariosService, IConvertService convertService)
        {
            _kidScenariosService = kidScenariosService;
            _convertService = convertService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(ErrorCodeResponse<KidScenariosErrorCodesContract>), (int) HttpStatusCode.OK)]
        public async Task<ErrorCodeResponse<KidScenariosErrorCodesContract>> AddAsync(
            [FromBody] AddKidScenarioRequest request)
        {
            var kidScenario = _convertService.Convert<AddKidScenarioRequest, KidScenario>(request);

            var result = await _kidScenariosService.InsertAsync(kidScenario);

            var response = new ErrorCodeResponse<KidScenariosErrorCodesContract>();

            if (result.IsFailed)
            {
                response.ErrorCode =
                    _convertService.Convert<KidScenariosErrorCodes, KidScenariosErrorCodesContract>(
                        result.Error.GetValueOrDefault());
            }

            return response;
        }

        [HttpPut("{isin}")]
        [ProducesResponseType(typeof(ErrorCodeResponse<KidScenariosErrorCodesContract>), (int) HttpStatusCode.OK)]
        public async Task<ErrorCodeResponse<KidScenariosErrorCodesContract>> UpdateAsync(string isin,
            [FromBody] UpdateKidScenarioRequest request)
        {
            var kidScenario = _convertService.Convert<UpdateKidScenarioRequest, KidScenario>(request);
            kidScenario.Isin = isin;

            var result = await _kidScenariosService.UpdateAsync(kidScenario);

            var response = new ErrorCodeResponse<KidScenariosErrorCodesContract>();

            if (result.IsFailed)
            {
                response.ErrorCode =
                    _convertService.Convert<KidScenariosErrorCodes, KidScenariosErrorCodesContract>(
                        result.Error.GetValueOrDefault());
            }

            return response;
        }

        [HttpDelete("{isin}")]
        [ProducesResponseType(typeof(ErrorCodeResponse<KidScenariosErrorCodesContract>), (int) HttpStatusCode.OK)]
        public async Task<ErrorCodeResponse<KidScenariosErrorCodesContract>> DeleteAsync(string isin)
        {
            var result = await _kidScenariosService.DeleteAsync(isin);

            var response = new ErrorCodeResponse<KidScenariosErrorCodesContract>();

            if (result.IsFailed)
            {
                response.ErrorCode =
                    _convertService.Convert<KidScenariosErrorCodes, KidScenariosErrorCodesContract>(
                        result.Error.GetValueOrDefault());
            }

            return response;
        }

        [HttpGet("{isin}")]
        [ProducesResponseType(typeof(GetKidScenarioByIdResponse), (int) HttpStatusCode.OK)]
        public async Task<GetKidScenarioByIdResponse> GetByIdAsync(string isin)
        {
            var result = await _kidScenariosService.GetByIdAsync(isin);

            var response = new GetKidScenarioByIdResponse();

            if (result.IsSuccess)
            {
                response.KidScenario = _convertService.Convert<KidScenario, KidScenarioContract>(result.Value);
            }
            else
            {
                response.ErrorCode = _convertService.Convert<KidScenariosErrorCodes, KidScenariosErrorCodesContract>(
                    result.Error.GetValueOrDefault());
            }

            return response;
        }

        [HttpPost("list")]
        [ProducesResponseType(typeof(GetKidScenariosResponse), (int) HttpStatusCode.OK)]
        public async Task<GetKidScenariosResponse> GetAllAsync([FromBody] GetKidScenariosRequest request)
        {
            var result = await _kidScenariosService.GetAllAsync(request.Isins.ToArray(), request.Skip, request.Take);

            var response = new GetKidScenariosResponse()
            {
                KidScenarios = result.Value
                    .Select(ks => _convertService.Convert<KidScenario, KidScenarioContract>(ks))
                    .ToList()
            };

            return response;
        }
    }
}