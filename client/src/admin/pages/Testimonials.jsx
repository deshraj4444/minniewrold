import { useState } from 'react';
import { api } from '../../lib/api.js';
import { useAsync } from '../../lib/hooks.js';
import { Field, ImageField, Notice } from '../components.jsx';

const empty = { customerName: '', location: '', quote: '', rating: 5, avatarUrl: '', isPublished: true };

export default function Testimonials() {
  const { data, reload } = useAsync(() => api.get('/admin/testimonials'), []);
  const [editing, setEditing] = useState(null);
  const [notice, setNotice] = useState(null);

  async function save(e) {
    e.preventDefault();
    try {
      const body = { ...editing, rating: Number(editing.rating) };
      if (editing.id) await api.put(`/admin/testimonials/${editing.id}`, body);
      else await api.post('/admin/testimonials', body);
      setEditing(null);
      setNotice({ type: 'ok', message: 'Saved.' });
      reload();
    } catch (err) {
      setNotice({ type: 'error', message: err.message });
    }
  }

  async function remove(t) {
    if (!confirm(`Delete testimonial from ${t.customerName}?`)) return;
    await api.del(`/admin/testimonials/${t.id}`);
    reload();
  }

  const bind = (key) => ({ value: editing[key] ?? '', onChange: (e) => setEditing((t) => ({ ...t, [key]: e.target.value })) });

  return (
    <>
      <div className="admin-head">
        <h1 className="admin-h1">Testimonials <span className="muted small">(AstroAura home page)</span></h1>
        {!editing && <button className="admin-btn" onClick={() => setEditing(empty)}>+ Add testimonial</button>}
      </div>
      <Notice notice={notice} />

      {editing && (
        <form className="admin-card admin-inline-form" onSubmit={save}>
          <div className="admin-grid-2">
            <Field label="Name"><input {...bind('customerName')} required maxLength={120} /></Field>
            <Field label="Location"><input {...bind('location')} maxLength={120} /></Field>
          </div>
          <Field label="Quote"><textarea {...bind('quote')} rows={3} required maxLength={1000} /></Field>
          <div className="admin-grid-2">
            <Field label="Rating"><select {...bind('rating')}>{[5, 4, 3, 2, 1].map((r) => <option key={r} value={r}>{'★'.repeat(r)}</option>)}</select></Field>
            <label className="af-check"><input type="checkbox" checked={editing.isPublished} onChange={(e) => setEditing((t) => ({ ...t, isPublished: e.target.checked }))} /> Published</label>
          </div>
          <ImageField label="Photo (optional)" value={editing.avatarUrl} onChange={(v) => setEditing((t) => ({ ...t, avatarUrl: v }))} />
          <div className="actions"><button className="admin-btn">Save</button><button type="button" className="admin-btn admin-btn-ghost" onClick={() => setEditing(null)}>Cancel</button></div>
        </form>
      )}

      <div className="admin-cards">
        {data?.map((t) => (
          <article key={t.id} className="admin-card">
            <p>“{t.quote}”</p>
            <p className="small"><b>{t.customerName}</b>{t.location && `, ${t.location}`} · {'★'.repeat(t.rating)} {!t.isPublished && <span className="pill pill-grey">Hidden</span>}</p>
            <footer className="actions">
              <button className="link" onClick={() => setEditing(t)}>Edit</button>
              <button className="link danger" onClick={() => remove(t)}>Delete</button>
            </footer>
          </article>
        ))}
      </div>
    </>
  );
}
