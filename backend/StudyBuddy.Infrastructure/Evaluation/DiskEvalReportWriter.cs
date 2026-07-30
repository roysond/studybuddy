using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.AI.Evaluation.Reporting;
using Microsoft.Extensions.AI.Evaluation.Reporting.Storage;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenAI;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Domain.Models;
using StudyBuddy.Infrastructure.ExternalServices;

namespace StudyBuddy.Infrastructure.Evaluation;

/// <summary>
/// Disk-backed eval reporting via <see cref="DiskBasedReportingConfiguration"/>.
/// Each scenario is evaluated through a <see cref="ScenarioRun"/> so results land under
/// <c>eval-reports/</c> in a format <c>dotnet aieval report</c> can render.
/// </summary>
/// <remarks>
/// View reports:
///   dotnet tool install --global Microsoft.Extensions.AI.Evaluation.Console
///   dotnet aieval report --path eval-reports --output report.html --open
/// </remarks>
public sealed class DiskEvalReportWriter : IEvalReportWriter
{
    public const string ReportsFolderName = "eval-reports";

    private static readonly string[] DisplayMetricOrder =
    [
        "Groundedness",
        "Relevance",
        "Completeness",
        "Fluency",
        "Coherence",
        "Truthfulness"
    ];

    private readonly ChatConfiguration _chatConfiguration;
    private readonly string _storageRootPath;
    private readonly object _gate = new();
    private ReportingConfiguration? _reportingConfiguration;

    public DiskEvalReportWriter(
        IOptions<OpenRouterOptions> openRouterOptions,
        IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(openRouterOptions);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        _chatConfiguration = CreateChatConfiguration(openRouterOptions.Value, hostEnvironment.IsDevelopment());
        _storageRootPath = ResolveRepoRelativePath(hostEnvironment, ReportsFolderName);
    }

    public async Task<IReadOnlyDictionary<string, EvalMetricResult>> EvaluateAndPersistAsync(
        string scenarioName,
        string userPrompt,
        string modelOutput,
        string studyMaterialForGroundedness,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scenarioName);
        ArgumentNullException.ThrowIfNull(userPrompt);
        ArgumentNullException.ThrowIfNull(modelOutput);
        ArgumentNullException.ThrowIfNull(studyMaterialForGroundedness);

        var reporting = GetOrCreateReportingConfiguration();

        await using var scenarioRun = await reporting
            .CreateScenarioRunAsync(scenarioName, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.User, userPrompt)
        };
        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, modelOutput));

        var evaluationResult = await scenarioRun.EvaluateAsync(
            messages,
            response,
            additionalContext: [new GroundednessEvaluatorContext(studyMaterialForGroundedness)],
            cancellationToken).ConfigureAwait(false);

        return ExtractDisplayMetrics(evaluationResult);
    }

    private ReportingConfiguration GetOrCreateReportingConfiguration()
    {
        lock (_gate)
        {
            if (_reportingConfiguration is not null)
            {
                return _reportingConfiguration;
            }

            // One execution name per DI scope (one dashboard "Run evals" request).
            var executionName = DateTimeOffset.UtcNow.ToString("yyyyMMdd'T'HHmmss'Z'");

            Directory.CreateDirectory(_storageRootPath);

            _reportingConfiguration = DiskBasedReportingConfiguration.Create(
                storageRootPath: _storageRootPath,
                evaluators: CreateEvaluators(),
                chatConfiguration: _chatConfiguration,
                enableResponseCaching: true,
                executionName: executionName);

            return _reportingConfiguration;
        }
    }

    private static IEnumerable<IEvaluator> CreateEvaluators()
    {
        // Relevance / Completeness / Truth (Truthfulness) come from the experimental
        // RelevanceTruthAndCompletenessEvaluator (AIEVAL001). Groundedness / Fluency /
        // Coherence use the stable Quality evaluators.
#pragma warning disable AIEVAL001
        yield return new GroundednessEvaluator();
        yield return new FluencyEvaluator();
        yield return new CoherenceEvaluator();
        yield return new RelevanceTruthAndCompletenessEvaluator();
#pragma warning restore AIEVAL001
    }

    private static IReadOnlyDictionary<string, EvalMetricResult> ExtractDisplayMetrics(
        EvaluationResult evaluationResult)
    {
        var metrics = new Dictionary<string, EvalMetricResult>(StringComparer.Ordinal);

        TryCapture(evaluationResult, GroundednessEvaluator.GroundednessMetricName, "Groundedness", metrics);
#pragma warning disable AIEVAL001
        TryCapture(evaluationResult, RelevanceTruthAndCompletenessEvaluator.RelevanceMetricName, "Relevance", metrics);
        TryCapture(evaluationResult, RelevanceTruthAndCompletenessEvaluator.CompletenessMetricName, "Completeness", metrics);
        TryCapture(evaluationResult, RelevanceTruthAndCompletenessEvaluator.TruthMetricName, "Truthfulness", metrics);
#pragma warning restore AIEVAL001
        TryCapture(evaluationResult, FluencyEvaluator.FluencyMetricName, "Fluency", metrics);
        TryCapture(evaluationResult, CoherenceEvaluator.CoherenceMetricName, "Coherence", metrics);

        // Preserve display order for consumers that iterate keys.
        var ordered = new Dictionary<string, EvalMetricResult>(StringComparer.Ordinal);
        foreach (var name in DisplayMetricOrder)
        {
            if (metrics.TryGetValue(name, out var metric))
            {
                ordered[name] = metric;
            }
        }

        return ordered;
    }

    private static void TryCapture(
        EvaluationResult evaluationResult,
        string libraryMetricName,
        string displayName,
        IDictionary<string, EvalMetricResult> metrics)
    {
        if (!evaluationResult.TryGet<NumericMetric>(libraryMetricName, out var metric)
            || metric.Value is null)
        {
            return;
        }

        metrics[displayName] = new EvalMetricResult(
            Value: metric.Value.Value,
            Reasoning: string.IsNullOrWhiteSpace(metric.Reason) ? null : metric.Reason);
    }

    internal static string ResolveRepoRelativePath(IHostEnvironment hostEnvironment, string folderName)
    {
        // API ContentRoot is typically backend/StudyBuddy.API — two levels up is the repo root.
        var repoRoot = Path.GetFullPath(Path.Combine(hostEnvironment.ContentRootPath, "..", ".."));
        return Path.Combine(repoRoot, folderName);
    }

    private static ChatConfiguration CreateChatConfiguration(OpenRouterOptions options, bool isDevelopment)
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
            ?? options.ApiKey
            ?? string.Empty;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new InvalidOperationException(
                "OPENROUTER_API_KEY is not set. Provide it via environment variable or appsettings.Development.json.");
        }

        var clientOptions = new OpenAIClientOptions
        {
            Endpoint = new Uri(options.BaseUrl)
        };

        if (isDevelopment)
        {
            var handler = new SocketsHttpHandler
            {
                SslOptions = new System.Net.Security.SslClientAuthenticationOptions
                {
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                }
            };

            clientOptions.Transport = new HttpClientPipelineTransport(new HttpClient(handler));
        }

        var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey), clientOptions);
        IChatClient chatClient = openAiClient.GetChatClient(options.Model).AsIChatClient();
        return new ChatConfiguration(chatClient);
    }
}
