using StudyBuddy.Application.Interfaces;
using StudyBuddy.Application.Models;
using StudyBuddy.Domain.Models;

namespace StudyBuddy.Infrastructure.Evaluation;

/// <summary>
/// Runs tutoring-mode outputs through Quality evaluators and averages scores per mode.
/// Calls the existing Explain/Quiz/Summarise services — does not duplicate their logic.
/// Evaluation + disk reporting are delegated to <see cref="IEvalReportWriter"/>.
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
    private readonly IEvalReportWriter _evalReportWriter;

    public EvalRunnerService(
        IExplainService explainService,
        IQuizService quizService,
        ISummariseService summariseService,
        IEvalTestSetProvider testSetProvider,
        IEvalExecutionContext evalExecutionContext,
        IEvalReportWriter evalReportWriter)
    {
        _explainService = explainService ?? throw new ArgumentNullException(nameof(explainService));
        _quizService = quizService ?? throw new ArgumentNullException(nameof(quizService));
        _summariseService = summariseService ?? throw new ArgumentNullException(nameof(summariseService));
        _testSetProvider = testSetProvider ?? throw new ArgumentNullException(nameof(testSetProvider));
        _evalExecutionContext = evalExecutionContext
            ?? throw new ArgumentNullException(nameof(evalExecutionContext));
        _evalReportWriter = evalReportWriter ?? throw new ArgumentNullException(nameof(evalReportWriter));
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
        var caseResults = new List<EvalCaseResult>(cases.Count);

        foreach (var testCase in cases)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (userPrompt, modelOutput) = await produceOutput(testCase, cancellationToken).ConfigureAwait(false);

            var metrics = await _evalReportWriter.EvaluateAndPersistAsync(
                testCase.Name,
                userPrompt,
                modelOutput,
                testCase.StudyMaterial,
                cancellationToken).ConfigureAwait(false);

            var metricsCopy = new Dictionary<string, EvalMetricResult>(metrics, StringComparer.Ordinal);
            caseResults.Add(new EvalCaseResult(testCase.Name, metricsCopy));

            foreach (var (metricName, metric) in metricsCopy)
            {
                if (!totals.ContainsKey(metricName))
                {
                    continue;
                }

                totals[metricName] += metric.Value;
                counts[metricName] += 1;
            }
        }

        var averages = new Dictionary<string, double>(StringComparer.Ordinal);
        foreach (var metricName in DisplayMetricOrder)
        {
            averages[metricName] = counts[metricName] == 0
                ? 0
                : Math.Round(totals[metricName] / counts[metricName], 2);
        }

        return new ModeEvalScores(averages, caseResults);
    }

    private static string BuildUserPrompt(string intent, EvalTestCase testCase)
    {
        if (string.IsNullOrWhiteSpace(testCase.UserMessageOrTopic))
        {
            return $"{intent}\n\nStudy material:\n{testCase.StudyMaterial}";
        }

        return $"{intent}\n\nFocus: {testCase.UserMessageOrTopic}\n\nStudy material:\n{testCase.StudyMaterial}";
    }
}
