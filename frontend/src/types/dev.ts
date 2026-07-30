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

export interface EvalMetricResult {
  value: number;
  reasoning: string | null;
}

export interface EvalCaseResult {
  caseName: string;
  metrics: Record<string, EvalMetricResult>;
}

export interface ModeEvalScores {
  scores: Record<string, number>;
  caseResults: EvalCaseResult[];
}

export interface EvalRunResult {
  runAt: string;
  modeScores: Record<string, ModeEvalScores>;
}
