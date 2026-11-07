import { useState } from 'react';
import { api } from '../../lib/api.js';
import { useTitle } from '../../lib/hooks.js';

export default function DharmaContact() {
  useTitle('Contact | Dharma Path');
  const [status, setStatus] = useState({ type: '', message: '' });
  const [sending, setSending] = useState(false);

  async function submit(e) {
    e.preventDefault();
    const form = e.currentTarget;
    setSending(true);
    try {
      const res = await api.contact({ ...Object.fromEntries(new FormData(form)), siteKey: 'dharma' });
      setStatus({ type: 'success', message: res.message });
      form.reset();
    } catch (err) {
      setStatus({ type: 'error', message: err.message });
    } finally {
      setSending(false);
    }
  }

  return (
    <div className="dh-narrow">
      <h1 className="dh-page-title">Write to us</h1>
      <p className="dh-muted dh-center">Questions, suggestions for topics, or corrections: we read every message.</p>
      <form className="dh-form" onSubmit={submit}>
        {status.type && <p className={status.type === 'success' ? 'dh-ok' : 'dh-err'} role="status">{status.message}</p>}
        <label>Name<input name="fullName" required maxLength={120} /></label>
        <label>Email<input name="email" type="email" required /></label>
        <label>Subject<input name="subject" maxLength={200} /></label>
        <label>Message<textarea name="message" rows={6} required minLength={5} maxLength={4000} /></label>
        <button className="dh-btn" disabled={sending}>{sending ? 'Sending…' : 'Send'}</button>
      </form>
    </div>
  );
}
