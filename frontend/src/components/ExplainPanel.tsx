import { useState, type FormEvent } from 'react';
import { explain } from '../api/studyApi';

interface ExplainPanelProps {
  studyMaterial: string;
}

/**
 * Explain mode panel — owns its own question, loading, result, and error state.
 * Study material is shared, passed down from App.
 */
export function ExplainPanel({ studyMaterial }: ExplainPanelProps) {
  const [userMessage, setUserMessage] = useState('');
  const [explanation, setExplanation] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');
    setExplanation('');

    if (!studyMaterial.trim()) {
      setError('Paste some study material above first.');
      return;
    }

    setIsLoading(true);
    try {
      const trimmedQuestion = userMessage.trim();
      const response = await explain({
        studyMaterial: studyMaterial.trim(),
        ...(trimmedQuestion ? { userMessage: trimmedQuestion } : {}),
      });
      setExplanation(response.explanation);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to get explanation.');
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <section className="panel">
      <h2 className="panel__title">Explain</h2>
      <p className="panel__subtitle">
        Optionally ask a specific question — leave it blank and the whole study material will be explained.
      </p>

      <form className="panel__form" onSubmit={handleSubmit}>
        <label className="field">
          <span className="field__label">Your question (optional)</span>
          <input
            className="field__input"
            type="text"
            value={userMessage}
            onChange={(event) => setUserMessage(event.target.value)}
            placeholder="Leave blank to explain the whole material"
            disabled={isLoading}
          />
        </label>

        <button className="button" type="submit" disabled={isLoading}>
          {isLoading ? 'Explaining…' : 'Explain'}
        </button>
      </form>

      {error ? <p className="message message--error" role="alert">{error}</p> : null}

      {explanation ? (
        <div className="result">
          <h3 className="result__title">Explanation</h3>
          <pre className="result__body">{explanation}</pre>
        </div>
      ) : null}
    </section>
  );
}
