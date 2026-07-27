using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StudyBuddy.Application.Interfaces;
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
        services.Configure<ElevenLabsOptions>(options =>
        {
            configuration.GetSection(ElevenLabsOptions.SectionName).Bind(options);

            options.ApiKey = Environment.GetEnvironmentVariable("ELEVENLABS_API_KEY")
                ?? options.ApiKey;

            options.VoiceId = Environment.GetEnvironmentVariable("ELEVENLABS_VOICE_ID")
                ?? options.VoiceId;
        });

        services.AddHttpClient("ElevenLabs", (sp, client) =>
        {
            var options = configuration.GetSection(ElevenLabsOptions.SectionName).Get<ElevenLabsOptions>()
                ?? new ElevenLabsOptions();

            var apiKey = Environment.GetEnvironmentVariable("ELEVENLABS_API_KEY")
                ?? options.ApiKey;

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Add("Accept", "audio/mpeg");

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                client.DefaultRequestHeaders.Add("xi-api-key", apiKey);
            }
        });

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            services.AddDbContext<StudyBuddyDbContext>(options =>
                options.UseNpgsql(connectionString));
        }

        services.AddScoped<ISpeechService, ElevenLabsSpeechService>();

        return services;
    }
}
