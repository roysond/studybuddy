import { API_BASE_URL } from '../config';
import type {
  ExplainRequest,
  ExplainResponse,
  QuizEvaluationRequest,
  QuizEvaluationResponse,
  QuizQuestionsRequest,
  QuizQuestionsResponse,
  SummariseRequest,
  SummariseResponse,
} from '../types/study';

/**
 * The only module that performs HTTP calls to the StudyBuddy backend.
 * Components must call these functions — never fetch() directly.
 */
async function postJson<TRequest, TResponse>(
  path: string,
  body: TRequest,
): Promise<TResponse> {
  const response = await fetch(`${API_BASE_URL}${path}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      Accept: 'application/json',
    },
    body: JSON.stringify(body),
  });

  if (!response.ok) {
    const errorText = await response.text();
    throw new Error(
      errorText.trim() || `Request failed with status ${response.status}`,
    );
  }

  return (await response.json()) as TResponse;
}

export function explain(request: ExplainRequest): Promise<ExplainResponse> {
  return postJson<ExplainRequest, ExplainResponse>(
    '/api/study/explain',
    request,
  );
}

export function generateQuizQuestions(
  request: QuizQuestionsRequest,
): Promise<QuizQuestionsResponse> {
  return postJson<QuizQuestionsRequest, QuizQuestionsResponse>(
    '/api/study/quiz/questions',
    request,
  );
}

export function evaluateQuizAnswers(
  request: QuizEvaluationRequest,
): Promise<QuizEvaluationResponse> {
  return postJson<QuizEvaluationRequest, QuizEvaluationResponse>(
    '/api/study/quiz/evaluate',
    request,
  );
}

export function summariseMaterial(
  request: SummariseRequest,
): Promise<SummariseResponse> {
  return postJson<SummariseRequest, SummariseResponse>(
    '/api/study/summarise',
    request,
  );
}
