import { useEffect, useState } from 'react';
import { api } from '../../lib/api.js';
import { Field, Notice } from '../components.jsx';

const keys = {
  number: 'astro.whatsappNumber',
  greeting: 'astro.whatsappGreeting',
  name: 'astro.astrologerName',
  upiId: 'payments.upiId',
  payee: 'payments.payeeName',
  payHelp: 'payments.helpWhatsApp',
  review: 'site.googleReviewUrl'
};

// The same check as the server (PublicEndpoints.IsGoogleLink): only Google links are shown on the site.
function isGoogleLink(url) {
  try {
    const u = new URL(url.trim());
    return u.protocol === 'https:'
      && (['g.page', 'maps.app.goo.gl', 'goo.gl'].includes(u.hostname) || /^([a-z0-9-]+\.)*google\.[a-z]{2,3}(\.[a-z]{2})?$/i.test(u.hostname));
  } catch {
    return false;
  }
}

export default function Settings() {
  const [values, setValues] = useState(null);
  const [notice, setNotice] = useState(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    api.get('/admin/settings').then(setValues).catch((e) => setNotice({ type: 'error', message: e.message }));
  }, []);

  if (!values) return <p className="muted">Loading…</p>;

  const bind = (key) => ({ value: values[key] ?? '', onChange: (e) => setValues((v) => ({ ...v, [key]: e.target.value })) });
  const digits = (values[keys.number] ?? '').replace(/\D/g, '');

  async function save(e) {
    e.preventDefault();
    setSaving(true);
    try {
      setValues(await api.put('/admin/settings', { values }));
      setNotice({ type: 'ok', message: 'Settings saved.' });
    } catch (err) {
      setNotice({ type: 'error', message: err.message });
    } finally {
      setSaving(false);
    }
  }

  return (
    <>
      <h1 className="admin-h1">Settings</h1>
      <Notice notice={notice} />
      <form className="admin-card admin-inline-form" onSubmit={save}>
        <h2>AstroAura: chat on WhatsApp</h2>
        <p className="muted small">
          When a number is set, every AstroAura page shows a “Chat on WhatsApp” button, and the Talk to Astrologer page shows a WhatsApp chat box.
          Visitors' messages arrive directly in the astrologer's WhatsApp.
        </p>
        <Field label="Astrologer's WhatsApp number" hint="With country code, e.g. 91 98765 43210. Leave empty to hide the WhatsApp buttons.">
          <input {...bind(keys.number)} inputMode="tel" maxLength={30} placeholder="91 98765 43210" />
        </Field>
        {digits && digits.length < 11 && <p className="af-error">Add the country code (91 for India) in front of the number.</p>}
        <Field label="Astrologer's name (optional)"><input {...bind(keys.name)} maxLength={100} placeholder="Pandit ji" /></Field>
        <Field label="First message typed for the visitor (optional)" hint="Visitors can edit it before sending.">
          <textarea {...bind(keys.greeting)} rows={3} maxLength={500} placeholder="Namaste, I would like a consultation." />
        </Field>
        <h2>Mock tests: payments by UPI</h2>
        <p className="muted small">
          Students pay paid tests to this UPI ID (QR code or UPI app), then enter their transaction ID. You approve it in admin → Payments.
          Paid tests can't be unlocked until a UPI ID is set.
        </p>
        <Field label="Your UPI ID" hint="Google Pay → tap your photo → your UPI ID, e.g. yourname@okaxis">
          <input {...bind(keys.upiId)} maxLength={100} placeholder="yourname@okaxis" autoCapitalize="none" spellCheck={false} />
        </Field>
        {values[keys.upiId] && !/^[\w.-]{2,}@[a-zA-Z]{2,}$/.test(values[keys.upiId].trim()) && <p className="af-error">That doesn't look like a UPI ID (name@bank).</p>}
        <Field label="Name shown to students when paying" hint="Should match the name on your bank account, so students trust the payment."><input {...bind(keys.payee)} maxLength={100} /></Field>
        <Field label="WhatsApp number for payment help (optional)" hint="With country code. Students with a payment problem can message this number.">
          <input {...bind(keys.payHelp)} inputMode="tel" maxLength={30} placeholder="91 98765 43210" />
        </Field>
        <h2>Rate us on Google</h2>
        <p className="muted small">
          When set, the Curious Minds footer and result pages show a “Rate us on Google” button. To get the link: open your Google Business Profile
          (search your business name on Google while signed in) → <b>Ask for reviews</b> (or <b>Get more reviews</b>) → copy the link.
        </p>
        <Field label="Google review link" hint="Looks like https://g.page/r/…/review. Leave empty to hide the button.">
          <input {...bind(keys.review)} maxLength={500} placeholder="https://g.page/r/XXXXXXXX/review" autoCapitalize="none" spellCheck={false} />
        </Field>
        {values[keys.review] && !isGoogleLink(values[keys.review]) && <p className="af-error">This must be a Google link starting with https:// (g.page, maps.app.goo.gl or google.com). Other links are not shown.</p>}
        <div className="actions">
          <button className="admin-btn" disabled={saving}>{saving ? 'Saving…' : 'Save settings'}</button>
          {digits.length >= 11 && <a className="admin-btn admin-btn-ghost" href={`https://wa.me/${digits}`} target="_blank" rel="noreferrer">Test the link ↗</a>}
        </div>
      </form>
    </>
  );
}
