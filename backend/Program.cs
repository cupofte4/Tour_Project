using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
builder.Services.AddScoped<VinhKhanhGuide.Application.Analytics.IAnalyticsService, VinhKhanhGuide.Infrastructure.Analytics.AnalyticsService>();

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

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
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

app.UseCors("AllowAll");
app.UseStaticFiles();

app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
