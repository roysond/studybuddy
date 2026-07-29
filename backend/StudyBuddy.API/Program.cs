using System.Security.Cryptography.X509Certificates;
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
builder.Services.AddScoped<IQuizService, QuizService>();
builder.Services.AddScoped<ISummariseService, SummariseService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("StudyBuddyFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5180")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("StudyBuddyFrontend");
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

    // .NET validates certificate revocation status by default. On networks that block
    // OCSP/CRL endpoints this fails with RevocationStatusUnknown even though the
    // certificate is valid. In Development only, skip the revocation check.
    // Chain and hostname validation remain fully enforced.
    HttpClient? httpClient = null;

    if (builder.Environment.IsDevelopment())
    {
        var handler = new SocketsHttpHandler
        {
            SslOptions = new System.Net.Security.SslClientAuthenticationOptions
            {
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck
            }
        };

        httpClient = new HttpClient(handler);
    }

    // Custom OpenAI-compatible endpoints (OpenRouter) are experimental in SK.
#pragma warning disable SKEXP0010
    var kernelBuilder = builder.Services.AddKernel();
    kernelBuilder.AddOpenAIChatCompletion(
        modelId: modelId,
        apiKey: apiKey,
        endpoint: endpoint,
        httpClient: httpClient);
#pragma warning restore SKEXP0010

    kernelBuilder.Plugins.AddFromType<ExplainPlugin>();
    kernelBuilder.Plugins.AddFromType<QuizPlugin>();
    kernelBuilder.Plugins.AddFromType<SummarisePlugin>();
}
