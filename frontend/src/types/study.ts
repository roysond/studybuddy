/** Request/response contracts mirroring StudyBuddy.Application DTOs. */

export interface ExplainRequest {
  studyMaterial: string;
  userMessage?: string;
}

export interface ExplainResponse {
  explanation: string;
}

export interface QuizQuestionsRequest {
  studyMaterial: string;
  topic?: string;
}

export interface QuizQuestionsResponse {
  questions: string;
}

export interface QuizEvaluationRequest {
  questions: string;
  studentAnswers: string;
  studyMaterial: string;
}

export interface QuizEvaluationResponse {
  evaluation: string;
}

export interface SummariseRequest {
  studyMaterial: string;
}

export interface SummariseResponse {
  summary: string;
}
