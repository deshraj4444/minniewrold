import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { api } from '../../lib/api.js';
import VideoEmbed from '../../shared/VideoEmbed.jsx';
import { Field, MarkdownField, Notice } from '../components.jsx';

const empty = { title: '', slug: '', summary: '', content: '', videoUrl: '', durationMinutes: 10, isPublished: true };

export default function LessonEditor() {
  const { id, courseId: courseIdParam } = useParams();
  const navigate = useNavigate();
  const [lesson, setLesson] = useState(empty);
  const [courseId, setCourseId] = useState(courseIdParam);
  const [errors, setErrors] = useState({});
  const [notice, setNotice] = useState(null);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    if (!id) return;
    api.get(`/admin/lessons/${id}`)
      .then((l) => { setLesson(l); setCourseId(String(l.courseId)); })
      .catch((e) => setNotice({ type: 'error', message: e.message }));
  }, [id]);

  const set = (key) => (value) => setLesson((l) => ({ ...l, [key]: value }));
  const bind = (key) => ({ value: lesson[key] ?? '', onChange: (e) => set(key)(e.target.value) });

  async function save(e) {
    e.preventDefault();
    setSaving(true);
    setErrors({});
    const body = { ...lesson, durationMinutes: Number(lesson.durationMinutes) || 0, sortOrder: id ? lesson.sortOrder : null };
    try {
      if (id) {
        await api.put(`/admin/lessons/${id}`, body);
        setNotice({ type: 'ok', message: 'Saved.' });
      } else {
        const res = await api.post(`/admin/courses/${courseId}/lessons`, body);
        navigate(`/admin/lessons/${res.id}`, { replace: true });
      }
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
        <h1 className="admin-h1"><Link to={`/admin/courses/${courseId}`} className="muted">Course</Link> / {id ? 'Edit lesson' : 'New lesson'}</h1>
        <button className="admin-btn" disabled={saving}>{saving ? 'Saving…' : 'Save lesson'}</button>
      </div>
      <Notice notice={notice} />

      <div className="admin-editor">
        <div className="admin-card">
          <Field label="Title" error={errors.title?.[0]}><input {...bind('title')} required maxLength={200} /></Field>
          <Field label="Summary" hint="One line shown in the syllabus and share previews."><input {...bind('summary')} maxLength={500} /></Field>
          <MarkdownField label="Lesson content" value={lesson.content} onChange={set('content')} rows={22} />
        </div>
        <div className="admin-card admin-editor-side">
          <label className="af-check"><input type="checkbox" checked={lesson.isPublished} onChange={(e) => set('isPublished')(e.target.checked)} /> Published</label>
          <Field label="Duration (minutes)" error={errors.durationMinutes?.[0]}><input type="number" min={0} max={1000} {...bind('durationMinutes')} /></Field>
          <Field label="Video URL" hint="Optional YouTube or Vimeo link, embedded above the text." error={errors.videoUrl?.[0]}><input {...bind('videoUrl')} maxLength={500} placeholder="https://youtu.be/…" /></Field>
          <VideoEmbed url={lesson.videoUrl} title="Preview" className="admin-video" />
          <Field label="URL slug" hint="Leave blank to generate from the title."><input {...bind('slug')} maxLength={220} /></Field>
        </div>
      </div>
    </form>
  );
}
