using System.Diagnostics;
using Microsoft.SemanticKernel;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Application.Plugins;
using StudyBuddy.Domain.Models;

namespace StudyBuddy.Infrastructure.Telemetry;

/// <summary>
/// Semantic Kernel filter that records latency and token usage for tutoring plugin
/// invocations without modifying Explain, Quiz, or Summarise plugin code.
/// Nested prompt calls contribute Usage metadata to the outermost tracked mode.
/// </summary>
public sealed class TelemetryFunctionInvocationFilter : IFunctionInvocationFilter
{
    private static readonly AsyncLocal<Stack<UsageAccumulator>?> ActiveTrackers = new();

    private readonly ITelemetryStore _telemetryStore;
    private readonly IEvalExecutionContext _evalExecutionContext;

    public TelemetryFunctionInvocationFilter(
        ITelemetryStore telemetryStore,
        IEvalExecutionContext evalExecutionContext)
    {
        _telemetryStore = telemetryStore ?? throw new ArgumentNullException(nameof(telemetryStore));
        _evalExecutionContext = evalExecutionContext
            ?? throw new ArgumentNullException(nameof(evalExecutionContext));
    }

    public async Task OnFunctionInvocationAsync(
        FunctionInvocationContext context,
        Func<FunctionInvocationContext, Task> next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var mode = MapMode(context.Function);
        var isTracked = mode is not null;
        UsageAccumulator? tracker = null;
        var stopwatch = Stopwatch.StartNew();

        if (isTracked)
        {
            tracker = new UsageAccumulator();
            var stack = ActiveTrackers.Value ??= new Stack<UsageAccumulator>();
            stack.Push(tracker);
        }

        try
        {
            await next(context).ConfigureAwait(false);
        }
        finally
        {
            stopwatch.Stop();
            AccumulateUsageFromResult(context.Result);

            if (isTracked && tracker is not null)
            {
                var stack = ActiveTrackers.Value;
                if (stack is { Count: > 0 } && ReferenceEquals(stack.Peek(), tracker))
                {
                    stack.Pop();
                    if (stack.Count == 0)
                    {
                        ActiveTrackers.Value = null;
                    }
                }

                _telemetryStore.Record(new TelemetryEntry(
                    Id: Guid.NewGuid(),
                    Mode: mode!,
                    TokensIn: tracker.TokensIn,
                    TokensOut: tracker.TokensOut,
                    LatencyMs: stopwatch.ElapsedMilliseconds,
                    Timestamp: DateTimeOffset.UtcNow,
                    Source: _evalExecutionContext.IsEvalRun
                        ? TelemetrySource.Eval
                        : TelemetrySource.Tutoring));
            }
        }
    }

    private static string? MapMode(KernelFunction function)
    {
        var name = function.Name;
        var plugin = function.PluginName;

        if (plugin == nameof(ExplainPlugin) && name == ExplainPlugin.FunctionName)
        {
            return "Explain";
        }

        if (plugin == nameof(QuizPlugin) && name == QuizPlugin.GenerateQuestionsFunctionName)
        {
            return "Quiz.GenerateQuestions";
        }

        if (plugin == nameof(QuizPlugin) && name == QuizPlugin.EvaluateAnswersFunctionName)
        {
            return "Quiz.EvaluateAnswers";
        }

        if (plugin == nameof(SummarisePlugin) && name == SummarisePlugin.FunctionName)
        {
            return "Summarise";
        }

        // Fall back to function name alone for prompt-style invocations that carry Usage.
        return name switch
        {
            ExplainPlugin.FunctionName => "Explain",
            QuizPlugin.GenerateQuestionsFunctionName => "Quiz.GenerateQuestions",
            QuizPlugin.EvaluateAnswersFunctionName => "Quiz.EvaluateAnswers",
            SummarisePlugin.FunctionName => "Summarise",
            _ => null
        };
    }

    private static void AccumulateUsageFromResult(FunctionResult? result)
    {
        if (result?.Metadata is null)
        {
            return;
        }

        if (!result.Metadata.TryGetValue("Usage", out var usage) || usage is null)
        {
            return;
        }

        var stack = ActiveTrackers.Value;
        if (stack is null || stack.Count == 0)
        {
            return;
        }

        var (tokensIn, tokensOut) = ExtractTokenCounts(usage);
        var tracker = stack.Peek();
        tracker.TokensIn += tokensIn;
        tracker.TokensOut += tokensOut;
    }

    private static (int TokensIn, int TokensOut) ExtractTokenCounts(object usage)
    {
        var type = usage.GetType();

        var tokensIn =
            ReadIntProperty(usage, type, "InputTokenCount")
            ?? ReadIntProperty(usage, type, "PromptTokens")
            ?? 0;

        var tokensOut =
            ReadIntProperty(usage, type, "OutputTokenCount")
            ?? ReadIntProperty(usage, type, "CompletionTokens")
            ?? 0;

        return (tokensIn, tokensOut);
    }

    private static int? ReadIntProperty(object target, Type type, string propertyName)
    {
        var property = type.GetProperty(propertyName);
        if (property is null)
        {
            return null;
        }

        var value = property.GetValue(target);
        return value switch
        {
            int i => i,
            long l => (int)l,
            _ => null
        };
    }

    private sealed class UsageAccumulator
    {
        public int TokensIn { get; set; }

        public int TokensOut { get; set; }
    }
}
