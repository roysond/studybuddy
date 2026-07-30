import { useEffect, useState } from 'react';
import {
  fetchTelemetryRecent,
  fetchTelemetrySummary,
} from '../api/devApi';
import type { TelemetryEntry, TelemetrySummary } from '../types/dev';

const POLL_INTERVAL_MS = 4000;

const EMPTY_SUMMARY: TelemetrySummary = {
  callsToday: 0,
  averageLatencyMs: 0,
  totalTokens: 0,
  estimatedCostUsd: 0,
  evalCallsToday: 0,
  evalTotalTokens: 0,
  evalEstimatedCostUsd: 0,
};

/**
 * Polls telemetry summary + recent calls every 4 seconds while mounted.
 */
export function useTelemetryPolling(recentCount = 20) {
  const [summary, setSummary] = useState<TelemetrySummary>(EMPTY_SUMMARY);
  const [recent, setRecent] = useState<TelemetryEntry[]>([]);
  const [error, setError] = useState('');
  const [hasLoadedOnce, setHasLoadedOnce] = useState(false);

  useEffect(() => {
    let cancelled = false;

    async function load() {
      try {
        const [nextSummary, nextRecent] = await Promise.all([
          fetchTelemetrySummary(),
          fetchTelemetryRecent(recentCount),
        ]);

        if (cancelled) {
          return;
        }

        setSummary(nextSummary);
        setRecent(nextRecent);
        setHasLoadedOnce(true);
        setError('');
      } catch (err) {
        if (cancelled) {
          return;
        }

        setError(
          err instanceof Error ? err.message : 'Failed to load telemetry.',
        );
      }
    }

    void load();
    const intervalId = window.setInterval(() => {
      void load();
    }, POLL_INTERVAL_MS);

    return () => {
      cancelled = true;
      window.clearInterval(intervalId);
    };
  }, [recentCount]);

  const isStale = Boolean(error) && hasLoadedOnce;

  return { summary, recent, error, isStale };
}
