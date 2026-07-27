import { useEffect, useRef, useState } from 'react';
import { useSpeechVoices } from '../hooks/useSpeechVoices';

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
 * Reusable audio playback control using the browser's built-in speech synthesis.
 * Presentation + playback only — no knowledge of study modes, no network calls.
 */
export function PlayButton({ text }: PlayButtonProps) {
  const [isPlaying, setIsPlaying] = useState(false);
  const [error, setError] = useState('');
  const [rate, setRate] = useState(1);
  const isCancelledRef = useRef(false);
  const { voices, selectedVoice, selectVoice, isSupported } = useSpeechVoices();

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

    const voice = selectedVoice;
    setIsPlaying(true);

    chunks.forEach((chunk, index) => {
      const utterance = new SpeechSynthesisUtterance(chunk);
      utterance.rate = rate;
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
      <div className="playback__controls">
        <button
          className="button button--secondary"
          type="button"
          onClick={isPlaying ? stop : play}
          disabled={!text.trim() || !isSupported}
        >
          {isPlaying ? 'Stop' : 'Read aloud'}
        </button>

        {voices.length > 0 ? (
          <label className="playback__field">
            <span className="playback__label">Voice</span>
            <select
              className="playback__select"
              value={selectedVoice?.voiceURI ?? ''}
              onChange={(event) => selectVoice(event.target.value)}
              disabled={isPlaying}
            >
              {voices.map((voice) => (
                <option key={voice.voiceURI} value={voice.voiceURI}>
                  {voice.name}
                </option>
              ))}
            </select>
          </label>
        ) : null}

        <label className="playback__field">
          <span className="playback__label">Speed {rate.toFixed(1)}x</span>
          <input
            className="playback__range"
            type="range"
            min="0.5"
            max="1.5"
            step="0.1"
            value={rate}
            onChange={(event) => setRate(Number(event.target.value))}
            disabled={isPlaying}
          />
        </label>
      </div>

      {error ? <p className="message message--error" role="alert">{error}</p> : null}
    </div>
  );
}
