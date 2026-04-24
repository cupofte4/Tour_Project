using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tour_Project.Models;
using VinhKhanhGuide.Application.Analytics;

namespace VinhKhanhGuide.Api.Controllers;

[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = Roles.Admin)]
public sealed class AdminAnalyticsController(IAnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await analyticsService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    [HttpGet("chart-data")]
    public async Task<IActionResult> GetChartData([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, CancellationToken cancellationToken)
    {
        if (endDate < startDate) return BadRequest(new { message = "endDate must be >= startDate" });

        var data = await analyticsService.GetChartDataAsync(startDate, endDate, cancellationToken);
        return Ok(data);
    }
}
