import { useState } from 'react';
import { Link } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useStoredState, useTitle } from '../../lib/hooks.js';
import { formatDate, formatEnd, formatTime, todayIso } from '../../lib/format.js';
import PlaceSearch, { placeLabel } from '../components/PlaceSearch.jsx';

const delhi = { name: 'New Delhi', region: 'Delhi', country: 'India', latitude: 28.6139, longitude: 77.209, timeZone: 'Asia/Kolkata' };

const shiftDate = (iso, days) => {
  const d = new Date(`${iso}T12:00:00`);
  d.setDate(d.getDate() + days);
  return d.toISOString().slice(0, 10);
};

export default function PanchangPage() {
  useTitle("Today's Panchang | AstroAura");
  const [place, setPlace] = useStoredState('astro.panchangPlace', delhi);
  const [pending, setPending] = useState(place);
  const [date, setDate] = useState(todayIso());

  const { data: p, loading, error } = useAsync(
    () => api.astro.panchang({ date, lat: place.latitude, lon: place.longitude, tz: place.timeZone, place: placeLabel(place) }),
    [date, place.latitude, place.longitude, place.timeZone]
  );

  const range = (r) => `${formatTime(r.start)} – ${formatTime(r.end)}`;

  return (
    <div className="astro-container astro-section">
      <nav className="astro-breadcrumb" aria-label="Breadcrumb"><Link to="/astro">Home</Link> › Panchang</nav>
      <h1 className="astro-page-title">Panchang</h1>
      <p className="astro-section-sub">The five limbs of the day (tithi, vara, nakshatra, yoga, karana) with sunrise, sunset and auspicious and inauspicious times for your city.</p>

      <form className="astro-card astro-panchang-form" onSubmit={(e) => { e.preventDefault(); if (pending) setPlace(pending); }}>
        <PlaceSearch value={pending} onChange={setPending} label="City" id="panchang-place" />
        <div className="field">
          <label htmlFor="panchang-date">Date</label>
          <div className="astro-date-nav">
            <button type="button" className="astro-btn astro-btn-ghost" onClick={() => setDate((d) => shiftDate(d, -1))} aria-label="Previous day">‹</button>
            <input id="panchang-date" type="date" value={date} onChange={(e) => e.target.value && setDate(e.target.value)} required />
            <button type="button" className="astro-btn astro-btn-ghost" onClick={() => setDate((d) => shiftDate(d, 1))} aria-label="Next day">›</button>
          </div>
        </div>
        <button className="astro-btn" disabled={!pending}>Show Panchang</button>
      </form>

      {loading && <p className="muted">Calculating…</p>}
      {error && <p className="form-error" role="alert">{error.message}</p>}
      {p && (
        <div className="astro-panchang">
          <div className="astro-panchang-head astro-card">
            <div>
              <h2>{formatDate(p.date, { weekday: 'long', day: 'numeric', month: 'long', year: 'numeric' })}</h2>
              <p className="muted">{p.place}</p>
            </div>
            <div className="astro-sun">
              <span>☀ Sunrise <b>{formatTime(p.sunrise)}</b></span>
              <span>☾ Sunset <b>{formatTime(p.sunset)}</b></span>
            </div>
          </div>

          <div className="astro-panchang-grid">
            <section className="astro-card">
              <h3>Panchang</h3>
              <dl className="astro-kv">
                <dt>Tithi</dt><dd>{p.tithi.name} <small>until {formatEnd(p.tithi.endsAt, p.date)}</small></dd>
                <dt>Paksha</dt><dd>{p.paksha}</dd>
                <dt>Vara</dt><dd>{p.vara}</dd>
                <dt>Nakshatra</dt><dd>{p.nakshatra.name} <small>until {formatEnd(p.nakshatra.endsAt, p.date)}</small></dd>
                <dt>Yoga</dt><dd>{p.yoga.name} <small>until {formatEnd(p.yoga.endsAt, p.date)}</small></dd>
                <dt>Karana</dt><dd>{p.karana.name} <small>until {formatEnd(p.karana.endsAt, p.date)}</small></dd>
              </dl>
            </section>

            <section className="astro-card">
              <h3>Hindu calendar</h3>
              <dl className="astro-kv">
                <dt>Month (Amanta)</dt><dd>{p.amantaMonth}</dd>
                <dt>Month (Purnimanta)</dt><dd>{p.purnimantaMonth}</dd>
                <dt>Vikram Samvat</dt><dd>{p.vikramSamvat}</dd>
                <dt>Shaka Samvat</dt><dd>{p.shakaSamvat}</dd>
                <dt>Ritu</dt><dd>{p.ritu}</dd>
                <dt>Sun sign</dt><dd>{p.sunSign}</dd>
                <dt>Moon sign</dt><dd>{p.moonSign}</dd>
              </dl>
            </section>

            <section className="astro-card">
              <h3>Auspicious times</h3>
              <dl className="astro-kv">
                <dt>Brahma Muhurat</dt><dd className="good">{range(p.brahmaMuhurta)}</dd>
                <dt>Abhijit Muhurat</dt><dd className="good">{range(p.abhijitMuhurta)}</dd>
              </dl>
              <h3>Inauspicious times</h3>
              <dl className="astro-kv">
                <dt>Rahu Kaal</dt><dd className="warn">{range(p.rahuKaal)}</dd>
                <dt>Yamaganda</dt><dd className="warn">{range(p.yamaganda)}</dd>
                <dt>Gulika Kaal</dt><dd className="warn">{range(p.gulika)}</dd>
              </dl>
            </section>
          </div>
          <p className="muted">Times are local to {p.timeZone}. Tithi, nakshatra and yoga are those prevailing at sunrise.</p>
        </div>
      )}
    </div>
  );
}
