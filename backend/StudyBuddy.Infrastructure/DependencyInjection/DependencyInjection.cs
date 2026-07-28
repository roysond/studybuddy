using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyBuddy.Infrastructure.ExternalServices;
using StudyBuddy.Infrastructure.Persistence;

namespace StudyBuddy.Infrastructure.DependencyInjection;

/// <summary>
/// Registers Infrastructure-layer services (PostgreSQL persistence, external API options).
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<OpenRouterOptions>(configuration.GetSection(OpenRouterOptions.SectionName));

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<StudyBuddyDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        return services;
    }
}
