using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VinhKhanhGuide.Application.Stalls;
using VinhKhanhGuide.Infrastructure.Persistence;
using VinhKhanhGuide.Infrastructure.Stalls;

namespace VinhKhanhGuide.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IStallReadRepository, StallReadRepository>();

        return services;
    }
}
