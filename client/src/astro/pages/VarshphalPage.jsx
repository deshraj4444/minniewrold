import { useState } from 'react';
import { Link } from 'react-router';
import { useStoredState, useTitle } from '../../lib/hooks.js';
import BirthForm, { emptyBirth, isBirthComplete, toBirthInput } from '../components/BirthForm.jsx';
import VarshphalView from '../components/VarshphalView.jsx';

export default function VarshphalPage() {
  useTitle('Varshphal (Annual Horoscope) | AstroAura');
  const [stored, setStored] = useStoredState('astro.birth', emptyBirth);
  const [birth, setBirth] = useState(stored);
  const [input, setInput] = useState(() => (isBirthComplete(stored) ? toBirthInput(stored) : null));
  const [error, setError] = useState('');

  function submit(e) {
    e.preventDefault();
    if (!isBirthComplete(birth)) { setError('Please choose the birth place from the list.'); return; }
    setStored(birth);
    setInput(toBirthInput(birth));
  }

  return (
    <div className="astro-container astro-section">
      <nav className="astro-breadcrumb no-print" aria-label="Breadcrumb"><Link to="/astro">Home</Link> › Varshphal</nav>
      <h1 className="astro-page-title">Varshphal: annual horoscope</h1>
      <p className="astro-section-sub no-print">
        The Tajik annual chart, cast for the moment the Sun returns to its exact birth position each year. See the Varsha Lagna, Muntha,
        year lord and Mudda dasha for any year of your life.
      </p>

      <div className={input ? 'astro-tool has-result' : 'astro-tool'}>
        <form className="astro-card astro-tool-form no-print" onSubmit={submit}>
          <BirthForm value={birth} onChange={(b) => { setBirth(b); setError(''); }} idPrefix="v" />
          {error && <p className="form-error" role="alert">{error}</p>}
          <button className="astro-btn astro-btn-block">{input ? 'Update' : 'Show Varshphal'}</button>
        </form>

        {input && (
          <section className="astro-result" aria-live="polite">
            <div className="astro-result-head">
              <div>
                <h2>{input.name}'s Varshphal</h2>
                <p className="muted">Born {input.date}, {input.time} · {input.place}</p>
              </div>
              <div className="astro-result-actions no-print">
                <button type="button" className="astro-btn astro-btn-ghost" onClick={() => window.print()}>Print / PDF</button>
              </div>
            </div>
            <VarshphalView birthInput={input} />
          </section>
        )}
      </div>
    </div>
  );
}
