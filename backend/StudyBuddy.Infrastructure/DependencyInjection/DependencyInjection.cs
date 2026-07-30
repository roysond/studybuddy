using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using StudyBuddy.Application.Eval;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Infrastructure.Evaluation;
using StudyBuddy.Infrastructure.ExternalServices;
using StudyBuddy.Infrastructure.Persistence;
using StudyBuddy.Infrastructure.Telemetry;

namespace StudyBuddy.Infrastructure.DependencyInjection;

/// <summary>
/// Registers Infrastructure-layer services (PostgreSQL persistence, external API options,
/// developer telemetry, and on-demand evaluation).
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

        RegisterDeveloperDashboard(services);

        return services;
    }

    private static void RegisterDeveloperDashboard(IServiceCollection services)
    {
        // Shared by telemetry and evaluation — ambient flag for tagging Kernel calls.
        services.AddSingleton<IEvalExecutionContext, EvalExecutionContext>();

        // Telemetry half — live capture, independent of evals and tutoring plugins.
        services.AddSingleton<ITelemetryStore, InMemoryTelemetryStore>();
        services.AddSingleton<IFunctionInvocationFilter, TelemetryFunctionInvocationFilter>();

        // Evaluation half — on-demand only, independent of telemetry.
        services.AddSingleton<IEvalResultStore, FileEvalResultStore>();
        services.AddSingleton<IEvalTestSetProvider, HardcodedEvalTestSetProvider>();
        services.AddScoped<IEvalReportWriter, DiskEvalReportWriter>();
        services.AddScoped<IEvalRunnerService, EvalRunnerService>();
    }
}
