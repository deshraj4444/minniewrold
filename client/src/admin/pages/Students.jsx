import { useState } from 'react';
import { Link } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';

const filters = [['all', 'All'], ['pass', 'All-Access'], ['disabled', 'Disabled']];

export default function Students() {
  const { data, loading } = useAsync(() => api.get('/admin/students'), []);
  const [q, setQ] = useState('');
  const [filter, setFilter] = useState('all');
  const term = q.trim().toLowerCase();
  const now = new Date().toISOString();
  const passActive = (s) => s.hasAllAccess && (!s.allAccessUntil || s.allAccessUntil > now);

  const rows = (data ?? [])
    .filter((s) => filter === 'all' || (filter === 'pass' ? passActive(s) : s.isDisabled))
    .filter((s) => !term || s.fullName.toLowerCase().includes(term) || s.email.includes(term) || s.phone.includes(term));

  return (
    <>
      <div className="admin-head">
        <h1 className="admin-h1">Students <span className="muted small">({data?.length ?? 0} registered)</span></h1>
        <div className="admin-head-actions">
          <input className="admin-search" placeholder="Search name, email, phone" value={q} onChange={(e) => setQ(e.target.value)} />
          <Link to="/admin/students/new" className="admin-btn">+ Add student</Link>
        </div>
      </div>
      <div className="admin-filter">
        {filters.map(([k, label]) => <button key={k} className={filter === k ? 'active' : ''} onClick={() => setFilter(k)}>{label}</button>)}
      </div>
      {loading && <p className="muted">Loading…</p>}
      {data?.length === 0 && <p className="muted">No students yet. Students can register on the site, or you can add them here.</p>}
      {rows.length > 0 && (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead><tr><th>Name</th><th>Email</th><th>Phone</th><th>Access</th><th>Tests taken</th><th>Best</th><th>Joined</th><th>Last login</th></tr></thead>
            <tbody>
              {rows.map((s) => (
                <tr key={s.id}>
                  <td><Link to={`/admin/students/${s.id}`} className="strong">{s.fullName}</Link></td>
                  <td>{s.email}</td>
                  <td>{s.phone || '—'}</td>
                  <td>
                    {s.isDisabled && <span className="pill pill-grey">Disabled</span>}
                    {passActive(s) && <span className="pill pill-green">All-Access{s.allAccessUntil ? ` to ${formatDate(s.allAccessUntil)}` : ''}</span>}
                    {!s.isDisabled && !passActive(s) && (s.unlockedTests ? `${s.unlockedTests} paid test${s.unlockedTests > 1 ? 's' : ''}` : 'Free tests')}
                  </td>
                  <td>{s.attempts}</td>
                  <td>{s.bestPercent != null ? `${Math.round(s.bestPercent)}%` : '—'}</td>
                  <td>{formatDate(s.createdAt)}</td>
                  <td>{s.lastLoginAt ? formatDate(s.lastLoginAt) : '—'}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
