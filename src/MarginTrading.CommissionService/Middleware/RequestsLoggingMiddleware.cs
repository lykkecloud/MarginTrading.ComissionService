﻿using Common;
using Common.Log;
using JetBrains.Annotations;
using MarginTrading.CommissionService.Core.Helpers;
using MarginTrading.CommissionService.Core.Settings;
using MarginTrading.CommissionService.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MarginTrading.CommissionService.Middleware
{
    [UsedImplicitly]
    public class RequestsLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly RequestLoggerSettings _settings;
        private readonly ILog _log;
        private readonly ILog _requestsLog;

        private const int MaxStorageFieldLength = 2000;
        private readonly string[] _personalDataHeaders = { "Authorization", "api-key" };

        public RequestsLoggingMiddleware(RequestDelegate next, RequestLoggerSettings settings, ILog log)
        {
            _next = next;
            _settings = settings;
            _log = log;
            _requestsLog = LogLocator.RequestsLog;
        }

        [UsedImplicitly]
        public async Task Invoke(HttpContext context)
        {
            var requestContext =
                $"Request path: {context?.Request?.Path}{context?.Request?.QueryString}, {Environment.NewLine}Method: {context?.Request?.Method}";
            try
            {
                if (_settings.Enabled && (_settings.EnabledForGet || context.Request.Method.ToUpper() != "GET"))
                {
                    var reqBodyStream = new MemoryStream();
                    var originalRequestBody = new MemoryStream();

                    await context.Request.Body.CopyToAsync(reqBodyStream);
                    reqBodyStream.Seek(0, SeekOrigin.Begin);
                    await reqBodyStream.CopyToAsync(originalRequestBody);
                    reqBodyStream.Seek(0, SeekOrigin.Begin);
                    context.Request.Body = reqBodyStream;

                    using (originalRequestBody)
                    {
                        var body = await StreamHelpers.GetStreamPart(originalRequestBody, _settings.MaxPartSize);
                        var headers = context.Request.Headers.Where(h => !_personalDataHeaders.Contains(h.Key)).ToJson();
                        var info = $"Body:{body} {Environment.NewLine}Headers:{headers}";
                        if (info.Length > MaxStorageFieldLength)
                        {
                            info = info.Substring(0, MaxStorageFieldLength);
                        }

                        await _requestsLog.WriteInfoAsync("MIDDLEWARE", "RequestsLoggingMiddleware", requestContext, info);
                    }
                }
            }
            catch (Exception ex)
            {
                await _log.WriteErrorAsync("MIDDLEWARE", "RequestsLoggingMiddleware", requestContext, ex);
            }
            finally
            {
                await _next.Invoke(context);
            }
        }
    }
}
