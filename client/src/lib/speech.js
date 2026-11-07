import { useCallback, useEffect, useRef, useState } from 'react';

const Recognition = typeof window !== 'undefined' ? window.SpeechRecognition || window.webkitSpeechRecognition : undefined;

/** True in browsers that can turn speech into text (Chrome, Edge, Safari, Android). Firefox can't, so no mic is shown there. */
export const voiceSupported = Boolean(Recognition);

/**
 * Voice input for a search box. `listen()` starts the microphone (the browser asks permission the first time) and
 * calls `onResult(text)` with what was said. Speech is processed by the browser; nothing is sent to this site.
 * `lang` is 'en-IN' (English) or 'hi-IN' (Hindi).
 */
export function useVoiceSearch(onResult) {
  const [listening, setListening] = useState(false);
  const [error, setError] = useState('');
  const rec = useRef(null);
  const cb = useRef(onResult);
  cb.current = onResult;

  useEffect(() => () => rec.current?.abort(), []);

  const listen = useCallback((lang = 'en-IN') => {
    if (!Recognition) return;
    if (rec.current) { rec.current.abort(); rec.current = null; setListening(false); return; }
    const r = new Recognition();
    r.lang = lang;
    r.interimResults = false;
    r.maxAlternatives = 1;
    r.onresult = (e) => {
      const text = e.results?.[0]?.[0]?.transcript?.trim();
      if (text) cb.current(text);
    };
    r.onerror = (e) => setError(e.error === 'not-allowed' ? 'Microphone permission was blocked.' : e.error === 'no-speech' ? 'Didn’t hear anything. Try again.' : '');
    r.onend = () => { rec.current = null; setListening(false); };
    setError('');
    rec.current = r;
    setListening(true);
    try { r.start(); } catch { setListening(false); rec.current = null; }
  }, []);

  return { listening, error, listen };
}
