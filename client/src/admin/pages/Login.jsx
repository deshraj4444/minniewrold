import { useState } from 'react';
import { api } from '../../lib/api.js';

export default function Login({ onLogin }) {
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);

  async function submit(e) {
    e.preventDefault();
    setBusy(true);
    setError('');
    try {
      onLogin(await api.post('/auth/login', Object.fromEntries(new FormData(e.currentTarget))));
    } catch (err) {
      setError(err.status === 429 ? 'Too many attempts. Please wait a few minutes.' : err.message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="admin-login">
      <form onSubmit={submit} className="admin-card">
        <h1>MinnieWorld admin</h1>
        <p className="muted">Sign in to manage AstroAura, Dharma Path and Curious Minds.</p>
        {error && <p className="admin-notice error" role="alert">{error}</p>}
        <label className="af"><span className="af-label">Email</span><input name="email" type="email" autoComplete="username" required autoFocus /></label>
        <label className="af"><span className="af-label">Password</span><input name="password" type="password" autoComplete="current-password" required /></label>
        <button className="admin-btn" disabled={busy}>{busy ? 'Signing in…' : 'Sign in'}</button>
      </form>
    </div>
  );
}
