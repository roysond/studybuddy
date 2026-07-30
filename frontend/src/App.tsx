import { useEffect, useState } from 'react';
import { DeveloperDashboard } from './components/DeveloperDashboard';
import { ExplainPanel } from './components/ExplainPanel';
import { QuizPanel } from './components/QuizPanel';
import { SummarisePanel } from './components/SummarisePanel';
import { StudyMaterialInput } from './components/StudyMaterialInput';

type StudyMode = 'explain' | 'quiz' | 'summarise';
type AppView = 'study' | 'dev';

function readViewFromPath(): AppView {
  return window.location.pathname.startsWith('/dev') ? 'dev' : 'study';
}

/**
 * Top-level shell: owns the shared study material, mode switching, and the
 * Developer view (separate from student-facing tutoring modes).
 */
function App() {
  const [view, setView] = useState<AppView>(readViewFromPath);
  const [mode, setMode] = useState<StudyMode>('explain');
  const [studyMaterial, setStudyMaterial] = useState('');

  useEffect(() => {
    function onPopState() {
      setView(readViewFromPath());
    }

    window.addEventListener('popstate', onPopState);
    return () => window.removeEventListener('popstate', onPopState);
  }, []);

  function goStudy() {
    window.history.pushState({}, '', '/');
    setView('study');
  }

  function goDev() {
    window.history.pushState({}, '', '/dev');
    setView('dev');
  }

  return (
    <div className="app">
      <header className="app__header">
        <div className="app__header-row">
          <div>
            <h1 className="app__brand">StudyBuddy</h1>
            <p className="app__tagline">Personal AI tutor for any study material</p>
          </div>
          <nav className="app__nav" aria-label="App sections">
            <button
              type="button"
              className={
                view === 'study'
                  ? 'app__nav-link is-active'
                  : 'app__nav-link'
              }
              onClick={goStudy}
            >
              Study
            </button>
            <button
              type="button"
              className={
                view === 'dev'
                  ? 'app__nav-link app__nav-link--dev is-active'
                  : 'app__nav-link app__nav-link--dev'
              }
              onClick={goDev}
            >
              Developer
            </button>
          </nav>
        </div>
      </header>

      {view === 'dev' ? (
        <DeveloperDashboard />
      ) : (
        <>
          <StudyMaterialInput
            label="Study material"
            value={studyMaterial}
            onChange={setStudyMaterial}
          />

          <nav className="mode-switcher" aria-label="Study modes">
            <button
              type="button"
              className={
                mode === 'explain'
                  ? 'mode-switcher__button is-active'
                  : 'mode-switcher__button'
              }
              onClick={() => setMode('explain')}
            >
              Explain
            </button>
            <button
              type="button"
              className={
                mode === 'quiz'
                  ? 'mode-switcher__button is-active'
                  : 'mode-switcher__button'
              }
              onClick={() => setMode('quiz')}
            >
              Quiz
            </button>
            <button
              type="button"
              className={
                mode === 'summarise'
                  ? 'mode-switcher__button is-active'
                  : 'mode-switcher__button'
              }
              onClick={() => setMode('summarise')}
            >
              Summarise
            </button>
          </nav>

          <main className="app__main">
            {mode === 'explain' ? (
              <ExplainPanel studyMaterial={studyMaterial} />
            ) : null}
            {mode === 'quiz' ? (
              <QuizPanel studyMaterial={studyMaterial} />
            ) : null}
            {mode === 'summarise' ? (
              <SummarisePanel studyMaterial={studyMaterial} />
            ) : null}
          </main>
        </>
      )}
    </div>
  );
}

export default App;
