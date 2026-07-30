using System.ClientModel;
using System.ClientModel.Primitives;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Evaluation;
using Microsoft.Extensions.AI.Evaluation.Quality;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenAI;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Application.Models;
using StudyBuddy.Domain.Models;
using StudyBuddy.Infrastructure.ExternalServices;

namespace StudyBuddy.Infrastructure.Evaluation;

/// <summary>
/// Runs tutoring-mode outputs through Quality evaluators and averages scores per mode.
/// Calls the existing Explain/Quiz/Summarise services — does not duplicate their logic.
/// </summary>
public sealed class EvalRunnerService : IEvalRunnerService
{
    public const string ModeExplain = "Explain";
    public const string ModeQuiz = "Quiz";
    public const string ModeSummarise = "Summarise";

    private static readonly string[] DisplayMetricOrder =
    [
        "Groundedness",
        "Relevance",
        "Completeness",
        "Fluency",
        "Coherence",
        "Truthfulness"
    ];

    private readonly IExplainService _explainService;
    private readonly IQuizService _quizService;
    private readonly ISummariseService _summariseService;
    private readonly IEvalTestSetProvider _testSetProvider;
    private readonly IEvalExecutionContext _evalExecutionContext;
    private readonly ChatConfiguration _chatConfiguration;
    private readonly IEvaluator _evaluator;

    public EvalRunnerService(
        IExplainService explainService,
        IQuizService quizService,
        ISummariseService summariseService,
        IEvalTestSetProvider testSetProvider,
        IEvalExecutionContext evalExecutionContext,
        IOptions<OpenRouterOptions> openRouterOptions,
        IHostEnvironment hostEnvironment)
    {
        _explainService = explainService ?? throw new ArgumentNullException(nameof(explainService));
        _quizService = quizService ?? throw new ArgumentNullException(nameof(quizService));
        _summariseService = summariseService ?? throw new ArgumentNullException(nameof(summariseService));
        _testSetProvider = testSetProvider ?? throw new ArgumentNullException(nameof(testSetProvider));
        _evalExecutionContext = evalExecutionContext
            ?? throw new ArgumentNullException(nameof(evalExecutionContext));
        ArgumentNullException.ThrowIfNull(openRouterOptions);
        ArgumentNullException.ThrowIfNull(hostEnvironment);

        _chatConfiguration = CreateChatConfiguration(openRouterOptions.Value, hostEnvironment.IsDevelopment());

        // Relevance / Completeness / Truth (Truthfulness) come from the experimental
        // RelevanceTruthAndCompletenessEvaluator (AIEVAL001). Groundedness / Fluency /
        // Coherence use the stable Quality evaluators.
#pragma warning disable AIEVAL001
        _evaluator = new CompositeEvaluator(
            new GroundednessEvaluator(),
            new FluencyEvaluator(),
            new CoherenceEvaluator(),
            new RelevanceTruthAndCompletenessEvaluator());
#pragma warning restore AIEVAL001
    }

    public async Task<EvalRunResult> RunAsync(CancellationToken cancellationToken = default)
    {
        using var _ = _evalExecutionContext.BeginEvalRun();

        var modeScores = new Dictionary<string, ModeEvalScores>(StringComparer.Ordinal);

        modeScores[ModeExplain] = await EvaluateModeAsync(
            _testSetProvider.GetExplainCases(),
            async (testCase, ct) =>
            {
                var result = await _explainService.ExplainAsync(
                    testCase.StudyMaterial,
                    testCase.UserMessageOrTopic,
                    ct).ConfigureAwait(false);
                return (BuildUserPrompt("Explain this study material.", testCase), result.Explanation);
            },
            cancellationToken).ConfigureAwait(false);

        modeScores[ModeQuiz] = await EvaluateModeAsync(
            _testSetProvider.GetQuizCases(),
            async (testCase, ct) =>
            {
                var result = await _quizService.GenerateQuestionsAsync(
                    testCase.StudyMaterial,
                    testCase.UserMessageOrTopic,
                    ct).ConfigureAwait(false);
                return (BuildUserPrompt("Generate quiz questions on this topic.", testCase), result.Questions);
            },
            cancellationToken).ConfigureAwait(false);

        modeScores[ModeSummarise] = await EvaluateModeAsync(
            _testSetProvider.GetSummariseCases(),
            async (testCase, ct) =>
            {
                var result = await _summariseService.SummariseAsync(testCase.StudyMaterial, ct).ConfigureAwait(false);
                return (BuildUserPrompt("Summarise this study material into key points.", testCase), result.Summary);
            },
            cancellationToken).ConfigureAwait(false);

        return new EvalRunResult(DateTimeOffset.UtcNow, modeScores);
    }

    private async Task<ModeEvalScores> EvaluateModeAsync(
        IReadOnlyList<EvalTestCase> cases,
        Func<EvalTestCase, CancellationToken, Task<(string UserPrompt, string ModelOutput)>> produceOutput,
        CancellationToken cancellationToken)
    {
        var totals = DisplayMetricOrder.ToDictionary(name => name, _ => 0.0, StringComparer.Ordinal);
        var counts = DisplayMetricOrder.ToDictionary(name => name, _ => 0, StringComparer.Ordinal);

        foreach (var testCase in cases)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (userPrompt, modelOutput) = await produceOutput(testCase, cancellationToken).ConfigureAwait(false);

            var messages = new List<ChatMessage>
            {
                new(ChatRole.User, userPrompt)
            };
            var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, modelOutput));

            var evaluationResult = await _evaluator.EvaluateAsync(
                messages,
                response,
                _chatConfiguration,
                additionalContext: [new GroundednessEvaluatorContext(testCase.StudyMaterial)],
                cancellationToken).ConfigureAwait(false);

            AccumulateMetrics(evaluationResult, totals, counts);
        }

        var averages = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var metricName in DisplayMetricOrder)
        {
            averages[metricName] = counts[metricName] == 0
                ? 0
                : Math.Round(totals[metricName] / counts[metricName], 2);
        }

        return new ModeEvalScores(averages);
    }

    private static void AccumulateMetrics(
        EvaluationResult evaluationResult,
        IDictionary<string, double> totals,
        IDictionary<string, int> counts)
    {
        TryAccumulate(evaluationResult, GroundednessEvaluator.GroundednessMetricName, "Groundedness", totals, counts);
#pragma warning disable AIEVAL001
        TryAccumulate(evaluationResult, RelevanceTruthAndCompletenessEvaluator.RelevanceMetricName, "Relevance", totals, counts);
        TryAccumulate(evaluationResult, RelevanceTruthAndCompletenessEvaluator.CompletenessMetricName, "Completeness", totals, counts);
        TryAccumulate(evaluationResult, RelevanceTruthAndCompletenessEvaluator.TruthMetricName, "Truthfulness", totals, counts);
#pragma warning restore AIEVAL001
        TryAccumulate(evaluationResult, FluencyEvaluator.FluencyMetricName, "Fluency", totals, counts);
        TryAccumulate(evaluationResult, CoherenceEvaluator.CoherenceMetricName, "Coherence", totals, counts);
    }

    private static void TryAccumulate(
        EvaluationResult evaluationResult,
        string libraryMetricName,
        string displayName,
        IDictionary<string, double> totals,
        IDictionary<string, int> counts)
    {
        if (!evaluationResult.TryGet<NumericMetric>(libraryMetricName, out var metric)
            || metric.Value is null)
        {
            return;
        }

        totals[displayName] += metric.Value.Value;
        counts[displayName] += 1;
    }

    private static string BuildUserPrompt(string intent, EvalTestCase testCase)
    {
        if (string.IsNullOrWhiteSpace(testCase.UserMessageOrTopic))
        {
            return $"{intent}\n\nStudy material:\n{testCase.StudyMaterial}";
        }

        return $"{intent}\n\nFocus: {testCase.UserMessageOrTopic}\n\nStudy material:\n{testCase.StudyMaterial}";
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
