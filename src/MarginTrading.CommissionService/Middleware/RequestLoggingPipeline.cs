using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Text;

namespace MarginTrading.CommissionService.Middleware
{
    public class RequestLoggingPipeline
    {
        public void Configure(IApplicationBuilder applicationBuilder)
        {
            applicationBuilder.UseMiddleware<RequestsLoggingMiddleware>();
        }
    }
}
