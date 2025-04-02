using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Cards56Web
{
    public class SignalRHealthCheck : IHealthCheck
    {
        private readonly IHubContext<Cards56Hub> _hubContext;

        public SignalRHealthCheck(IHubContext<Cards56Hub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("HealthCheck", "Checking connection", cancellationToken);
                return HealthCheckResult.Healthy();
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(exception: ex);
            }
        }
    }
}
