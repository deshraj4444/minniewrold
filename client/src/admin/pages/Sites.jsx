import { useState } from 'react';
import { api } from '../../lib/api.js';
import { useAsync } from '../../lib/hooks.js';
import { Field, ImageField, Notice } from '../components.jsx';

const empty = { name: '', tagline: '', path: '/', thumbnailUrl: '', accentColor: '#4f46e5', sortOrder: 10, isActive: true };

export default function Sites() {
  const { data, reload } = useAsync(() => api.get('/admin/sites'), []);
  const [editing, setEditing] = useState(null);
  const [notice, setNotice] = useState(null);

  async function save(e) {
    e.preventDefault();
    try {
      const body = { ...editing, sortOrder: Number(editing.sortOrder) || 0 };
      if (editing.id) await api.put(`/admin/sites/${editing.id}`, body);
      else await api.post('/admin/sites', body);
      setEditing(null);
      setNotice({ type: 'ok', message: 'Saved.' });
      reload();
    } catch (err) {
      setNotice({ type: 'error', message: err.message });
    }
  }

  async function remove(s) {
    if (!confirm(`Remove the "${s.name}" tile from the home page?`)) return;
    await api.del(`/admin/sites/${s.id}`);
    reload();
  }

  const bind = (key) => ({ value: editing[key] ?? '', onChange: (e) => setEditing((s) => ({ ...s, [key]: e.target.value })) });

  return (
    <>
      <div className="admin-head">
        <h1 className="admin-h1">Hub tiles</h1>
        {!editing && <button className="admin-btn" onClick={() => setEditing(empty)}>+ Add tile</button>}
      </div>
      <p className="muted">The thumbnails on the MinnieWorld home page. Each tile links to a section of this site (like <code>/astro</code>) or an external website.</p>
      <Notice notice={notice} />

      {editing && (
        <form className="admin-card admin-inline-form" onSubmit={save}>
          <div className="admin-grid-2">
            <Field label="Name"><input {...bind('name')} required maxLength={100} /></Field>
            <Field label="Link" hint="/astro, /dharma, /gk, or https://…"><input {...bind('path')} required maxLength={300} /></Field>
          </div>
          <Field label="Tagline"><input {...bind('tagline')} maxLength={300} /></Field>
          <div className="admin-grid-2">
            <Field label="Accent colour"><input type="color" {...bind('accentColor')} /></Field>
            <Field label="Order" hint="Lower numbers come first"><input type="number" {...bind('sortOrder')} /></Field>
          </div>
          <label className="af-check"><input type="checkbox" checked={editing.isActive} onChange={(e) => setEditing((s) => ({ ...s, isActive: e.target.checked }))} /> Show on home page</label>
          <ImageField label="Thumbnail" value={editing.thumbnailUrl} onChange={(v) => setEditing((s) => ({ ...s, thumbnailUrl: v }))} />
          <div className="actions"><button className="admin-btn">Save</button><button type="button" className="admin-btn admin-btn-ghost" onClick={() => setEditing(null)}>Cancel</button></div>
        </form>
      )}

      <div className="admin-site-grid">
        {data?.map((s) => (
          <article key={s.id} className="admin-card admin-site" style={{ borderTopColor: s.accentColor }}>
            {s.thumbnailUrl && <img src={s.thumbnailUrl} alt="" />}
            <h3>{s.name} {!s.isActive && <span className="pill pill-grey">Hidden</span>}</h3>
            <p className="muted small">{s.path} · order {s.sortOrder}</p>
            <p className="small">{s.tagline}</p>
            <footer className="actions">
              <button className="link" onClick={() => setEditing(s)}>Edit</button>
              <button className="link danger" onClick={() => remove(s)}>Delete</button>
            </footer>
          </article>
        ))}
      </div>
    </>
  );
}
