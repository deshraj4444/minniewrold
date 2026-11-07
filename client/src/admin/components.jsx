import { useState } from 'react';
import { api } from '../lib/api.js';
import Markdown from '../shared/Markdown.jsx';

export const blogOptions = [
  { key: 'dharma', label: 'Dharma Path (religious)', path: '/dharma' },
  { key: 'gk', label: 'Curious Minds (GK)', path: '/gk' }
];

export const blogLabel = (key) => blogOptions.find((b) => b.key === key)?.label ?? key;
export const blogPath = (key) => blogOptions.find((b) => b.key === key)?.path ?? '/';

export function Field({ label, hint, error, children }) {
  return (
    <label className={error ? 'af has-error' : 'af'}>
      <span className="af-label">{label}</span>
      {children}
      {hint && !error && <small className="af-hint">{hint}</small>}
      {error && <small className="af-error">{error}</small>}
    </label>
  );
}

/** Image URL with an upload button and preview. */
export function ImageField({ label, value, onChange }) {
  const [uploading, setUploading] = useState(false);
  const [error, setError] = useState('');

  async function upload(e) {
    const file = e.target.files?.[0];
    e.target.value = '';
    if (!file) return;
    setUploading(true);
    setError('');
    try {
      const { url } = await api.upload(file);
      onChange(url);
    } catch (err) {
      setError(err.message);
    } finally {
      setUploading(false);
    }
  }

  return (
    <div className="af">
      <span className="af-label">{label}</span>
      <div className="af-image">
        {value ? <img src={value} alt="" /> : <div className="af-image-empty">No image</div>}
        <div className="af-image-controls">
          <input value={value} onChange={(e) => onChange(e.target.value)} placeholder="https://… or upload" />
          <label className="admin-btn admin-btn-ghost">
            {uploading ? 'Uploading…' : 'Upload'}
            <input type="file" accept="image/jpeg,image/png,image/webp,image/gif" onChange={upload} hidden disabled={uploading} />
          </label>
          {value && <button type="button" className="admin-btn admin-btn-ghost" onClick={() => onChange('')}>Remove</button>}
        </div>
      </div>
      {error && <small className="af-error">{error}</small>}
    </div>
  );
}

/** Markdown textarea with Write / Preview tabs and a short syntax cheat sheet. */
export function MarkdownField({ label, value, onChange, rows = 18 }) {
  const [preview, setPreview] = useState(false);
  return (
    <div className="af">
      <div className="af-md-head">
        <span className="af-label">{label}</span>
        <div className="af-md-tabs" role="tablist">
          <button type="button" role="tab" aria-selected={!preview} onClick={() => setPreview(false)}>Write</button>
          <button type="button" role="tab" aria-selected={preview} onClick={() => setPreview(true)}>Preview</button>
        </div>
      </div>
      {preview
        ? <div className="af-md-preview"><Markdown>{value || '_Nothing to preview yet._'}</Markdown></div>
        : <textarea value={value} onChange={(e) => onChange(e.target.value)} rows={rows} spellCheck />}
      <small className="af-hint">Markdown: <code>## Heading</code> <code>**bold**</code> <code>*italic*</code> <code>- list</code> <code>&gt; quote</code> <code>[link](https://…)</code> <code>![image](url)</code> and tables with <code>| a | b |</code>.</small>
    </div>
  );
}

export function StatusBadge({ published }) {
  return <span className={published ? 'pill pill-green' : 'pill pill-grey'}>{published ? 'Published' : 'Draft'}</span>;
}

export function Notice({ notice }) {
  if (!notice?.message) return null;
  return <p className={notice.type === 'error' ? 'admin-notice error' : 'admin-notice'} role="status">{notice.message}</p>;
}
