import { useState } from 'react';
import { api } from '../../lib/api.js';
import { useAsync } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';

const sites = [['', 'All'], ['astro', 'AstroAura'], ['dharma', 'Dharma Path'], ['gk', 'Curious Minds'], ['hub', 'Hub']];

export default function Messages() {
  const [site, setSite] = useState('');
  const { data, loading, reload } = useAsync(() => api.get(`/admin/messages${site ? `?site=${site}` : ''}`), [site]);

  async function markRead(m) {
    await api.put(`/admin/messages/${m.id}/read`);
    reload();
  }

  async function remove(m) {
    if (!confirm('Delete this message?')) return;
    await api.del(`/admin/messages/${m.id}`);
    reload();
  }

  return (
    <>
      <h1 className="admin-h1">Messages</h1>
      <div className="admin-filter">
        {sites.map(([key, label]) => <button key={key} className={site === key ? 'active' : ''} onClick={() => setSite(key)}>{label}</button>)}
      </div>
      {loading && <p className="muted">Loading…</p>}
      {data?.length === 0 && <p className="muted">No messages.</p>}
      <div className="admin-cards">
        {data?.map((m) => (
          <article key={m.id} className={m.isRead ? 'admin-card admin-message' : 'admin-card admin-message unread'}>
            <header>
              <div>
                <b>{m.fullName}</b> <a href={`mailto:${m.email}?subject=${encodeURIComponent(`Re: ${m.subject}`)}`}>{m.email}</a>
                <div className="muted small">{sites.find(([k]) => k === m.siteKey)?.[1] ?? m.siteKey} · {formatDate(m.createdAt)}{m.phone && ` · ${m.phone}`}</div>
              </div>
              {!m.isRead && <span className="pill pill-blue">New</span>}
            </header>
            {m.subject && <p className="strong">{m.subject}</p>}
            <p className="admin-note">{m.message}</p>
            <footer className="actions">
              {!m.isRead && <button className="link" onClick={() => markRead(m)}>Mark as read</button>}
              <button className="link danger" onClick={() => remove(m)}>Delete</button>
            </footer>
          </article>
        ))}
      </div>
    </>
  );
}
