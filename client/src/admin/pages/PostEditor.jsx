import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams, useSearchParams } from 'react-router';
import { api } from '../../lib/api.js';
import { blogOptions, blogPath, Field, ImageField, MarkdownField, Notice } from '../components.jsx';

const empty = { blog: 'dharma', title: '', slug: '', excerpt: '', content: '', category: '', tags: '', coverImageUrl: '', author: '', isPublished: true };

export default function PostEditor() {
  const { id } = useParams();
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const [post, setPost] = useState(() => ({ ...empty, blog: params.get('blog') ?? empty.blog }));
  const [errors, setErrors] = useState({});
  const [notice, setNotice] = useState(null);
  const [saving, setSaving] = useState(false);
  const [categories, setCategories] = useState([]);

  useEffect(() => {
    if (id) api.get(`/admin/posts/${id}`).then(setPost).catch((e) => setNotice({ type: 'error', message: e.message }));
  }, [id]);

  useEffect(() => {
    api.blog(post.blog).categories().then((c) => setCategories(c.posts)).catch(() => setCategories([]));
  }, [post.blog]);

  const set = (key) => (value) => setPost((p) => ({ ...p, [key]: value }));
  const bind = (key) => ({ value: post[key] ?? '', onChange: (e) => set(key)(e.target.value) });

  async function save(e) {
    e.preventDefault();
    setSaving(true);
    setErrors({});
    try {
      const res = id ? await api.put(`/admin/posts/${id}`, post) : await api.post('/admin/posts', post);
      setNotice({ type: 'ok', message: 'Saved.' });
      setPost((p) => ({ ...p, slug: res.slug }));
      if (!id) navigate(`/admin/posts/${res.id}`, { replace: true });
    } catch (err) {
      setErrors(err.errors ?? {});
      setNotice({ type: 'error', message: err.message });
    } finally {
      setSaving(false);
    }
  }

  return (
    <form onSubmit={save}>
      <div className="admin-head">
        <h1 className="admin-h1"><Link to="/admin/posts" className="muted">Posts</Link> / {id ? 'Edit' : 'New'} post</h1>
        <div className="admin-head-actions">
          {id && post.isPublished && <a className="admin-btn admin-btn-ghost" href={`${blogPath(post.blog)}/posts/${post.slug}`} target="_blank" rel="noreferrer">View ↗</a>}
          <button className="admin-btn" disabled={saving}>{saving ? 'Saving…' : 'Save'}</button>
        </div>
      </div>
      <Notice notice={notice} />

      <div className="admin-editor">
        <div className="admin-card">
          <Field label="Title" error={errors.title?.[0]}><input {...bind('title')} required maxLength={200} /></Field>
          <Field label="Excerpt" hint="One or two sentences. Shown on cards and in social share previews." error={errors.excerpt?.[0]}>
            <textarea {...bind('excerpt')} rows={2} maxLength={500} />
          </Field>
          <MarkdownField label="Content" value={post.content} onChange={set('content')} />
        </div>

        <div className="admin-card admin-editor-side">
          <Field label="Blog" error={errors.blog?.[0]}>
            <select {...bind('blog')}>{blogOptions.map((b) => <option key={b.key} value={b.key}>{b.label}</option>)}</select>
          </Field>
          <label className="af-check"><input type="checkbox" checked={post.isPublished} onChange={(e) => set('isPublished')(e.target.checked)} /> Published</label>
          <Field label="Category" error={errors.category?.[0]}>
            <input {...bind('category')} list="post-categories" required maxLength={80} />
            <datalist id="post-categories">{categories.map((c) => <option key={c} value={c} />)}</datalist>
          </Field>
          <Field label="Tags" hint="Comma separated"><input {...bind('tags')} maxLength={300} /></Field>
          <Field label="Author"><input {...bind('author')} maxLength={120} /></Field>
          <Field label="URL slug" hint="Leave blank to generate from the title." error={errors.slug?.[0]}><input {...bind('slug')} maxLength={220} /></Field>
          <ImageField label="Cover image" value={post.coverImageUrl} onChange={set('coverImageUrl')} />
        </div>
      </div>
    </form>
  );
}
