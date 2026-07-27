import { useEffect, useRef, useState } from 'react';

interface PlayButtonProps {
  text: string;
}

/** Max characters per utterance — Chrome silently stops long utterances, so text is chunked. */
const MAX_CHUNK_LENGTH = 200;

/**
 * Splits text into speech-sized chunks on sentence boundaries where possible,
 * falling back to hard splits for very long sentences.
 */
function chunkText(text: string): string[] {
  const sentences = text.replace(/\s+/g, ' ').trim().split(/(?<=[.!?])\s+/);
  const chunks: string[] = [];
  let current = '';

  for (const sentence of sentences) {
    if (sentence.length > MAX_CHUNK_LENGTH) {
      if (current) {
        chunks.push(current);
        current = '';
      }
      for (let i = 0; i < sentence.length; i += MAX_CHUNK_LENGTH) {
        chunks.push(sentence.slice(i, i + MAX_CHUNK_LENGTH));
      }
      continue;
    }

    if ((current + ' ' + sentence).trim().length > MAX_CHUNK_LENGTH) {
      chunks.push(current);
      current = sentence;
    } else {
      current = (current + ' ' + sentence).trim();
    }
  }

  if (current) {
    chunks.push(current);
  }

  return chunks.filter((chunk) => chunk.length > 0);
}

/**
 * Picks the most natural available English voice for a tutor persona.
 * Falls back to the browser default if none of the preferred voices exist.
 */
function selectVoice(voices: SpeechSynthesisVoice[]): SpeechSynthesisVoice | null {
  if (voices.length === 0) {
    return null;
  }

  const preferredNames = ['Samantha', 'Google US English', 'Karen', 'Daniel', 'Alex'];

  for (const name of preferredNames) {
    const match = voices.find((voice) => voice.name === name);
    if (match) {
      return match;
    }
  }

  return voices.find((voice) => voice.lang.startsWith('en')) ?? voices[0];
}

/**
 * Reusable audio playback control using the browser's built-in speech synthesis.
 * Presentation + playback only — no knowledge of study modes, no network calls.
 */
export function PlayButton({ text }: PlayButtonProps) {
  const [isPlaying, setIsPlaying] = useState(false);
  const [error, setError] = useState('');
  const [voices, setVoices] = useState<SpeechSynthesisVoice[]>([]);
  const isCancelledRef = useRef(false);

  const isSupported =
    typeof window !== 'undefined' && 'speechSynthesis' in window;

  // Voices load asynchronously in most browsers.
  useEffect(() => {
    if (!isSupported) {
      return;
    }

    function loadVoices() {
      setVoices(window.speechSynthesis.getVoices());
    }

    loadVoices();
    window.speechSynthesis.addEventListener('voiceschanged', loadVoices);

    return () => {
      window.speechSynthesis.removeEventListener('voiceschanged', loadVoices);
    };
  }, [isSupported]);

  // Stop any in-flight speech when the component unmounts or the text changes.
  useEffect(() => {
    return () => {
      if (isSupported) {
        isCancelledRef.current = true;
        window.speechSynthesis.cancel();
      }
    };
  }, [text, isSupported]);

  function stop() {
    isCancelledRef.current = true;
    window.speechSynthesis.cancel();
    setIsPlaying(false);
  }

  function play() {
    setError('');

    if (!isSupported) {
      setError('Speech playback is not supported in this browser.');
      return;
    }

    // Clear any queued speech from a previous run before starting.
    window.speechSynthesis.cancel();
    isCancelledRef.current = false;

    const chunks = chunkText(text);
    if (chunks.length === 0) {
      return;
    }

    const voice = selectVoice(voices);
    setIsPlaying(true);

    chunks.forEach((chunk, index) => {
      const utterance = new SpeechSynthesisUtterance(chunk);
      utterance.rate = 1;
      utterance.pitch = 1;

      if (voice) {
        utterance.voice = voice;
        utterance.lang = voice.lang;
      }

      if (index === chunks.length - 1) {
        utterance.onend = () => {
          if (!isCancelledRef.current) {
            setIsPlaying(false);
          }
        };
      }

      utterance.onerror = (event) => {
        // 'interrupted' and 'canceled' fire on deliberate stop — not real errors.
        if (event.error === 'interrupted' || event.error === 'canceled') {
          return;
        }
        setError('Playback failed.');
        setIsPlaying(false);
      };

      window.speechSynthesis.speak(utterance);
    });
  }

  return (
    <div className="playback">
      <button
        className="button button--secondary"
        type="button"
        onClick={isPlaying ? stop : play}
        disabled={!text.trim() || !isSupported}
      >
        {isPlaying ? 'Stop' : 'Read aloud'}
      </button>
      {error ? <p className="message message--error" role="alert">{error}</p> : null}
    </div>
  );
}
