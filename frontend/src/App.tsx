import { useState } from 'react';
import { ExplainPanel } from './components/ExplainPanel';
import { QuizPanel } from './components/QuizPanel';
import { SummarisePanel } from './components/SummarisePanel';
import { StudyMaterialInput } from './components/StudyMaterialInput';

type StudyMode = 'explain' | 'quiz' | 'summarise';

/**
 * Top-level shell: owns the shared study material and mode switching.
 * No API calls or business logic here.
 */
function App() {
  const [mode, setMode] = useState<StudyMode>('explain');
  const [studyMaterial, setStudyMaterial] = useState('');

  return (
    <div className="app">
      <header className="app__header">
        <h1 className="app__brand">StudyBuddy</h1>
        <p className="app__tagline">Personal AI tutor for any study material</p>
      </header>

      <StudyMaterialInput
        label="Study material"
        value={studyMaterial}
        onChange={setStudyMaterial}
      />

      <nav className="mode-switcher" aria-label="Study modes">
        <button
          type="button"
          className={mode === 'explain' ? 'mode-switcher__button is-active' : 'mode-switcher__button'}
          onClick={() => setMode('explain')}
        >
          Explain
        </button>
        <button
          type="button"
          className={mode === 'quiz' ? 'mode-switcher__button is-active' : 'mode-switcher__button'}
          onClick={() => setMode('quiz')}
        >
          Quiz
        </button>
        <button
          type="button"
          className={mode === 'summarise' ? 'mode-switcher__button is-active' : 'mode-switcher__button'}
          onClick={() => setMode('summarise')}
        >
          Summarise
        </button>
      </nav>

      <main className="app__main">
        {mode === 'explain' ? <ExplainPanel studyMaterial={studyMaterial} /> : null}
        {mode === 'quiz' ? <QuizPanel studyMaterial={studyMaterial} /> : null}
        {mode === 'summarise' ? <SummarisePanel studyMaterial={studyMaterial} /> : null}
      </main>
    </div>
  );
}

export default App;
