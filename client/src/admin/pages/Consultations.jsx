import { useState } from 'react';
import { api } from '../../lib/api.js';
import { useAsync } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';

const statuses = ['New', 'In progress', 'Completed', 'Cancelled'];

export default function Consultations() {
  const [filter, setFilter] = useState('');
  const { data, loading, reload } = useAsync(() => api.get(`/admin/consultations${filter ? `?status=${encodeURIComponent(filter)}` : ''}`), [filter]);
  const [open, setOpen] = useState(null);

  async function setStatus(id, status) {
    await api.put(`/admin/consultations/${id}/status`, { status });
    reload();
  }

  async function remove(c) {
    if (!confirm(`Delete the request from ${c.fullName}?`)) return;
    await api.del(`/admin/consultations/${c.id}`);
    reload();
  }

  return (
    <>
      <h1 className="admin-h1">Consultation requests</h1>
      <div className="admin-filter">
        <button className={!filter ? 'active' : ''} onClick={() => setFilter('')}>All</button>
        {statuses.map((s) => <button key={s} className={filter === s ? 'active' : ''} onClick={() => setFilter(s)}>{s}</button>)}
      </div>
      {loading && <p className="muted">Loading…</p>}
      {data?.length === 0 && <p className="muted">No requests.</p>}
      <div className="admin-cards">
        {data?.map((c) => (
          <article key={c.id} className="admin-card admin-request">
            <header>
              <div>
                <b>{c.fullName}</b> <span className="muted">· {c.serviceType}</span>
                <div className="muted small"><a href={`mailto:${c.email}`}>{c.email}</a> · <a href={`tel:${c.phone}`}>{c.phone}</a> · received {formatDate(c.createdAt)}</div>
              </div>
              <select value={c.status} onChange={(e) => setStatus(c.id, e.target.value)} aria-label="Status" className={`status-${c.status.replace(' ', '-').toLowerCase()}`}>
                {statuses.map((s) => <option key={s}>{s}</option>)}
              </select>
            </header>
            <p className="small">Born {formatDate(c.birthDate)} at {c.birthTime.slice(0, 5)}, {c.birthPlace}</p>
            {c.notes && (open === c.id ? <p className="admin-note">{c.notes}</p> : <button className="link" onClick={() => setOpen(c.id)}>Show question</button>)}
            <footer className="actions">
              <a href={`/astro/kundali?${new URLSearchParams({ n: c.fullName, d: c.birthDate, t: c.birthTime.slice(0, 5) })}`} target="_blank" rel="noreferrer" title="Opens the Kundali tool; pick the birth place there">Open in Kundali tool ↗</a>
              <button className="link danger" onClick={() => remove(c)}>Delete</button>
            </footer>
          </article>
        ))}
      </div>
    </>
  );
}
