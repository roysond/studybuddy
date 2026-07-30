using Microsoft.Extensions.Options;
using StudyBuddy.Application.Interfaces;
using StudyBuddy.Domain.Models;
using StudyBuddy.Infrastructure.ExternalServices;

namespace StudyBuddy.Infrastructure.Telemetry;

/// <summary>
/// Thread-safe, bounded in-memory telemetry ring for the Developer Dashboard.
/// Keeps only the most recent <see cref="MaxEntries"/> entries.
/// </summary>
public sealed class InMemoryTelemetryStore : ITelemetryStore
{
    public const int MaxEntries = 200;

    private readonly object _gate = new();
    private readonly LinkedList<TelemetryEntry> _entries = new();
    private readonly OpenRouterOptions _openRouter;

    public InMemoryTelemetryStore(IOptions<OpenRouterOptions> openRouterOptions)
    {
        ArgumentNullException.ThrowIfNull(openRouterOptions);
        _openRouter = openRouterOptions.Value;
    }

    public void Record(TelemetryEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        lock (_gate)
        {
            _entries.AddLast(entry);
            while (_entries.Count > MaxEntries)
            {
                _entries.RemoveFirst();
            }
        }
    }

    public IReadOnlyList<TelemetryEntry> GetRecent(int count)
    {
        if (count <= 0)
        {
            return [];
        }

        lock (_gate)
        {
            return _entries
                .Reverse()
                .Take(count)
                .ToList();
        }
    }

    public TelemetrySummary GetSummary()
    {
        lock (_gate)
        {
            var today = DateTimeOffset.UtcNow.Date;
            var todays = _entries
                .Where(e => e.Timestamp.UtcDateTime.Date == today)
                .ToList();

            var tutoring = todays
                .Where(e => e.Source == TelemetrySource.Tutoring)
                .ToList();
            var evals = todays
                .Where(e => e.Source == TelemetrySource.Eval)
                .ToList();

            var tutoringTokensIn = tutoring.Sum(e => e.TokensIn);
            var tutoringTokensOut = tutoring.Sum(e => e.TokensOut);
            var tutoringTotalTokens = tutoringTokensIn + tutoringTokensOut;

            var evalTokensIn = evals.Sum(e => e.TokensIn);
            var evalTokensOut = evals.Sum(e => e.TokensOut);
            var evalTotalTokens = evalTokensIn + evalTokensOut;

            var averageLatency = tutoring.Count == 0
                ? 0
                : Math.Round(tutoring.Average(e => (double)e.LatencyMs), 1);

            return new TelemetrySummary(
                CallsToday: tutoring.Count,
                AverageLatencyMs: averageLatency,
                TotalTokens: tutoringTotalTokens,
                EstimatedCostUsd: Math.Round(CalculateCostUsd(tutoringTokensIn, tutoringTokensOut), 4),
                EvalCallsToday: evals.Count,
                EvalTotalTokens: evalTotalTokens,
                EvalEstimatedCostUsd: Math.Round(CalculateCostUsd(evalTokensIn, evalTokensOut), 4));
        }
    }

    private decimal CalculateCostUsd(int tokensIn, int tokensOut) =>
        (tokensIn / 1_000_000m * _openRouter.InputCostPerMillionUsd)
        + (tokensOut / 1_000_000m * _openRouter.OutputCostPerMillionUsd);
}
