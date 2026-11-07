import { Link, useNavigate, useParams, useSearchParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import ShareButtons from '../../shared/ShareButtons.jsx';
import { zodiac } from '../data.js';

const periods = [
  { key: 'daily', label: 'Today' },
  { key: 'weekly', label: 'This week' },
  { key: 'monthly', label: 'This month' }
];

export default function HoroscopePage() {
  const { sign: signParam } = useParams();
  const [params, setParams] = useSearchParams();
  const navigate = useNavigate();
  const sign = zodiac.find((z) => z.key === signParam);
  const period = periods.some((p) => p.key === params.get('period')) ? params.get('period') : 'daily';

  useTitle(sign ? `${sign.name} Horoscope ${periods.find((p) => p.key === period).label} | AstroAura` : 'Horoscope | AstroAura');

  const horoscope = useAsync(() => (sign ? api.astro.horoscope(sign.key, period) : Promise.resolve(null)), [sign?.key, period]);

  return (
    <div className="astro-container astro-section">
      <nav className="astro-breadcrumb" aria-label="Breadcrumb"><Link to="/astro">Home</Link> › <Link to="/astro/horoscope">Horoscope</Link>{sign && <> › {sign.name}</>}</nav>
      <h1 className="astro-page-title">{sign ? `${sign.name} (${sign.hindi}) Horoscope` : 'Horoscope'}</h1>
      <p className="astro-section-sub">{sign ? `${sign.dates} · ${sign.element} sign · ruled by ${sign.lord}` : 'Pick your zodiac sign to read your daily, weekly or monthly horoscope.'}</p>

      <div className="astro-zodiac compact">
        {zodiac.map((z) => (
          <button key={z.key} className={z.key === sign?.key ? 'astro-sign active' : 'astro-sign'} onClick={() => navigate(`/astro/horoscope/${z.key}${period !== 'daily' ? `?period=${period}` : ''}`)} aria-pressed={z.key === sign?.key}>
            <span className="astro-sign-symbol" aria-hidden="true">{z.symbol}</span>
            <b>{z.name}</b>
          </button>
        ))}
      </div>

      {sign && (
        <div className="astro-card astro-horoscope">
          <div className="astro-tabs" role="tablist">
            {periods.map((p) => (
              <button key={p.key} role="tab" aria-selected={p.key === period} className={p.key === period ? 'active' : ''} onClick={() => setParams(p.key === 'daily' ? {} : { period: p.key })}>
                {p.label}
              </button>
            ))}
          </div>
          <div className="astro-horoscope-body" role="tabpanel">
            <div className="astro-horoscope-sign" aria-hidden="true">{sign.symbol}</div>
            <div>
              {horoscope.loading && <p className="muted">Reading the stars…</p>}
              {horoscope.error && <p className="form-error">Couldn't load the horoscope. Please try again.</p>}
              {horoscope.data && (
                <>
                  <p className="astro-horoscope-date">
                    {period === 'daily' && formatDate(horoscope.data.date, { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })}
                    {period === 'weekly' && `Week of ${formatDate(horoscope.data.date, { day: 'numeric', month: 'long' })}`}
                    {period === 'monthly' && formatDate(horoscope.data.date, { month: 'long', year: 'numeric' })}
                  </p>
                  <p className="astro-horoscope-text">{horoscope.data.content}</p>
                </>
              )}
              <ShareButtons title={`${sign.name} horoscope`} className="share astro-share" />
            </div>
          </div>
        </div>
      )}

      <div className="astro-card astro-note">
        <h2>Sun sign or Moon sign?</h2>
        <p>These horoscopes follow your Sun sign by birth date. In Vedic astrology your <b>rashi</b> is your Moon sign. Find yours in your <Link to="/astro/kundali">free Kundali</Link>.</p>
      </div>
    </div>
  );
}
