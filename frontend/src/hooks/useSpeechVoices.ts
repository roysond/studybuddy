import { useEffect, useState } from 'react';

const STORAGE_KEY = 'studybuddy.voiceUri';

/**
 * Loads the browser's available speech voices and remembers the user's choice.
 * Voices load asynchronously in most browsers, hence the voiceschanged listener.
 */
export function useSpeechVoices() {
  const [voices, setVoices] = useState<SpeechSynthesisVoice[]>([]);
  const [selectedUri, setSelectedUri] = useState<string>('');

  const isSupported = typeof window !== 'undefined' && 'speechSynthesis' in window;

  useEffect(() => {
    if (!isSupported) {
      return;
    }

    function loadVoices() {
      const available = window.speechSynthesis
        .getVoices()
        .filter((voice) => voice.lang.startsWith('en'));

      setVoices(available);

      setSelectedUri((current) => {
        if (current && available.some((voice) => voice.voiceURI === current)) {
          return current;
        }

        const remembered = window.localStorage.getItem(STORAGE_KEY);
        if (remembered && available.some((voice) => voice.voiceURI === remembered)) {
          return remembered;
        }

        return available[0]?.voiceURI ?? '';
      });
    }

    loadVoices();
    window.speechSynthesis.addEventListener('voiceschanged', loadVoices);

    return () => {
      window.speechSynthesis.removeEventListener('voiceschanged', loadVoices);
    };
  }, [isSupported]);

  function selectVoice(voiceUri: string) {
    setSelectedUri(voiceUri);
    window.localStorage.setItem(STORAGE_KEY, voiceUri);
  }

  const selectedVoice =
    voices.find((voice) => voice.voiceURI === selectedUri) ?? null;

  return { voices, selectedVoice, selectVoice, isSupported };
}
