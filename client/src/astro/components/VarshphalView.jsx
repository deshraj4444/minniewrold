import { useEffect, useState } from 'react';
import { api } from '../../lib/api.js';
import { formatDate } from '../../lib/format.js';
import NorthIndianChart from './NorthIndianChart.jsx';
import { grahaHindi } from '../data.js';

const dateTime = (v) => `${formatDate(v, { day: 'numeric', month: 'long', year: 'numeric' })}, ${v.slice(11, 16)}`;
const shortDate = (v) => formatDate(v, { day: 'numeric', month: 'short', year: 'numeric' });

/** Year picker plus the Tajik annual chart for the given birth details (API shape from toBirthInput). */
export default function VarshphalView({ birthInput, initialYear }) {
  const birthYear = Number(birthInput.date.slice(0, 4));
  const [year, setYear] = useState(() => Math.max(initialYear ?? new Date().getFullYear(), birthYear));
  const [data, setData] = useState(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const key = JSON.stringify(birthInput);

  useEffect(() => {
    let live = true;
    setLoading(true);
    setError('');
    api.astro.varshphal(birthInput, year)
      .then((d) => live && setData(d))
      .catch((e) => live && (setError(e.message), setData(null)))
      .finally(() => live && setLoading(false));
    return () => { live = false; };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [key, year]);

  const now = new Date().toISOString();

  return (
    <div className="astro-varshphal">
      <div className="astro-date-nav no-print">
        <button type="button" className="astro-btn astro-btn-ghost" disabled={year <= birthYear || loading} onClick={() => setYear((y) => y - 1)}>‹ {year - 1}</button>
        <label>
          <span className="visually-hidden">Year</span>
          <input type="number" min={birthYear} max={2200} value={year} onChange={(e) => e.target.value && setYear(Math.max(birthYear, Number(e.target.value)))} />
        </label>
        <button type="button" className="astro-btn astro-btn-ghost" disabled={loading} onClick={() => setYear((y) => y + 1)}>{year + 1} ›</button>
      </div>

      {error && <p className="form-error" role="alert">{error}</p>}
      {loading && !data && <p className="muted">Calculating Varshphal…</p>}

      {data && (
        <div className={loading ? 'is-loading' : ''}>
          <div className="astro-highlights">
            <div><span>Varsha Pravesh</span><b>{dateTime(data.varshaPravesh)}</b></div>
            <div><span>Varsha Lagna</span><b>{data.lagna.rashiName}</b></div>
            <div><span>Muntha</span><b>{data.muntha.rashiName} · {data.muntha.house}H</b></div>
            <div><span>Year lord</span><b>{data.yearLord} ({grahaHindi[data.yearLord]})</b></div>
          </div>

          <p className="muted">
            Annual chart for the year {data.year}–{data.year + 1} (age {data.age} completed, {ordinal(data.age + 1)} year running), cast for the moment the Sun returns
            to its birth position, at the birth place. {data.dayChart ? 'Day' : 'Night'} chart, {data.weekday}.
          </p>

          <div className="astro-charts">
            <NorthIndianChart houses={data.chart} title={`Varshphal ${data.year} chart`} />
            <div className={data.muntha.favourable ? 'astro-verdict good' : 'astro-verdict warn'}>
              <h3>Muntha in the {ordinal(data.muntha.house)} house</h3>
              <p>{data.muntha.effect}</p>
              <ul>
                <li>Muntha sign: {data.muntha.rashiName} (lord {data.muntha.lord})</li>
                <li>Punya Saham: {data.punyaSaham}</li>
                <li>Next Varsha Pravesh: {dateTime(data.nextVarshaPravesh)}</li>
              </ul>
            </div>
          </div>

          <h3>Panchadhikari (five office-bearers)</h3>
          <div className="astro-table-wrap">
            <table className="astro-table">
              <thead><tr><th>Office</th><th>Planet</th><th>Dignity</th><th>Aspects Lagna</th></tr></thead>
              <tbody>
                {data.officeBearers.map((b) => (
                  <tr key={b.role} className={b.graha === data.yearLord ? 'is-current' : ''}>
                    <th scope="row">{b.role}</th>
                    <td>{b.graha}{b.graha === data.yearLord && <span className="badge">Year lord</span>}</td>
                    <td>{b.dignity}</td>
                    <td>{b.aspectsLagna ? 'Yes' : 'No'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <p className="muted">{data.yearLordReason} Strength here is judged by sign dignity, a simplified form of the full Pancha-vargiya bala.</p>

          <h3>Planets at Varsha Pravesh</h3>
          <div className="astro-table-wrap">
            <table className="astro-table">
              <thead><tr><th>Planet</th><th>Rashi</th><th>Degree</th><th>Nakshatra</th><th>House</th><th>Status</th></tr></thead>
              <tbody>
                {[data.lagna, ...data.planets].map((p) => (
                  <tr key={p.graha}>
                    <th scope="row">{p.graha === 'Lagna' ? 'Ascendant' : p.graha}{p.retrograde && <span className="badge">R</span>}</th>
                    <td>{p.rashiName}</td><td>{p.degree}</td><td>{p.nakshatra} {p.pada}</td><td>{p.house}</td><td>{p.dignity || '—'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <h3>Mudda dasha (Vimshottari for the year)</h3>
          <div className="astro-table-wrap">
            <table className="astro-table">
              <thead><tr><th>Dasha</th><th>From</th><th>To</th><th>Days</th></tr></thead>
              <tbody>
                {data.muddaDashas.map((d) => {
                  const current = d.start <= now && now < d.end;
                  return (
                    <tr key={d.lord} className={current ? 'is-current' : ''}>
                      <th scope="row">{d.lord}{current && <span className="badge">Running</span>}</th>
                      <td>{shortDate(d.start)}</td><td>{shortDate(d.end)}</td><td>{d.days}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

const ordinal = (n) => n + ([11, 12, 13].includes(n % 100) ? 'th' : ({ 1: 'st', 2: 'nd', 3: 'rd' }[n % 10] ?? 'th'));
