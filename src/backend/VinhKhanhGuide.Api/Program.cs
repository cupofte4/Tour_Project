using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VinhKhanhGuide.Application;
using VinhKhanhGuide.Application.RemoteLocations;
using VinhKhanhGuide.Infrastructure;
using VinhKhanhGuide.Infrastructure.Persistence;
using VinhKhanhGuide.Infrastructure.RemoteLocations;
using Npgsql;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
var dashboardCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .GetChildren()
    .Select(section => section.Value?.Trim())
    .Where(value => !string.IsNullOrWhiteSpace(value))
    .Cast<string>()
    .ToArray();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
if (dashboardCorsOrigins.Length > 0)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("DashboardCors", policy =>
        {
            policy.WithOrigins(dashboardCorsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
    });
}
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var connectionString = app.Configuration.GetConnectionString("DefaultConnection");

if (!string.IsNullOrWhiteSpace(connectionString))
{
    var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString);
    logger.LogInformation(
        "Starting backend in {Environment} using PostgreSQL database {Database} on {Host}:{Port}.",
        app.Environment.EnvironmentName,
        connectionStringBuilder.Database,
        connectionStringBuilder.Host,
        connectionStringBuilder.Port);
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var importOptions = scope.ServiceProvider.GetRequiredService<IOptions<RemoteLocationImportOptions>>().Value;

    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }

    var shouldRunStartupSync = importOptions.Enabled && importOptions.RunOnStartup;

    if (!shouldRunStartupSync)
    {
        var skipReason = !importOptions.Enabled
            ? "RemoteLocationImport:Enabled is false"
            : "RemoteLocationImport:RunOnStartup is false";

        logger.LogInformation(
            "Startup location sync skipped because {Reason}. Manual sync endpoint remains available.",
            skipReason);
    }
    else
    {
        logger.LogInformation(
            "Starting location sync from snapshot {SqlSnapshotPath}. The sync is idempotent and only inserts, updates, skips unchanged records, or deactivates missing records.",
            importOptions.SqlSnapshotPath);

        var syncService = scope.ServiceProvider.GetRequiredService<IRemoteLocationSyncService>();
        var syncResult = await syncService.SyncAsync();

        logger.LogInformation(
            "Startup location sync completed. Inserted: {InsertedStalls} stalls, Updated: {UpdatedStalls}, Skipped: {SkippedStalls}, Deactivated: {DeactivatedStalls}, Failed: {FailedRecords}.",
            syncResult.InsertedStallCount,
            syncResult.UpdatedStallCount,
            syncResult.SkippedStallCount,
            syncResult.DeactivatedStallCount,
            syncResult.FailedRecordCount);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (dashboardCorsOrigins.Length > 0)
{
    app.UseCors("DashboardCors");
}

const string DemoCustomUrlScheme = "vinhkhanhguide";

app.MapGet("/", () => Results.Ok(new
{
    Name = "Vinh Khanh Guide API",
    Status = "Ready"
}));
app.MapGet("/demo/open", () => Results.Content(BuildDemoLandingHtml(null), "text/html; charset=utf-8"));
app.MapGet("/demo/open/stall/{stallId}", (string stallId) =>
{
    return Results.Content(
        int.TryParse(stallId, out var parsedStallId) && parsedStallId > 0
            ? BuildDemoLandingHtml(parsedStallId)
            : BuildDemoLandingHtml(null, "This demo link does not contain a valid stall id."),
        "text/html; charset=utf-8");
});
app.MapControllers();
app.Run();

static string BuildDemoLandingHtml(int? stallId, string? notice = null)
{
    var title = "Vinh Khanh Guide Demo";
    var description = "Open the iPhone app directly from this page for a quick tour demo.";
    var selectedStallText = stallId.HasValue
        ? $"Selected stall: {stallId.Value}"
        : "No specific stall selected.";
    var appLink = stallId.HasValue
        ? $"{DemoCustomUrlScheme}://stall/{stallId.Value}"
        : $"{DemoCustomUrlScheme}://open";
    var encodedAppLink = WebUtility.HtmlEncode(appLink);
    var encodedSelectedStallText = WebUtility.HtmlEncode(selectedStallText);
    var encodedNotice = string.IsNullOrWhiteSpace(notice)
        ? string.Empty
        : $"<p class=\"notice\">{WebUtility.HtmlEncode(notice)}</p>";

    return $$"""
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{{WebUtility.HtmlEncode(title)}}</title>
  <style>
    :root { color-scheme: light; }
    body {
      margin: 0;
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
      background: linear-gradient(180deg, #f7f2e8 0%, #fffdf9 100%);
      color: #1f2933;
    }
    main {
      max-width: 560px;
      margin: 0 auto;
      padding: 40px 20px 56px;
    }
    .card {
      background: rgba(255,255,255,0.94);
      border: 1px solid #e7dcc9;
      border-radius: 20px;
      padding: 24px;
      box-shadow: 0 20px 40px rgba(125, 91, 51, 0.08);
    }
    h1 { margin-top: 0; font-size: 2rem; }
    p { line-height: 1.5; }
    .stall { color: #7c5a35; font-weight: 600; }
    .notice {
      margin-top: 12px;
      padding: 10px 12px;
      border-radius: 12px;
      background: #fff4e5;
      color: #8a5a16;
    }
    .cta {
      display: inline-block;
      margin-top: 16px;
      padding: 14px 18px;
      border-radius: 999px;
      background: #1f7a5c;
      color: #fff;
      text-decoration: none;
      font-weight: 700;
    }
    .hint {
      margin-top: 18px;
      color: #52606d;
      font-size: 0.95rem;
    }
    code {
      display: inline-block;
      margin-top: 6px;
      padding: 4px 6px;
      border-radius: 8px;
      background: #f3ede2;
    }
  </style>
</head>
<body>
  <main>
    <section class="card">
      <h1>{{WebUtility.HtmlEncode(title)}}</h1>
      <p>{{WebUtility.HtmlEncode(description)}}</p>
      <p class="stall">{{encodedSelectedStallText}}</p>
      {{encodedNotice}}
      <a class="cta" href="{{encodedAppLink}}">Open App</a>
      <p class="hint">If the app does not open, make sure it is installed on this iPhone.</p>
      <p class="hint">QR target example: <code>/demo/open/stall/1</code></p>
    </section>
  </main>
</body>
</html>
""";
}

public partial class Program
{
}
