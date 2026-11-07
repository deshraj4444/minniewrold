import { useCallback, useEffect, useState } from 'react';
import { Link, useNavigate, useParams, useSearchParams } from 'react-router';
import { api } from '../../lib/api.js';
import { blogOptions, blogPath, Field, ImageField, MarkdownField, Notice, StatusBadge } from '../components.jsx';

const empty = { blog: 'dharma', title: '', slug: '', subtitle: '', description: '', category: '', level: 'Beginner', thumbnailUrl: '', isFeatured: false, isPublished: true, lessons: [] };

export default function CourseEditor() {
  const { id } = useParams();
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const [course, setCourse] = useState(() => ({ ...empty, blog: params.get('blog') ?? empty.blog }));
  const [errors, setErrors] = useState({});
  const [notice, setNotice] = useState(null);
  const [saving, setSaving] = useState(false);

  const load = useCallback(() => {
    if (id) api.get(`/admin/courses/${id}`).then(setCourse).catch((e) => setNotice({ type: 'error', message: e.message }));
  }, [id]);
  useEffect(load, [load]);

  const set = (key) => (value) => setCourse((c) => ({ ...c, [key]: value }));
  const bind = (key) => ({ value: course[key] ?? '', onChange: (e) => set(key)(e.target.value) });

  async function save(e) {
    e.preventDefault();
    setSaving(true);
    setErrors({});
    try {
      const { lessons, ...body } = course;
      const res = id ? await api.put(`/admin/courses/${id}`, body) : await api.post('/admin/courses', body);
      setNotice({ type: 'ok', message: id ? 'Saved.' : 'Course created. Now add lessons below.' });
      setCourse((c) => ({ ...c, slug: res.slug }));
      if (!id) navigate(`/admin/courses/${res.id}`, { replace: true });
    } catch (err) {
      setErrors(err.errors ?? {});
      setNotice({ type: 'error', message: err.message });
    } finally {
      setSaving(false);
    }
  }

  async function move(index, delta) {
    const lessons = [...course.lessons];
    const [item] = lessons.splice(index, 1);
    lessons.splice(index + delta, 0, item);
    setCourse((c) => ({ ...c, lessons }));
    await api.put(`/admin/courses/${id}/lessons/order`, lessons.map((l) => l.id));
  }

  async function removeLesson(l) {
    if (!confirm(`Delete lesson "${l.title}"?`)) return;
    await api.del(`/admin/lessons/${l.id}`);
    load();
  }

  return (
    <>
      <form onSubmit={save}>
        <div className="admin-head">
          <h1 className="admin-h1"><Link to="/admin/courses" className="muted">Courses</Link> / {id ? course.title || 'Edit' : 'New course'}</h1>
          <div className="admin-head-actions">
            {id && course.isPublished && <a className="admin-btn admin-btn-ghost" href={`${blogPath(course.blog)}/courses/${course.slug}`} target="_blank" rel="noreferrer">View ↗</a>}
            <button className="admin-btn" disabled={saving}>{saving ? 'Saving…' : 'Save course'}</button>
          </div>
        </div>
        <Notice notice={notice} />

        <div className="admin-editor">
          <div className="admin-card">
            <Field label="Title" error={errors.title?.[0]}><input {...bind('title')} required maxLength={200} /></Field>
            <Field label="Subtitle" hint="Shown on cards and in social share previews." error={errors.subtitle?.[0]}><input {...bind('subtitle')} maxLength={400} /></Field>
            <MarkdownField label="Description" value={course.description} onChange={set('description')} rows={10} />
          </div>
          <div className="admin-card admin-editor-side">
            <Field label="Blog" error={errors.blog?.[0]}>
              <select {...bind('blog')}>{blogOptions.map((b) => <option key={b.key} value={b.key}>{b.label}</option>)}</select>
            </Field>
            <label className="af-check"><input type="checkbox" checked={course.isPublished} onChange={(e) => set('isPublished')(e.target.checked)} /> Published</label>
            <label className="af-check"><input type="checkbox" checked={course.isFeatured} onChange={(e) => set('isFeatured')(e.target.checked)} /> Featured</label>
            <Field label="Category" error={errors.category?.[0]}><input {...bind('category')} required maxLength={80} /></Field>
            <Field label="Level">
              <select {...bind('level')}><option>Beginner</option><option>Intermediate</option><option>Advanced</option><option>All levels</option></select>
            </Field>
            <Field label="URL slug" hint="Leave blank to generate from the title."><input {...bind('slug')} maxLength={220} /></Field>
            <ImageField label="Thumbnail" value={course.thumbnailUrl} onChange={set('thumbnailUrl')} />
          </div>
        </div>
      </form>

      {id && (
        <section className="admin-card admin-lessons">
          <div className="admin-head">
            <h2>Lessons ({course.lessons.length})</h2>
            <Link to={`/admin/courses/${id}/lessons/new`} className="admin-btn">+ Add lesson</Link>
          </div>
          {course.lessons.length === 0 && <p className="muted">No lessons yet.</p>}
          <ol className="admin-lesson-list">
            {course.lessons.map((l, i) => (
              <li key={l.id}>
                <span className="admin-lesson-num">{i + 1}</span>
                <Link to={`/admin/lessons/${l.id}`} className="strong">{l.title}</Link>
                <span className="muted">{l.durationMinutes} min</span>
                <StatusBadge published={l.isPublished} />
                <span className="actions">
                  <button type="button" className="link" disabled={i === 0} onClick={() => move(i, -1)} aria-label="Move up">↑</button>
                  <button type="button" className="link" disabled={i === course.lessons.length - 1} onClick={() => move(i, 1)} aria-label="Move down">↓</button>
                  <Link to={`/admin/lessons/${l.id}`}>Edit</Link>
                  <button type="button" className="link danger" onClick={() => removeLesson(l)}>Delete</button>
                </span>
              </li>
            ))}
          </ol>
        </section>
      )}
    </>
  );
}
