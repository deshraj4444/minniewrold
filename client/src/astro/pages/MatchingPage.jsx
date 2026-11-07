import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useTitle } from '../../lib/hooks.js';
import ShareButtons from '../../shared/ShareButtons.jsx';
import BirthForm, { birthFromParams, birthToParams, emptyBirth, isBirthComplete, toBirthInput } from '../components/BirthForm.jsx';

export default function MatchingPage() {
  useTitle('Kundli Milan: 36 Guna Matching | AstroAura');
  const [params, setParams] = useSearchParams();
  const [boy, setBoy] = useState(() => birthFromParams(params, 'b') ?? emptyBirth);
  const [girl, setGirl] = useState(() => birthFromParams(params, 'g') ?? emptyBirth);
  const [result, setResult] = useState(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const b = birthFromParams(params, 'b');
    const g = birthFromParams(params, 'g');
    if (!b || !g) return;
    setBoy(b);
    setGirl(g);
    setLoading(true);
    setError('');
    api.astro.matching({ boy: toBirthInput(b), girl: toBirthInput(g) })
      .then(setResult)
      .catch((e) => { setError(e.message); setResult(null); })
      .finally(() => setLoading(false));
  }, [params]);

  function submit(e) {
    e.preventDefault();
    if (!isBirthComplete(boy) || !isBirthComplete(girl)) { setError('Please fill in both sets of details and pick each birth place from the list.'); return; }
    setParams({ ...birthToParams(boy, 'b'), ...birthToParams(girl, 'g') });
  }

  const pct = result ? Math.round((result.total / 36) * 100) : 0;

  return (
    <div className="astro-container astro-section">
      <nav className="astro-breadcrumb" aria-label="Breadcrumb"><Link to="/astro">Home</Link> › Kundli Milan</nav>
      <h1 className="astro-page-title">Kundli Milan (Guna Milan)</h1>
      <p className="astro-section-sub">Ashtakoota matching of the boy's and girl's Moon nakshatras: 8 kootas, 36 gunas, with Manglik compatibility.</p>

      <form className="astro-card astro-match-form" onSubmit={submit}>
        <BirthForm value={boy} onChange={(b) => { setBoy(b); setError(''); }} idPrefix="boy" legend="Boy's details" />
        <BirthForm value={girl} onChange={(g) => { setGirl(g); setError(''); }} idPrefix="girl" legend="Girl's details" />
        <div className="astro-match-submit">
          {error && <p className="form-error" role="alert">{error}</p>}
          <button className="astro-btn" disabled={loading}>{loading ? 'Matching…' : 'Match Kundli'}</button>
        </div>
      </form>

      {result && (
        <section className="astro-result" aria-live="polite">
          <div className="astro-match-summary astro-card">
            <div className="astro-gauge" style={{ '--pct': pct }} role="img" aria-label={`${result.total} of 36 gunas`}>
              <div><b>{result.total}</b><span>/ 36</span></div>
            </div>
            <div>
              <h2>{result.verdict}</h2>
              <p>{result.boy.name} ({result.boy.rashi}, {result.boy.nakshatra}) and {result.girl.name} ({result.girl.rashi}, {result.girl.nakshatra}) match on <b>{result.total} of 36</b> gunas.</p>
              <p className="muted">18 or more gunas is traditionally considered acceptable for marriage.</p>
              <p><b>Manglik:</b> {result.manglikNote}</p>
              <ShareButtons title={`Kundli Milan: ${result.boy.name} & ${result.girl.name}`} className="share astro-share" />
            </div>
          </div>

          <div className="astro-table-wrap">
            <table className="astro-table">
              <thead><tr><th>Koota</th><th>Boy</th><th>Girl</th><th>Obtained</th><th>Max</th><th>Area of life</th></tr></thead>
              <tbody>
                {result.kootas.map((k) => (
                  <tr key={k.name}>
                    <th scope="row">{k.name}</th>
                    <td>{k.boy}</td>
                    <td>{k.girl}</td>
                    <td className={k.obtained === 0 ? 'warn' : k.obtained === k.maximum ? 'good' : ''}><b>{k.obtained}</b></td>
                    <td>{k.maximum}</td>
                    <td>{k.meaning}</td>
                  </tr>
                ))}
              </tbody>
              <tfoot><tr><th scope="row">Total</th><td /><td /><td><b>{result.total}</b></td><td>36</td><td /></tr></tfoot>
            </table>
          </div>

          {(result.kootas.find((k) => k.name === 'Nadi').obtained === 0 || result.kootas.find((k) => k.name === 'Bhakoot').obtained === 0) && (
            <div className="astro-verdict warn">
              <h3>Doshas to review</h3>
              <ul>
                {result.kootas.find((k) => k.name === 'Nadi').obtained === 0 && <li><b>Nadi dosha</b>: both share the {result.kootas.find((k) => k.name === 'Nadi').boy} nadi.</li>}
                {result.kootas.find((k) => k.name === 'Bhakoot').obtained === 0 && <li><b>Bhakoot dosha</b>: the Moon signs fall in a 2/12, 5/9 or 6/8 relationship.</li>}
              </ul>
              <p className="muted">Several classical exceptions cancel these doshas. <Link to="/astro/consultation">Ask an astrologer</Link> for a full reading.</p>
            </div>
          )}
        </section>
      )}
    </div>
  );
}
