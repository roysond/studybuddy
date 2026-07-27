import { useState, type FormEvent } from 'react';
import { summariseMaterial } from '../api/studyApi';

interface SummarisePanelProps {
  studyMaterial: string;
}

/**
 * Summarise mode panel — owns its own result and error state.
 * Study material is shared, passed down from App.
 */
export function SummarisePanel({ studyMaterial }: SummarisePanelProps) {
  const [summary, setSummary] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setError('');
    setSummary('');

    if (!studyMaterial.trim()) {
      setError('Paste some study material above first.');
      return;
    }

    setIsLoading(true);
    try {
      const response = await summariseMaterial({ studyMaterial: studyMaterial.trim() });
      setSummary(response.summary);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to summarise material.');
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <section className="panel">
      <h2 className="panel__title">Summarise</h2>
      <p className="panel__subtitle">
        Condense your study material into the five most important points to remember.
      </p>

      <form className="panel__form" onSubmit={handleSubmit}>
        <button className="button" type="submit" disabled={isLoading}>
          {isLoading ? 'Summarising…' : 'Summarise'}
        </button>
      </form>

      {error ? <p className="message message--error" role="alert">{error}</p> : null}

      {summary ? (
        <div className="result">
          <h3 className="result__title">Summary</h3>
          <pre className="result__body">{summary}</pre>
        </div>
      ) : null}
    </section>
  );
}
