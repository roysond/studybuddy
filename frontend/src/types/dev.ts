export interface TelemetrySummary {
  callsToday: number;
  averageLatencyMs: number;
  totalTokens: number;
  estimatedCostUsd: number;
  evalCallsToday: number;
  evalTotalTokens: number;
  evalEstimatedCostUsd: number;
}

export interface TelemetryEntry {
  id: string;
  mode: string;
  tokensIn: number;
  tokensOut: number;
  latencyMs: number;
  timestamp: string;
  source: string;
}

export interface ModeEvalScores {
  scores: Record<string, number>;
}

export interface EvalRunResult {
  runAt: string;
  modeScores: Record<string, ModeEvalScores>;
}
