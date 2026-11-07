import { useState } from 'react';
import { Link, useNavigate } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useStoredState, useTitle } from '../../lib/hooks.js';
import { formatEnd, formatTime } from '../../lib/format.js';
import BirthForm, { birthToParams, emptyBirth, isBirthComplete } from '../components/BirthForm.jsx';
import { services, zodiac } from '../data.js';

export default function AstroHome() {
  useTitle('AstroAura | Free Kundali, Horoscope, Panchang & Kundli Milan');
  const navigate = useNavigate();
  const [birth, setBirth] = useStoredState('astro.birth', emptyBirth);
  const [error, setError] = useState('');
  const panchang = useAsync(() => api.astro.panchang(), []);
  const testimonials = useAsync(() => api.astro.testimonials(), []);

  function submit(e) {
    e.preventDefault();
    if (!isBirthComplete(birth)) { setError('Please choose your birth place from the list.'); return; }
    navigate(`/astro/kundali?${new URLSearchParams(birthToParams(birth))}`);
  }

  const p = panchang.data;

  return (
    <>
      <section className="astro-hero">
        <div className="astro-container astro-hero-grid">
          <div className="astro-hero-copy">
            <p className="astro-eyebrow">Vedic astrology, free and accurate</p>
            <h1>Your free Janam Kundali in seconds</h1>
            <p>Birth chart, planetary positions, nakshatra, Vimshottari dasha, Manglik check, Kundli Milan and daily Panchang, all calculated live for your exact birth place.</p>
            <div className="astro-hero-stats">
              <div><b>9</b><span>grahas</span></div>
              <div><b>36</b><span>guna matching</span></div>
              <div><b>27</b><span>nakshatras</span></div>
            </div>
          </div>
          <form className="astro-card astro-quick-form" onSubmit={submit}>
            <h2>Get your free Kundali</h2>
            <BirthForm value={birth} onChange={(b) => { setBirth(b); setError(''); }} idPrefix="home" />
            {error && <p className="form-error" role="alert">{error}</p>}
            <button className="astro-btn astro-btn-block">Generate Kundali</button>
          </form>
        </div>
      </section>

      <section className="astro-container astro-section">
        <h2 className="astro-section-title">Free astrology services</h2>
        <div className="astro-services">
          {services.map((s) => (
            <Link key={s.to} to={s.to} className="astro-service">
              <span className="astro-service-icon" aria-hidden="true">{s.icon}</span>
              <h3>{s.title}</h3>
              <p>{s.text}</p>
            </Link>
          ))}
        </div>
      </section>

      <section className="astro-band">
        <div className="astro-container astro-section">
          <h2 className="astro-section-title">Today's horoscope</h2>
          <p className="astro-section-sub">Choose your sign (rashi) for today's prediction.</p>
          <div className="astro-zodiac">
            {zodiac.map((z) => (
              <Link key={z.key} to={`/astro/horoscope/${z.key}`} className="astro-sign">
                <span className="astro-sign-symbol" aria-hidden="true">{z.symbol}</span>
                <b>{z.name}</b>
                <small>{z.hindi}</small>
              </Link>
            ))}
          </div>
        </div>
      </section>

      <section className="astro-container astro-section astro-two-col">
        <div className="astro-card">
          <div className="astro-card-head">
            <h2>Today's Panchang</h2>
            <Link to="/astro/panchang">Full Panchang →</Link>
          </div>
          {panchang.loading && <p className="muted">Calculating…</p>}
          {panchang.error && <p className="form-error">Couldn't load the Panchang.</p>}
          {p && (
            <dl className="astro-kv">
              <dt>Place</dt><dd>{p.place}</dd>
              <dt>Vara</dt><dd>{p.vara}</dd>
              <dt>Tithi</dt><dd>{p.tithi.name}, {p.paksha} <small>until {formatEnd(p.tithi.endsAt, p.date)}</small></dd>
              <dt>Nakshatra</dt><dd>{p.nakshatra.name} <small>until {formatEnd(p.nakshatra.endsAt, p.date)}</small></dd>
              <dt>Yoga</dt><dd>{p.yoga.name}</dd>
              <dt>Sunrise / Sunset</dt><dd>{formatTime(p.sunrise)} / {formatTime(p.sunset)}</dd>
              <dt>Rahu Kaal</dt><dd className="warn">{formatTime(p.rahuKaal.start)} – {formatTime(p.rahuKaal.end)}</dd>
              <dt>Abhijit Muhurat</dt><dd className="good">{formatTime(p.abhijitMuhurta.start)} – {formatTime(p.abhijitMuhurta.end)}</dd>
            </dl>
          )}
        </div>

        <div className="astro-card astro-cta-card">
          <h2>Need personal guidance?</h2>
          <p>Our astrologers read your kundali in detail and answer your questions on career, marriage, health, finance and remedies.</p>
          <ul className="astro-ticks">
            <li>Detailed kundali reading</li>
            <li>Marriage and compatibility</li>
            <li>Career and business timing</li>
            <li>Gemstone and remedy advice</li>
          </ul>
          <Link to="/astro/consultation" className="astro-btn">Book a consultation</Link>
        </div>
      </section>

      {testimonials.data?.length > 0 && (
        <section className="astro-band">
          <div className="astro-container astro-section">
            <h2 className="astro-section-title">What people say</h2>
            <div className="astro-testimonials">
              {testimonials.data.map((t) => (
                <blockquote key={t.id} className="astro-card astro-testimonial">
                  <div className="stars" aria-label={`${t.rating} out of 5`}>{'★'.repeat(t.rating)}{'☆'.repeat(5 - t.rating)}</div>
                  <p>“{t.quote}”</p>
                  <footer>{t.customerName}{t.location && <span> · {t.location}</span>}</footer>
                </blockquote>
              ))}
            </div>
          </div>
        </section>
      )}
    </>
  );
}
