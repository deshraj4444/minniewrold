import { useState } from 'react';
import { Link } from 'react-router';
import { api } from '../../lib/api.js';
import { useTitle } from '../../lib/hooks.js';

export default function NumerologyPage() {
  useTitle('Numerology Calculator | AstroAura');
  const [name, setName] = useState('');
  const [dob, setDob] = useState('');
  const [result, setResult] = useState(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  async function submit(e) {
    e.preventDefault();
    setLoading(true);
    setError('');
    try {
      setResult(await api.astro.numerology(name, dob));
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="astro-container astro-section">
      <nav className="astro-breadcrumb" aria-label="Breadcrumb"><Link to="/astro">Home</Link> › Numerology</nav>
      <h1 className="astro-page-title">Numerology calculator</h1>
      <p className="astro-section-sub">Find your Moolank (root number), Bhagyank (destiny number) and Namank (name number) using the Chaldean system.</p>

      <form className="astro-card astro-inline-form" onSubmit={submit}>
        <div className="field">
          <label htmlFor="num-name">Full name</label>
          <input id="num-name" value={name} onChange={(e) => setName(e.target.value)} required maxLength={80} />
        </div>
        <div className="field">
          <label htmlFor="num-dob">Date of birth</label>
          <input id="num-dob" type="date" value={dob} onChange={(e) => setDob(e.target.value)} required />
        </div>
        <button className="astro-btn" disabled={loading}>{loading ? 'Calculating…' : 'Calculate'}</button>
        {error && <p className="form-error" role="alert">{error}</p>}
      </form>

      {result && (
        <section className="astro-result" aria-live="polite">
          <div className="astro-numbers">
            {[['Moolank', 'Root number, from your birth day', result.moolank], ['Bhagyank', 'Destiny number, from your full birth date', result.bhagyank], ['Namank', 'Name number, from your name', result.namank]].map(([label, hint, n]) => (
              <article key={label} className="astro-card astro-number">
                <span className="astro-number-label">{label}</span>
                <b className="astro-number-value">{n.number}</b>
                <h3>{n.title} <small>· {n.planet}</small></h3>
                <p>{n.description}</p>
                <small className="muted">{hint}</small>
              </article>
            ))}
          </div>
          <div className="astro-card astro-lucky">
            <div><span>Lucky numbers</span><b>{result.luckyNumbers.join(', ')}</b></div>
            <div><span>Lucky colours</span><b>{result.luckyColours.join(', ')}</b></div>
            <div><span>Lucky day</span><b>{result.luckyDay}</b></div>
          </div>
        </section>
      )}
    </div>
  );
}
