using Microsoft.AspNetCore.Builder;

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
