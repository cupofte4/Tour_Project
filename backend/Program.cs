using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Tour_Project.Data;
using Tour_Project.Models;
using Tour_Project.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddScoped<LocationService>();
builder.Services.AddScoped<JwtService>();

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

// Seed database with admin user
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    
    // Check if admin user exists
    var adminExists = db.Users.Any(u => u.Username == "admin");
    if (!adminExists)
    {
        db.Users.Add(new User
        {
            FullName = "Administrator",
            Username = "admin",
            Password = "admin123", // TODO: Hash password in production
            Phone = "0900000000",
            Gender = "Nam",
            Role = Roles.Admin,
            IsLocked = false
        });
        db.SaveChanges();
    }
}

app.UseCors("AllowAll");
app.UseStaticFiles();

app.MapOpenApi();

app.MapControllers();

app.Run();
