import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useStoredState, useTitle } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import ShareButtons from '../../shared/ShareButtons.jsx';
import BirthForm, { birthFromParams, birthToParams, emptyBirth, isBirthComplete, toBirthInput } from '../components/BirthForm.jsx';
import NorthIndianChart from '../components/NorthIndianChart.jsx';
import VarshphalView from '../components/VarshphalView.jsx';
import { grahaHindi } from '../data.js';

const tabs = ['Basic details', 'Charts', 'Planets', 'Vimshottari dasha', 'Yogas & doshas', 'Sade Sati', 'Transits', 'Varshphal'];

export default function KundaliPage() {
  useTitle('Free Kundali Online | AstroAura');
  const [params, setParams] = useSearchParams();
  const [stored, setStored] = useStoredState('astro.birth', emptyBirth);
  const [birth, setBirth] = useState(() => birthFromParams(params) ?? stored);
  const [chart, setChart] = useState(null);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  const [tab, setTab] = useState(tabs[0]);

  // A URL with birth details (from the home form or a shared link) calculates straight away.
  useEffect(() => {
    const fromUrl = birthFromParams(params);
    if (fromUrl) { setBirth(fromUrl); calculate(fromUrl); }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [params]);

  async function calculate(b) {
    setLoading(true);
    setError('');
    try {
      setChart(await api.astro.kundali(toBirthInput(b)));
      setTab(tabs[0]);
    } catch (e) {
      setError(e.message);
      setChart(null);
    } finally {
      setLoading(false);
    }
  }

  function submit(e) {
    e.preventDefault();
    if (!isBirthComplete(birth)) { setError('Please choose the birth place from the list.'); return; }
    setStored(birth);
    setParams(birthToParams(birth));
  }

  return (
    <div className="astro-container astro-section">
      <nav className="astro-breadcrumb no-print" aria-label="Breadcrumb"><Link to="/astro">Home</Link> › Free Kundali</nav>
      <h1 className="astro-page-title">Free Janam Kundali</h1>
      <p className="astro-section-sub no-print">Enter birth details to generate your Vedic birth chart with planetary positions, nakshatra, dasha and Manglik analysis.</p>

      <div className={chart ? 'astro-tool has-result' : 'astro-tool'}>
        <form className="astro-card astro-tool-form no-print" onSubmit={submit}>
          <BirthForm value={birth} onChange={(b) => { setBirth(b); setError(''); }} idPrefix="k" />
          {error && <p className="form-error" role="alert">{error}</p>}
          <button className="astro-btn astro-btn-block" disabled={loading}>{loading ? 'Calculating…' : chart ? 'Recalculate' : 'Generate Kundali'}</button>
        </form>

        {chart && (
          <section className="astro-result" aria-live="polite">
            <div className="astro-result-head">
              <div>
                <h2>{chart.birth.name}</h2>
                <p className="muted">
                  {formatDate(chart.birth.date, { day: 'numeric', month: 'long', year: 'numeric' })}, {chart.birth.time.slice(0, 5)} · {chart.birth.place || `${chart.birth.latitude}, ${chart.birth.longitude}`}
                </p>
              </div>
              <div className="astro-result-actions no-print">
                <button type="button" className="astro-btn astro-btn-ghost" onClick={() => window.print()}>Print / PDF</button>
                <ShareButtons title={`${chart.birth.name}'s Kundali`} className="share astro-share" />
              </div>
            </div>

            <div className="astro-tabs no-print" role="tablist">
              {tabs.map((t) => <button key={t} role="tab" aria-selected={tab === t} className={tab === t ? 'active' : ''} onClick={() => setTab(t)}>{t}</button>)}
            </div>

            <div role="tabpanel" className="astro-tabpanel">
              {(tab === 'Basic details') && <BasicDetails chart={chart} />}
              {(tab === 'Charts') && <Charts chart={chart} />}
              {(tab === 'Planets') && <Planets chart={chart} />}
              {(tab === 'Vimshottari dasha') && <Dashas chart={chart} />}
              {(tab === 'Yogas & doshas') && <YogasDoshas chart={chart} />}
              {(tab === 'Sade Sati') && <SadeSati chart={chart} />}
              {(tab === 'Transits') && <Transits chart={chart} />}
              {(tab === 'Varshphal') && <VarshphalView birthInput={toBirthInput(birth)} />}
            </div>

            {/* Print shows every section, not just the open tab. */}
            <div className="print-only">
              <Charts chart={chart} />
              <Planets chart={chart} />
              <Dashas chart={chart} printAll />
              <YogasDoshas chart={chart} />
              <SadeSati chart={chart} />
            </div>
          </section>
        )}
      </div>
    </div>
  );
}

function BasicDetails({ chart }) {
  const a = chart.avakhada;
  const rows = [
    ['Lagna (Ascendant)', `${a.lagna} (lord ${a.lagnaLord})`],
    ['Rashi (Moon sign)', `${a.rashi} (lord ${a.rashiLord})`],
    ['Nakshatra', `${a.nakshatra}, pada ${a.pada} (lord ${a.nakshatraLord})`],
    ['Sun sign (sidereal)', a.sunSign],
    ['Varna', a.varna], ['Vashya', a.vashya], ['Yoni', a.yoni], ['Gana', a.gana], ['Nadi', a.nadi],
    ['Tithi at birth', a.tithi], ['Yoga at birth', a.yoga], ['Karana at birth', a.karana], ['Day of birth', a.weekday],
    ['Ayanamsa (Lahiri)', `${chart.ayanamsa.toFixed(4)}°`],
    ['Time zone', chart.birth.timeZone]
  ];
  return (
    <div className="astro-basic">
      <div className="astro-highlights">
        <div><span>Lagna</span><b>{a.lagna}</b></div>
        <div><span>Rashi</span><b>{a.rashi}</b></div>
        <div><span>Nakshatra</span><b>{a.nakshatra}</b></div>
        {chart.currentMahadasha && <div><span>Running dasha</span><b>{chart.currentMahadasha.lord}{chart.currentAntardasha && ` / ${chart.currentAntardasha.lord}`}</b></div>}
      </div>
      <table className="astro-table kv">
        <tbody>{rows.map(([k, v]) => <tr key={k}><th scope="row">{k}</th><td>{v}</td></tr>)}</tbody>
      </table>
    </div>
  );
}

function Charts({ chart }) {
  return (
    <div className="astro-charts">
      <NorthIndianChart houses={chart.lagnaChart} title="Lagna chart (D1)" />
      <NorthIndianChart houses={chart.moonChart} title="Chandra chart (Moon)" showAscendant={false} />
      <NorthIndianChart houses={chart.navamsaChart} title="Navamsa chart (D9)" />
      <NorthIndianChart houses={chart.insights.dashamsaChart} title="Dashamsa chart (D10, career)" />
      <p className="muted chart-legend">Numbers are rashis (1 Mesha … 12 Meena). Su Sun, Mo Moon, Ma Mars, Me Mercury, Ju Jupiter, Ve Venus, Sa Saturn, Ra Rahu, Ke Ketu, (R) retrograde.</p>
    </div>
  );
}

function Planets({ chart }) {
  const rows = [chart.lagna, ...chart.planets];
  return (
    <div className="astro-table-wrap">
      <table className="astro-table">
        <thead>
          <tr><th>Planet</th><th>Rashi</th><th>Degree</th><th>Nakshatra</th><th>Pada</th><th>Nak. lord</th><th>House</th><th>Status</th></tr>
        </thead>
        <tbody>
          {rows.map((p) => (
            <tr key={p.graha}>
              <th scope="row">{p.graha === 'Lagna' ? 'Ascendant' : `${p.graha} (${grahaHindi[p.graha]})`}{p.retrograde && <span className="badge" title="Retrograde">R</span>}</th>
              <td>{p.rashiName}</td>
              <td>{p.degree}</td>
              <td>{p.nakshatra}</td>
              <td>{p.pada}</td>
              <td>{p.nakshatraLord}</td>
              <td>{p.house}</td>
              <td className={p.dignity === 'Exalted' || p.dignity === 'Own sign' ? 'good' : p.dignity === 'Debilitated' ? 'warn' : ''}>{p.dignity || '—'}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function Dashas({ chart, printAll = false }) {
  const [open, setOpen] = useState(chart.currentMahadasha?.lord ?? null);
  const today = new Date().toISOString().slice(0, 10);
  const fmt = (d) => formatDate(d, { day: 'numeric', month: 'short', year: 'numeric' });
  return (
    <div className="astro-dashas">
      <p className="muted">Vimshottari mahadashas from birth. Click a mahadasha for its antardashas.</p>
      {chart.dashas.map((d) => {
        const current = d.start <= today && today < d.end;
        const isOpen = printAll || open === d.lord;
        return (
          <div key={d.lord + d.start} className={current ? 'astro-dasha current' : 'astro-dasha'}>
            <button type="button" className="astro-dasha-head" aria-expanded={isOpen} onClick={() => setOpen(isOpen ? null : d.lord)}>
              <b>{d.lord} mahadasha</b>
              <span>{fmt(d.start)} – {fmt(d.end)}</span>
              {current && <span className="badge">Running</span>}
            </button>
            {isOpen && (
              <ul className="astro-antardashas">
                {d.antardashas.map((ad) => (
                  <li key={ad.lord + ad.start} className={ad.start <= today && today < ad.end ? 'current' : ''}>
                    <span>{d.lord} – {ad.lord}</span>
                    <span>{fmt(ad.start)} – {fmt(ad.end)}</span>
                  </li>
                ))}
              </ul>
            )}
          </div>
        );
      })}
    </div>
  );
}

function YogasDoshas({ chart }) {
  const { yogas, kalsarp } = chart.insights;
  return (
    <div className="astro-yogas">
      <h3>Yogas in the chart</h3>
      {yogas.length === 0 && <p className="muted">None of the common yogas checked here (Gajakesari, Budhaditya, Pancha Mahapurusha, Sunapha/Anapha/Durudhara, Kemadruma) is formed.</p>}
      <ul className="astro-yoga-list">
        {yogas.map((y) => <li key={y.name}><b>{y.name}</b><span>{y.description}</span></li>)}
      </ul>
      <h3>Doshas</h3>
      <Manglik chart={chart} />
      <div className={kalsarp.present ? 'astro-verdict warn' : 'astro-verdict good'}>
        <h3>{kalsarp.present ? kalsarp.name + ' dosha' : 'No Kalsarp dosha'}</h3>
        <p>{kalsarp.summary}</p>
      </div>
    </div>
  );
}

function SadeSati({ chart }) {
  const s = chart.insights.sadeSati;
  const fmt = (d) => formatDate(d, { month: 'short', year: 'numeric' });
  return (
    <div className="astro-sadesati">
      <div className={s.inSadeSati ? 'astro-verdict warn' : s.inDhaiya ? 'astro-verdict warn' : 'astro-verdict good'}>
        <h3>{s.inSadeSati ? `Sade Sati running: ${s.phase}` : s.inDhaiya ? 'Shani Dhaiya running' : 'No Sade Sati now'}</h3>
        <p>{s.summary}</p>
      </div>
      <h3>Sade Sati periods in your life</h3>
      <p className="muted">Sade Sati is the roughly 7½ years when Saturn transits the 12th, 1st and 2nd signs from your Moon sign ({s.moonSign}).</p>
      <div className="astro-table-wrap">
        <table className="astro-table">
          <thead><tr><th>#</th><th>Starts</th><th>Ends</th><th></th></tr></thead>
          <tbody>
            {s.periods.map((p, i) => (
              <tr key={p.start} className={p.isCurrent ? 'is-current' : ''}>
                <td>{i + 1}</td><td>{fmt(p.start)}</td><td>{fmt(p.end)}</td><td>{p.isCurrent && <span className="badge">Running</span>}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function Transits({ chart }) {
  const t = chart.insights;
  return (
    <div>
      <p className="muted">Planets in the sky on {formatDate(t.transitDate, { day: 'numeric', month: 'long', year: 'numeric' })}, counted from your Moon sign ({chart.avakhada.rashi}) as in traditional gochar.</p>
      <div className="astro-table-wrap">
        <table className="astro-table">
          <thead><tr><th>Planet</th><th>Transiting</th><th>Degree</th><th>House from Moon</th><th>Effect</th></tr></thead>
          <tbody>
            {t.transits.map((p) => (
              <tr key={p.graha}>
                <th scope="row">{p.graha} ({grahaHindi[p.graha]}){p.retrograde && p.graha !== 'Rahu' && p.graha !== 'Ketu' && <span className="badge">R</span>}</th>
                <td>{p.rashiName}</td><td>{p.degree}</td><td>{p.houseFromMoon}</td>
                <td className={p.favourable ? 'good' : 'warn'}>{p.favourable ? 'Favourable' : 'Challenging'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
}

function Manglik({ chart }) {
  const m = chart.manglik;
  const manglik = m.fromLagna || m.fromMoon;
  return (
    <div className={manglik ? 'astro-verdict warn' : 'astro-verdict good'}>
      <h3>{manglik ? 'Manglik dosha present' : 'No Manglik dosha'}</h3>
      <p>{m.summary}</p>
      <ul>
        <li>Mars from Lagna: house {m.marsHouseFromLagna}</li>
        <li>Mars from Moon: house {m.marsHouseFromMoon}</li>
      </ul>
      <p className="muted">Manglik dosha is traditionally checked from houses 1, 2, 4, 7, 8 and 12. Many exceptions can cancel it, so ask an astrologer before drawing conclusions.</p>
    </div>
  );
}
