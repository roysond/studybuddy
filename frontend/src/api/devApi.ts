import { API_BASE_URL } from '../config';
import type {
  EvalRunResult,
  TelemetryEntry,
  TelemetrySummary,
} from '../types/dev';

/**
 * HTTP helpers for the Developer Dashboard.
 * Kept separate from studyApi so telemetry/evals stay decoupled from tutoring calls.
 */
async function getJson<TResponse>(path: string): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'GET',
    headers: { Accept: 'application/json' },
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(
      errorText.trim() || `Request failed with status ${response.status}`,
    );
  }

  return (await response.json()) as TResponse;
}

async function postJson<TResponse>(path: string): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    headers: { Accept: 'application/json' },
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(
      errorText.trim() || `Request failed with status ${response.status}`,
    );
  }

  return (await response.json()) as TResponse;
}

export function fetchTelemetrySummary(): Promise<TelemetrySummary> {
  return getJson<TelemetrySummary>('/api/dev/telemetry/summary');
}

export function fetchTelemetryRecent(
  count = 20,
): Promise<TelemetryEntry[]> {
  return getJson<TelemetryEntry[]>(`/api/dev/telemetry/recent?count=${count}`);
}

export function runEvals(): Promise<EvalRunResult> {
  return postJson<EvalRunResult>('/api/dev/evals/run');
}

export function fetchLatestEvals(): Promise<EvalRunResult> {
  return getJson<EvalRunResult>('/api/dev/evals/latest');
}
