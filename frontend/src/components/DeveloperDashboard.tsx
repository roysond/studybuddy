import { useEffect, useState } from 'react';
import { fetchLatestEvals, runEvals } from '../api/devApi';
import { useTelemetryPolling } from '../hooks/useTelemetryPolling';
import type { EvalRunResult } from '../types/dev';

const METRIC_ORDER = [
  'Groundedness',
  'Relevance',
  'Completeness',
  'Fluency',
  'Coherence',
  'Truthfulness',
] as const;

/**
 * Developer Dashboard — live telemetry (polled) + on-demand evaluation (button-driven).
 * Not student-facing; visually distinct from Explain/Quiz/Summarise panels.
 */
export function DeveloperDashboard() {
  const {
    summary,
    recent,
    error: telemetryError,
    isStale,
  } = useTelemetryPolling(20);
  const [evalResult, setEvalResult] = useState<EvalRunResult | null>(null);
  const [isRunningEvals, setIsRunningEvals] = useState(false);
  const [evalError, setEvalError] = useState('');

  useEffect(() => {
    let cancelled = false;

    async function loadLatest() {
      try {
        const latest = await fetchLatestEvals();
        if (!cancelled) {
          setEvalResult(latest);
          setEvalError('');
        }
      } catch {
        // 404 before the first run is expected — leave empty state.
        if (!cancelled) {
          setEvalResult(null);
        }
      }
    }

    void loadLatest();
    return () => {
      cancelled = true;
    };
  }, []);

  async function handleRunEvals() {
    setIsRunningEvals(true);
    setEvalError('');

    try {
      const result = await runEvals();
      setEvalResult(result);
    } catch (err) {
      setEvalError(
        err instanceof Error ? err.message : 'Eval run failed.',
      );
    } finally {
      setIsRunningEvals(false);
    }
  }

  return (
    <div className="dev-dashboard">
      <header className="dev-dashboard__header">
        <p className="dev-dashboard__eyebrow">Developer</p>
        <h2 className="dev-dashboard__title">Observability</h2>
        <p className="dev-dashboard__subtitle">
          Live Kernel telemetry and on-demand quality evaluation — separate from
          the student tutoring modes.
        </p>
      </header>

      <section className="panel dev-section" aria-labelledby="telemetry-heading">
        <h3 id="telemetry-heading" className="panel__title">
          Traceability
        </h3>
        <p className="panel__subtitle">
          Polls every 4 seconds — these polls read cached in-memory data and cost
          nothing. Costs below are estimates calculated from recorded token counts,
          not live billing.
        </p>

        {telemetryError ? (
          <p
            className={isStale ? 'message' : 'message message--error'}
            role="alert"
          >
            {isStale
              ? `${telemetryError} — showing last known data.`
              : telemetryError}
          </p>
        ) : null}

        <div className="metric-grid">
          <article className="metric-card">
            <p className="metric-card__label">Calls today</p>
            <p className="metric-card__value">{summary.callsToday}</p>
          </article>
          <article className="metric-card">
            <p className="metric-card__label">Avg latency</p>
            <p className="metric-card__value">
              {summary.averageLatencyMs.toFixed(0)}
              <span className="metric-card__unit">ms</span>
            </p>
          </article>
          <article className="metric-card">
            <p className="metric-card__label">Tokens used</p>
            <p className="metric-card__value">{summary.totalTokens}</p>
          </article>
          <article className="metric-card">
            <p className="metric-card__label">Est. cost (tutoring)</p>
            <p className="metric-card__value">
              ${summary.estimatedCostUsd.toFixed(4)}
            </p>
          </article>
          <article className="metric-card">
            <p className="metric-card__label">Eval cost ({summary.evalCallsToday} runs)</p>
            <p className="metric-card__value">
              ${summary.evalEstimatedCostUsd.toFixed(4)}
            </p>
          </article>
        </div>

        <div className="telemetry-table-wrap">
          <table className="telemetry-table">
            <thead>
              <tr>
                <th scope="col">Mode</th>
                <th scope="col">Source</th>
                <th scope="col">Time</th>
                <th scope="col">Tokens in</th>
                <th scope="col">Tokens out</th>
                <th scope="col">Latency</th>
              </tr>
            </thead>
            <tbody>
              {recent.length === 0 ? (
                <tr>
                  <td colSpan={6} className="telemetry-table__empty">
                    No Kernel calls recorded yet. Use Explain, Quiz, or Summarise
                    to generate traffic.
                  </td>
                </tr>
              ) : (
                recent.map((entry) => (
                  <tr key={entry.id}>
                    <td>{entry.mode}</td>
                    <td>{entry.source}</td>
                    <td>{formatTime(entry.timestamp)}</td>
                    <td>{entry.tokensIn}</td>
                    <td>{entry.tokensOut}</td>
                    <td>{entry.latencyMs} ms</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </section>

      <section className="panel dev-section" aria-labelledby="evals-heading">
        <div className="panel__header">
          <div>
            <h3 id="evals-heading" className="panel__title">
              Evaluation
            </h3>
            <p className="panel__subtitle">
              Runs a curated test set through each tutoring service and scores
              with Quality evaluators. Not polled — updates on button click or
              page load.
            </p>
          </div>
          <button
            className="button"
            type="button"
            onClick={() => {
              void handleRunEvals();
            }}
            disabled={isRunningEvals}
          >
            {isRunningEvals ? 'Running evals…' : 'Run evals'}
          </button>
        </div>

        {evalError ? (
          <p className="message message--error" role="alert">
            {evalError}
          </p>
        ) : null}

        {isRunningEvals ? (
          <p className="message">
            Evaluating Explain, Quiz, and Summarise — this makes real LLM calls
            and can take a minute or two.
          </p>
        ) : null}

        {evalResult ? (
          <div className="eval-results">
            <p className="eval-results__timestamp">
              Last run: {formatTimestamp(evalResult.runAt)}
            </p>
            {Object.entries(evalResult.modeScores).map(([mode, scores]) => (
              <div key={mode} className="eval-mode">
                <h4 className="eval-mode__title">{mode}</h4>
                <ul className="score-list">
                  {METRIC_ORDER.map((metric) => {
                    const value = scores.scores[metric] ?? 0;
                    const percent = Math.min(100, (value / 5) * 100);
                    return (
                      <li key={metric} className="score-row">
                        <span className="score-row__label">{metric}</span>
                        <div
                          className="score-bar"
                          role="meter"
                          aria-valuemin={0}
                          aria-valuemax={5}
                          aria-valuenow={value}
                          aria-label={`${metric} score`}
                        >
                          <div
                            className="score-bar__fill"
                            style={{ width: `${percent}%` }}
                          />
                        </div>
                        <span className="score-row__value">
                          {value.toFixed(2)}
                        </span>
                      </li>
                    );
                  })}
                </ul>
              </div>
            ))}
          </div>
        ) : !isRunningEvals ? (
          <p className="message">
            No eval results yet. Click Run evals to score the hardcoded test set.
          </p>
        ) : null}
      </section>
    </div>
  );
}

function formatTime(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) {
    return iso;
  }

  return date.toLocaleTimeString(undefined, {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
  });
}

function formatTimestamp(iso: string): string {
  const date = new Date(iso);
  if (Number.isNaN(date.getTime())) {
    return iso;
  }

  return date.toLocaleString();
}
