using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyBuddy.Infrastructure.ExternalServices;
using StudyBuddy.Infrastructure.Persistence;

namespace StudyBuddy.Infrastructure.DependencyInjection;

/// <summary>
/// Registers Infrastructure-layer services (PostgreSQL, HttpClient stubs for external APIs).
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
        services.Configure<ElevenLabsOptions>(configuration.GetSection(ElevenLabsOptions.SectionName));

        // Named HttpClient reserved for the future ElevenLabs TTS client.
        services.AddHttpClient("ElevenLabs", (sp, client) =>
        {
            var options = configuration.GetSection(ElevenLabsOptions.SectionName).Get<ElevenLabsOptions>()
                ?? new ElevenLabsOptions();

            client.BaseAddress = new Uri(options.BaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                client.DefaultRequestHeaders.Add("xi-api-key", options.ApiKey);
            }
        });

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<StudyBuddyDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        return services;
    }
}
