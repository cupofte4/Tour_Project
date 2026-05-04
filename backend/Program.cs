using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Tour_Project.Data;
using Tour_Project.Models;
using Tour_Project.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<backend.Services.ILocationService, backend.Services.LocationService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<VinhKhanhGuide.Infrastructure.Analytics.AnalyticsService>();
builder.Services.AddScoped<VinhKhanhGuide.Application.Analytics.IAnalyticsService>(sp =>
    sp.GetRequiredService<VinhKhanhGuide.Infrastructure.Analytics.AnalyticsService>());
builder.Services.AddSingleton<VinhKhanhGuide.Application.Analytics.IAudioPlayEventQueue, VinhKhanhGuide.Infrastructure.Analytics.AudioPlayEventQueue>();
builder.Services.AddHostedService<VinhKhanhGuide.Infrastructure.Analytics.AudioPlayEventWorker>();

var jwtSecretKey =
    builder.Configuration["Jwt:SecretKey"]
    ?? Environment.GetEnvironmentVariable("JWT_SECRET_KEY")
    ?? "your-super-secret-key-change-in-production-at-least-32-characters";

if (jwtSecretKey.Length < 32)
{
    throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = "TourGuideApp",
            ValidateAudience = true,
            ValidAudience = "TourGuideUser",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    ));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// Seed database with an initial admin user (if missing)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    const int maxDatabaseAttempts = 10;
    var databaseReady = false;

    for (var attempt = 1; attempt <= maxDatabaseAttempts; attempt++)
    {
        try
        {
            db.Database.Migrate();
            databaseReady = true;
            break;
        }
        catch when (attempt < maxDatabaseAttempts)
        {
            Thread.Sleep(TimeSpan.FromSeconds(5));
        }
    }

    if (!databaseReady)
    {
        throw new InvalidOperationException("Database is not ready after multiple connection attempts.");
    }

    if (!db.AdminUsers.Any(a => a.Username == "admin"))
    {
        db.AdminUsers.Add(new AdminUser
        {
            Username = "admin",
            PasswordHash = "admin123", // TODO: replace with hashed password
            FullName = "Administrator",
            Phone = "0900000000",
            Role = "admin",
            IsLocked = false,
            CreatedAt = DateTime.UtcNow
        });

        db.SaveChanges();
    }
}

app.UseForwardedHeaders();
app.UseCors("AllowAll");
app.UseStaticFiles();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Tour Project API v1");
    options.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapOpenApi();
app.MapControllers();

app.Run();
