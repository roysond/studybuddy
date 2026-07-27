import { useState, type FormEvent } from 'react';
import { evaluateQuizAnswers, generateQuizQuestions } from '../api/studyApi';
import { PlayButton } from './PlayButton';

type QuizStep = 'generate' | 'evaluate';

interface QuizPanelProps {
  studyMaterial: string;
}

/**
 * Quiz mode panel — two-step local flow: generate questions, then evaluate answers.
 * Study material is shared, passed down from App.
 */
export function QuizPanel({ studyMaterial }: QuizPanelProps) {
  const [step, setStep] = useState<QuizStep>('generate');
  const [topic, setTopic] = useState('');
  const [questions, setQuestions] = useState('');
  const [studentAnswers, setStudentAnswers] = useState('');
  const [evaluation, setEvaluation] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  function startOver() {
    setStep('generate');
    setTopic('');
    setQuestions('');
    setStudentAnswers('');
    setEvaluation('');
    setError('');
    setIsLoading(false);
  }

  async function handleGenerate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');
    setEvaluation('');

    if (!studyMaterial.trim()) {
      setError('Paste some study material above first.');
      return;
    }

    setIsLoading(true);
    try {
      const trimmedTopic = topic.trim();
      const response = await generateQuizQuestions({
        studyMaterial: studyMaterial.trim(),
        ...(trimmedTopic ? { topic: trimmedTopic } : {}),
      });
      setQuestions(response.questions);
      setStudentAnswers('');
      setStep('evaluate');
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to generate quiz questions.');
    } finally {
      setIsLoading(false);
    }
  }

  async function handleEvaluate(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');
    setEvaluation('');
    setIsLoading(true);

    try {
      const response = await evaluateQuizAnswers({
        questions,
        studentAnswers: studentAnswers.trim(),
        studyMaterial: studyMaterial.trim(),
      });
      setEvaluation(response.evaluation);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to evaluate quiz answers.');
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <section className="panel">
      <div className="panel__header">
        <div>
          <h2 className="panel__title">Quiz</h2>
          <p className="panel__subtitle">
            Generate three questions, answer them, then get tutor-style feedback.
          </p>
        </div>
        {step === 'evaluate' ? (
          <button className="button button--secondary" type="button" onClick={startOver}>
            Start Over
          </button>
        ) : null}
      </div>

      {step === 'generate' ? (
        <form className="panel__form" onSubmit={handleGenerate}>
          <label className="field">
            <span className="field__label">Topic to focus on (optional)</span>
            <input
              className="field__input"
              type="text"
              value={topic}
              onChange={(event) => setTopic(event.target.value)}
              placeholder="Leave blank and Claude will pick the focus"
              disabled={isLoading}
            />
          </label>

          <button className="button" type="submit" disabled={isLoading}>
            {isLoading ? 'Generating…' : 'Generate Questions'}
          </button>
        </form>
      ) : (
        <form className="panel__form" onSubmit={handleEvaluate}>
          <div className="result">
            <h3 className="result__title">Questions</h3>
            <pre className="result__body">{questions}</pre>
            <PlayButton text={questions} />
          </div>

          <label className="field">
            <span className="field__label">Your answers</span>
            <textarea
              className="field__textarea"
              value={studentAnswers}
              onChange={(event) => setStudentAnswers(event.target.value)}
              rows={8}
              placeholder="Type your answers for questions 1, 2, and 3…"
              disabled={isLoading}
            />
          </label>

          <button className="button" type="submit" disabled={isLoading}>
            {isLoading ? 'Evaluating…' : 'Evaluate'}
          </button>
        </form>
      )}

      {error ? <p className="message message--error" role="alert">{error}</p> : null}

      {evaluation ? (
        <div className="result">
          <h3 className="result__title">Evaluation</h3>
          <pre className="result__body">{evaluation}</pre>
          <PlayButton text={evaluation} />
        </div>
      ) : null}
    </section>
  );
}
