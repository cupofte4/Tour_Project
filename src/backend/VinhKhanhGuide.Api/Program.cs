using Microsoft.EntityFrameworkCore;
using VinhKhanhGuide.Application;
using VinhKhanhGuide.Application.RemoteLocations;
using VinhKhanhGuide.Infrastructure;
using VinhKhanhGuide.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var syncService = scope.ServiceProvider.GetRequiredService<IRemoteLocationSyncService>();

    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }

    await syncService.SyncAsync();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapGet("/", () => Results.Ok(new
{
    Name = "Vinh Khanh Guide API",
    Status = "Ready"
}));
app.MapControllers();
app.Run();

public partial class Program
{
}
