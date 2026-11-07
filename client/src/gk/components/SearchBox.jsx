import { useRef, useState } from 'react';
import { useNavigate } from 'react-router';
import { useVoiceSearch, voiceSupported } from '../../lib/speech.js';

/**
 * Search for facts and articles, by typing or by voice (🎤, in browsers that support speech recognition). Spoken
 * words fill the box and search straight away. EN / हिं picks the language the microphone listens for.
 */
export default function SearchBox({ id, defaultValue = '', placeholder = 'Search facts & topics…', onSearched, className = 'gk-search' }) {
  const navigate = useNavigate();
  const input = useRef(null);
  const [voiceLang, setVoiceLang] = useState('en-IN');

  function go(q) {
    onSearched?.();
    navigate(`/gk/posts${q ? `?q=${encodeURIComponent(q)}` : ''}`);
  }

  const { listening, error, listen } = useVoiceSearch((text) => {
    if (input.current) input.current.value = text;
    go(text);
  });

  return (
    <form className={className} role="search" onSubmit={(e) => { e.preventDefault(); go(new FormData(e.currentTarget).get('q')?.trim()); }}>
      <label htmlFor={id} className="visually-hidden">Search</label>
      <input ref={input} id={id} name="q" type="search" defaultValue={defaultValue} placeholder={listening ? 'Listening… speak now' : placeholder} />
      {voiceSupported && (
        <>
          <button type="button" className={`gk-mic${listening ? ' on' : ''}`} onClick={() => listen(voiceLang)} aria-pressed={listening}
            aria-label={listening ? 'Stop listening' : `Search by voice (${voiceLang === 'hi-IN' ? 'Hindi' : 'English'})`} title="Search by voice">
            <svg viewBox="0 0 24 24" aria-hidden="true" width="18" height="18" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round"><rect x="9" y="3" width="6" height="11" rx="3" /><path d="M5 11a7 7 0 0 0 14 0M12 18v3" /></svg>
          </button>
          <button type="button" className="gk-mic-lang" onClick={() => setVoiceLang((l) => (l === 'en-IN' ? 'hi-IN' : 'en-IN'))}
            aria-label={`Voice language: ${voiceLang === 'hi-IN' ? 'Hindi' : 'English'}. Switch`} title="Voice language">
            {voiceLang === 'hi-IN' ? <span lang="hi">हिं</span> : 'EN'}
          </button>
        </>
      )}
      {error && <span className="gk-mic-error" role="status">{error}</span>}
    </form>
  );
}
