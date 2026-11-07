import { useState } from 'react';
import { api } from '../../lib/api.js';
import { useTitle } from '../../lib/hooks.js';

export default function GkContact() {
  useTitle('Contact | Curious Minds');
  const [status, setStatus] = useState({ type: '', message: '' });
  const [sending, setSending] = useState(false);

  async function submit(e) {
    e.preventDefault();
    const form = e.currentTarget;
    setSending(true);
    try {
      const res = await api.contact({ ...Object.fromEntries(new FormData(form)), siteKey: 'gk' });
      setStatus({ type: 'success', message: res.message });
      form.reset();
    } catch (err) {
      setStatus({ type: 'error', message: err.message });
    } finally {
      setSending(false);
    }
  }

  return (
    <div className="gk-wrap gk-section gk-contact">
      <div>
        <h1 className="gk-page-title">Say hello 👋</h1>
        <p className="gk-lead">Spotted a mistake? Have a topic you want explained? Tell us.</p>
      </div>
      <form className="gk-form" onSubmit={submit}>
        {status.type && <p className={status.type === 'success' ? 'gk-ok' : 'gk-err'} role="status">{status.message}</p>}
        <label>Name<input name="fullName" required maxLength={120} /></label>
        <label>Email<input name="email" type="email" required /></label>
        <label>Subject<input name="subject" maxLength={200} /></label>
        <label>Message<textarea name="message" rows={5} required minLength={5} maxLength={4000} /></label>
        <button className="gk-btn" disabled={sending}>{sending ? 'Sending…' : 'Send message'}</button>
      </form>
    </div>
  );
}
