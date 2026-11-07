import { useState } from 'react';
import { Link } from 'react-router';
import { api } from '../../lib/api.js';
import { useTitle } from '../../lib/hooks.js';
import { useContactInfo, whatsAppUrl, WhatsAppIcon } from '../components/WhatsApp.jsx';

const waTemplate = ['Namaste, I would like a consultation.', 'Name:', 'Date of birth:', 'Time of birth:', 'Place of birth:', 'My question:'].join('\n');

const serviceTypes = ['Detailed Kundali reading', 'Marriage & Kundli Milan', 'Career & business', 'Health', 'Finance & property', 'Gemstone & remedies', 'Other question'];

export default function ConsultationPage() {
  useTitle('Talk to an Astrologer | AstroAura');
  const [status, setStatus] = useState({ type: '', message: '', errors: {} });
  const [sending, setSending] = useState(false);
  const contact = useContactInfo();

  async function submit(e) {
    e.preventDefault();
    const form = e.currentTarget;
    setSending(true);
    try {
      const res = await api.astro.consultation(Object.fromEntries(new FormData(form)));
      setStatus({ type: 'success', message: res.message, errors: {} });
      form.reset();
    } catch (err) {
      setStatus({ type: 'error', message: err.message, errors: err.errors });
    } finally {
      setSending(false);
    }
  }

  const err = (k) => status.errors?.[k]?.[0];

  return (
    <div className="astro-container astro-section">
      <nav className="astro-breadcrumb" aria-label="Breadcrumb"><Link to="/astro">Home</Link> › Talk to an astrologer</nav>
      <div className="astro-two-col">
        <div>
          <h1 className="astro-page-title">Talk to an astrologer</h1>
          <p className="astro-section-sub">Share your birth details and question. An experienced Vedic astrologer will study your chart and contact you within 24 hours.</p>
          <ul className="astro-ticks">
            <li>Personal reading based on your exact birth chart</li>
            <li>Clear answers on timing using your dasha periods</li>
            <li>Practical remedies: mantras, gemstones, charity</li>
            <li>Your details stay private</li>
          </ul>
          {contact?.whatsAppNumber && (
            <div className="astro-card astro-wa-card">
              <h2>Chat directly on WhatsApp</h2>
              <p className="muted">Message {contact.astrologerName || 'our astrologer'} on WhatsApp. Send your name, date, time and place of birth with your question.</p>
              <a className="astro-btn astro-wa-btn" href={whatsAppUrl(contact.whatsAppNumber, contact.greeting || waTemplate)} target="_blank" rel="noreferrer">
                <WhatsAppIcon /> Chat on WhatsApp
              </a>
            </div>
          )}
        </div>

        <form className="astro-card astro-form" onSubmit={submit} noValidate={false}>
          {status.type === 'success' && <p className="form-success" role="status">{status.message}</p>}
          {status.type === 'error' && <p className="form-error" role="alert">{status.message}</p>}
          <div className="field"><label htmlFor="c-name">Full name</label><input id="c-name" name="fullName" required maxLength={120} aria-invalid={!!err('fullName')} /></div>
          <div className="field-row">
            <div className="field"><label htmlFor="c-email">Email</label><input id="c-email" name="email" type="email" required aria-invalid={!!err('email')} /></div>
            <div className="field"><label htmlFor="c-phone">Phone</label><input id="c-phone" name="phone" type="tel" required maxLength={30} /></div>
          </div>
          <div className="field-row">
            <div className="field"><label htmlFor="c-date">Date of birth</label><input id="c-date" name="birthDate" type="date" required /></div>
            <div className="field"><label htmlFor="c-time">Time of birth</label><input id="c-time" name="birthTime" type="time" required /></div>
          </div>
          <div className="field"><label htmlFor="c-place">Birth place</label><input id="c-place" name="birthPlace" required maxLength={200} /></div>
          <div className="field">
            <label htmlFor="c-service">Consultation for</label>
            <select id="c-service" name="serviceType" required defaultValue={serviceTypes[0]}>{serviceTypes.map((s) => <option key={s}>{s}</option>)}</select>
          </div>
          <div className="field"><label htmlFor="c-notes">Your question</label><textarea id="c-notes" name="notes" rows={4} maxLength={2000} /></div>
          <button className="astro-btn astro-btn-block" disabled={sending}>{sending ? 'Sending…' : 'Request consultation'}</button>
        </form>
      </div>
    </div>
  );
}
