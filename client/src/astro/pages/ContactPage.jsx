import { useState } from 'react';
import { Link } from 'react-router';
import { api } from '../../lib/api.js';
import { useTitle } from '../../lib/hooks.js';

export default function ContactPage() {
  useTitle('Contact | AstroAura');
  const [status, setStatus] = useState({ type: '', message: '' });
  const [sending, setSending] = useState(false);

  async function submit(e) {
    e.preventDefault();
    const form = e.currentTarget;
    setSending(true);
    try {
      const res = await api.contact({ ...Object.fromEntries(new FormData(form)), siteKey: 'astro' });
      setStatus({ type: 'success', message: res.message });
      form.reset();
    } catch (err) {
      setStatus({ type: 'error', message: err.message });
    } finally {
      setSending(false);
    }
  }

  return (
    <div className="astro-container astro-section astro-narrow">
      <nav className="astro-breadcrumb" aria-label="Breadcrumb"><Link to="/astro">Home</Link> › Contact</nav>
      <h1 className="astro-page-title">Contact us</h1>
      <form className="astro-card astro-form" onSubmit={submit}>
        {status.type && <p className={status.type === 'success' ? 'form-success' : 'form-error'} role="status">{status.message}</p>}
        <div className="field-row">
          <div className="field"><label htmlFor="ct-name">Name</label><input id="ct-name" name="fullName" required maxLength={120} /></div>
          <div className="field"><label htmlFor="ct-email">Email</label><input id="ct-email" name="email" type="email" required /></div>
        </div>
        <div className="field"><label htmlFor="ct-subject">Subject</label><input id="ct-subject" name="subject" maxLength={200} /></div>
        <div className="field"><label htmlFor="ct-message">Message</label><textarea id="ct-message" name="message" rows={5} required minLength={5} maxLength={4000} /></div>
        <button className="astro-btn" disabled={sending}>{sending ? 'Sending…' : 'Send message'}</button>
      </form>
    </div>
  );
}
