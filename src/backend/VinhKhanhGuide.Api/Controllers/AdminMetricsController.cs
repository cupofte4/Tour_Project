using Microsoft.AspNetCore.Mvc;
using VinhKhanhGuide.Application.Analytics;

namespace VinhKhanhGuide.Api.Controllers;

[ApiController]
[Route("api/admin/metrics")]
public sealed class AdminMetricsController(IAppUsageAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("active-users")]
    public Task<ActiveUsersMetricDto> GetActiveUsers(CancellationToken cancellationToken)
    {
        return analyticsService.GetActiveUsersAsync(cancellationToken);
    }
}
