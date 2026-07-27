using Microsoft.SemanticKernel;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Application.Plugins;
using StudyBuddy.Application.Services;
using StudyBuddy.Infrastructure.DependencyInjection;
using StudyBuddy.Infrastructure.ExternalServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

ConfigureSemanticKernelTelemetry(builder);
ConfigureSemanticKernel(builder);

builder.Services.AddScoped<IExplainService, ExplainService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

static void ConfigureSemanticKernelTelemetry(WebApplicationBuilder builder)
{
    // Enable SK GenAI diagnostics so Claude prompts/completions appear in telemetry during development.
    AppContext.SetSwitch("Microsoft.SemanticKernel.Experimental.GenAI.EnableOTelDiagnosticsSensitive", true);

    var resourceBuilder = ResourceBuilder
        .CreateDefault()
        .AddService(
            serviceName: "StudyBuddy.API",
            serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "1.0.0");

    builder.Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("StudyBuddy.API"))
        .WithTracing(tracing => tracing
            .SetResourceBuilder(resourceBuilder)
            .AddSource("Microsoft.SemanticKernel*")
            .AddConsoleExporter())
        .WithMetrics(metrics => metrics
            .SetResourceBuilder(resourceBuilder)
            .AddMeter("Microsoft.SemanticKernel*")
            .AddConsoleExporter());
}

static void ConfigureSemanticKernel(WebApplicationBuilder builder)
{
    var openRouter = builder.Configuration.GetSection(OpenRouterOptions.SectionName).Get<OpenRouterOptions>()
        ?? new OpenRouterOptions();

    var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
        ?? openRouter.ApiKey
        ?? string.Empty;

    if (string.IsNullOrWhiteSpace(apiKey))
    {
        throw new InvalidOperationException(
            "OPENROUTER_API_KEY is not set. Provide it via environment variable or appsettings.Development.json.");
    }

    var modelId = openRouter.Model;
    var endpoint = new Uri(openRouter.BaseUrl);

    // Custom OpenAI-compatible endpoints (OpenRouter) are experimental in SK.
#pragma warning disable SKEXP0010
    var kernelBuilder = builder.Services.AddKernel();
    kernelBuilder.AddOpenAIChatCompletion(
        modelId: modelId,
        apiKey: apiKey,
        endpoint: endpoint);
#pragma warning restore SKEXP0010

    kernelBuilder.Plugins.AddFromType<ExplainPlugin>();
}
