import { useEffect, useState } from 'react';
import { Link, useNavigate, useParams } from 'react-router';
import { api } from '../../lib/api.js';
import { Field, Notice } from '../components.jsx';
import { testHref } from '../../gk/jee.js';

const kinds = ['Mock test', 'Previous year', 'Sectional', 'Sample test', 'Year pattern', 'Expected paper'];
const letters = ['A', 'B', 'C', 'D'];
const emptyExam = { title: '', slug: '', description: '', category: 'HP TET TGT Arts', kind: 'Mock test', year: '', durationMinutes: 150, negativeMark: 0, price: 0, isPublished: true };
const emptyQuestion = { section: '', text: '', options: ['', '', '', ''], answer: 0, explanation: '', textHi: '', optionsHi: ['', '', '', ''], explanationHi: '', marks: 1, figure: '', passage: '', passageHi: '' };

const importExample = `[
  {
    "section": "General Awareness",
    "question": "Which river is known as Chandrabhaga in Himachal Pradesh?",
    "options": ["Beas", "Chenab", "Ravi", "Sutlej"],
    "answer": "B",
    "explanation": "The Chandra and Bhaga meet at Tandi to form the Chenab."
  }
]`;

export default function ExamEditor() {
  const { id } = useParams();
  const isNew = !id;
  const navigate = useNavigate();
  const [exam, setExam] = useState(isNew ? emptyExam : null);
  const [questions, setQuestions] = useState([]);
  const [notice, setNotice] = useState(null);
  const [saving, setSaving] = useState(false);
  const [editing, setEditing] = useState(null);
  const [importing, setImporting] = useState(null);
  const [filter, setFilter] = useState('');

  async function load() {
    const data = await api.get(`/admin/exams/${id}`);
    setExam({ ...data.exam, year: data.exam.year ?? '' });
    setQuestions(data.questions);
  }

  useEffect(() => {
    if (!isNew) load().catch((e) => setNotice({ type: 'error', message: e.message }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  if (!exam) return <p className="muted">Loading…</p>;

  const bind = (key) => ({ value: exam[key] ?? '', onChange: (e) => setExam((x) => ({ ...x, [key]: e.target.value })) });

  async function save(e) {
    e.preventDefault();
    setSaving(true);
    try {
      const body = { ...exam, year: exam.year === '' ? null : Number(exam.year), durationMinutes: Number(exam.durationMinutes), negativeMark: Number(exam.negativeMark), price: Number(exam.price) };
      if (isNew) {
        const created = await api.post('/admin/exams', body);
        navigate(`/admin/exams/${created.id}`, { replace: true });
        return;
      }
      const saved = await api.put(`/admin/exams/${id}`, body);
      setExam({ ...saved, year: saved.year ?? '' });
      setNotice({ type: 'ok', message: 'Test saved.' });
    } catch (err) {
      setNotice({ type: 'error', message: err.message });
    } finally {
      setSaving(false);
    }
  }

  async function saveQuestion(e) {
    e.preventDefault();
    try {
      const body = { ...editing, answer: Number(editing.answer) };
      if (editing.id) await api.put(`/admin/questions/${editing.id}`, body);
      else await api.post(`/admin/exams/${id}/questions`, body);
      setEditing(null);
      setNotice({ type: 'ok', message: 'Question saved.' });
      await load();
    } catch (err) {
      setNotice({ type: 'error', message: err.message });
    }
  }

  async function removeQuestion(q) {
    if (!confirm(`Delete question ${q.sortOrder}?`)) return;
    await api.del(`/admin/questions/${q.id}`);
    await load();
  }

  async function runImport(e) {
    e.preventDefault();
    try {
      const res = await api.post(`/admin/exams/${id}/import`, importing);
      setImporting(null);
      setNotice({
        type: res.skipped ? 'error' : 'ok',
        message: `Imported ${res.imported} questions.${res.skipped ? ` Skipped ${res.skipped}: ${res.errors.join(' · ')}` : ''}`
      });
      await load();
    } catch (err) {
      setNotice({ type: 'error', message: [err.message, ...(err.errors?.json ?? [])].join(' · ') });
    }
  }

  async function readFile(e) {
    const file = e.target.files?.[0];
    if (!file) return;
    const text = await file.text();
    setImporting((i) => ({ ...i, json: text }));
  }

  const sections = [...new Set(questions.map((q) => q.section).filter(Boolean))];
  const shown = filter ? questions.filter((q) => q.section === filter) : questions;

  return (
    <>
      <div className="admin-head">
        <h1 className="admin-h1"><Link to="/admin/exams">Mock tests</Link> › {isNew ? 'New test' : exam.title}</h1>
        {!isNew && <a className="admin-btn admin-btn-ghost" href={testHref(exam.slug)} target="_blank" rel="noreferrer">View on site ↗</a>}
      </div>
      <Notice notice={notice} />

      <form className="admin-card admin-inline-form" onSubmit={save}>
        <div className="admin-grid-2">
          <Field label="Title"><input {...bind('title')} required maxLength={200} /></Field>
          <Field label="Exam / category" hint="Tests are grouped by this on the site, e.g. HP TET TGT Arts"><input {...bind('category')} required maxLength={100} list="exam-categories" /></Field>
        </div>
        <datalist id="exam-categories"><option value="HP TET TGT Arts" /><option value="HP TET JBT" /><option value="General Knowledge" /></datalist>
        <Field label="Description"><textarea {...bind('description')} rows={2} maxLength={2000} /></Field>
        <div className="admin-grid-2">
          <Field label="Type"><select {...bind('kind')}>{kinds.map((k) => <option key={k}>{k}</option>)}</select></Field>
          <Field label="Year (for previous-year papers)"><input {...bind('year')} type="number" min={1990} max={2100} /></Field>
        </div>
        <div className="admin-grid-2">
          <Field label="Time (minutes)"><input {...bind('durationMinutes')} type="number" min={1} max={600} required /></Field>
          <Field label="Negative marks per wrong answer" hint="0 for HP TET"><input {...bind('negativeMark')} type="number" min={0} max={5} step={0.25} /></Field>
        </div>
        <Field label="Price (₹)" hint="0 = free. Students pay by UPI and you approve the payment in admin → Payments."><input {...bind('price')} type="number" min={0} max={10000} /></Field>
        <Field label="URL slug" hint="Leave blank to create it from the title."><input {...bind('slug')} maxLength={220} /></Field>
        <label className="af-check"><input type="checkbox" checked={exam.isPublished} onChange={(e) => setExam((x) => ({ ...x, isPublished: e.target.checked }))} /> Published (visible to students)</label>
        <div className="actions"><button className="admin-btn" disabled={saving}>{saving ? 'Saving…' : isNew ? 'Create test' : 'Save test'}</button></div>
      </form>

      {!isNew && (
        <>
          <div className="admin-head">
            <h2>Questions ({questions.length})</h2>
            <div className="admin-head-actions">
              <button className="admin-btn admin-btn-ghost" onClick={() => setImporting({ json: '', replace: false })}>Import JSON</button>
              <button className="admin-btn" onClick={() => setEditing({ ...emptyQuestion, section: filter || sections[sections.length - 1] || '' })}>+ Add question</button>
            </div>
          </div>

          {importing && (
            <form className="admin-card admin-inline-form" onSubmit={runImport}>
              <h3>Import questions</h3>
              <p className="muted small">
                Paste a JSON array of questions (or an object with a <code>questions</code> array), or choose a .json file. Each question needs the text
                (<code>question</code> or <code>text</code>), four <code>options</code>, and the <code>answer</code> as a letter A–D, the option text, or a number 0–3.
                Optional: <code>section</code>, <code>explanation</code>, and the Hindi version <code>textHi</code>, <code>optionsHi</code> (4), <code>explanationHi</code>.
              </p>
              <input type="file" accept=".json,application/json" onChange={readFile} />
              <Field label="JSON"><textarea value={importing.json} onChange={(e) => setImporting((i) => ({ ...i, json: e.target.value }))} rows={12} placeholder={importExample} required spellCheck={false} className="mono" /></Field>
              <label className="af-check"><input type="checkbox" checked={importing.replace} onChange={(e) => setImporting((i) => ({ ...i, replace: e.target.checked }))} /> Replace all existing questions in this test</label>
              <div className="actions"><button className="admin-btn">Import</button><button type="button" className="admin-btn admin-btn-ghost" onClick={() => setImporting(null)}>Cancel</button></div>
            </form>
          )}

          {editing && (
            <form id="question-form" className="admin-card admin-inline-form" onSubmit={saveQuestion}>
              <h3>{editing.id ? `Edit question ${editing.sortOrder}` : 'New question'}</h3>
              <Field label="Section"><input value={editing.section} onChange={(e) => setEditing((q) => ({ ...q, section: e.target.value }))} maxLength={100} list="exam-sections" /></Field>
              <datalist id="exam-sections">{sections.map((s) => <option key={s} value={s} />)}</datalist>
              <Field label="Question"><textarea value={editing.text} onChange={(e) => setEditing((q) => ({ ...q, text: e.target.value }))} rows={3} required maxLength={8000} /></Field>
              <div className="admin-grid-2">
                {editing.options.map((o, i) => (
                  <Field key={i} label={`Option ${letters[i]}`}>
                    <input value={o} required maxLength={1000} onChange={(e) => setEditing((q) => ({ ...q, options: q.options.map((x, j) => (j === i ? e.target.value : x)) }))} />
                  </Field>
                ))}
              </div>
              <Field label="Correct answer">
                <select value={editing.answer} onChange={(e) => setEditing((q) => ({ ...q, answer: Number(e.target.value) }))}>
                  {letters.map((l, i) => <option key={l} value={i}>{l}: {editing.options[i] || '…'}</option>)}
                </select>
              </Field>
              <Field label="Explanation (shown after the test)"><textarea value={editing.explanation} onChange={(e) => setEditing((q) => ({ ...q, explanation: e.target.value }))} rows={2} maxLength={2000} /></Field>
              <details open={!!editing.textHi}>
                <summary>Hindi version (हिंदी) — shown under the English. Leave empty for English/Hindi language sections.</summary>
                <Field label="Question (Hindi)"><textarea lang="hi" value={editing.textHi ?? ''} onChange={(e) => setEditing((q) => ({ ...q, textHi: e.target.value }))} rows={3} maxLength={8000} /></Field>
                <div className="admin-grid-2">
                  {[0, 1, 2, 3].map((i) => (
                    <Field key={i} label={`Option ${letters[i]} (Hindi)`}>
                      <input lang="hi" value={editing.optionsHi?.[i] ?? ''} maxLength={1000} onChange={(e) => setEditing((q) => ({ ...q, optionsHi: [0, 1, 2, 3].map((j) => (j === i ? e.target.value : q.optionsHi?.[j] ?? '')) }))} />
                    </Field>
                  ))}
                </div>
                <Field label="Explanation (Hindi)"><textarea lang="hi" value={editing.explanationHi ?? ''} onChange={(e) => setEditing((q) => ({ ...q, explanationHi: e.target.value }))} rows={2} maxLength={2000} /></Field>
              </details>
              <details open={!!(editing.figure || editing.passage)}>
                <summary>Marks, picture and reading passage (optional)</summary>
                <Field label="Marks for a right answer"><input type="number" min="0.25" max="10" step="0.25" value={editing.marks ?? 1} onChange={(e) => setEditing((q) => ({ ...q, marks: Number(e.target.value) }))} /></Field>
                <Field label="Picture (SVG code: <svg xmlns=… viewBox=…>…</svg>)"><textarea value={editing.figure ?? ''} onChange={(e) => setEditing((q) => ({ ...q, figure: e.target.value }))} rows={4} maxLength={20000} spellCheck={false} /></Field>
                {editing.figure?.trim().startsWith('<svg') && <img className="admin-figure" alt="Picture preview" src={`data:image/svg+xml;charset=utf-8,${encodeURIComponent(editing.figure)}`} />}
                <Field label="Reading passage (repeat it on each question about it)"><textarea value={editing.passage ?? ''} onChange={(e) => setEditing((q) => ({ ...q, passage: e.target.value }))} rows={4} maxLength={8000} /></Field>
                <Field label="Reading passage (Hindi)"><textarea lang="hi" value={editing.passageHi ?? ''} onChange={(e) => setEditing((q) => ({ ...q, passageHi: e.target.value }))} rows={3} maxLength={8000} /></Field>
              </details>
              <div className="actions"><button className="admin-btn">Save question</button><button type="button" className="admin-btn admin-btn-ghost" onClick={() => setEditing(null)}>Cancel</button></div>
            </form>
          )}

          {sections.length > 1 && (
            <div className="admin-filter">
              <button className={!filter ? 'active' : ''} onClick={() => setFilter('')}>All</button>
              {sections.map((s) => <button key={s} className={filter === s ? 'active' : ''} onClick={() => setFilter(s)}>{s} ({questions.filter((q) => q.section === s).length})</button>)}
            </div>
          )}

          {questions.length === 0 && <p className="muted">No questions yet. Add them one by one or import a JSON file.</p>}
          <ol className="admin-questions">
            {shown.map((q) => (
              <li key={q.id} className="admin-card">
                <div className="muted small">Q{q.sortOrder} · {q.section || 'No section'}</div>
                <p className="strong pre">{q.text}</p>
                {q.textHi && <p className="muted pre" lang="hi">{q.textHi}</p>}
                {q.figure && <img className="admin-figure" alt="" src={`data:image/svg+xml;charset=utf-8,${encodeURIComponent(q.figure)}`} />}
                <ol type="A" className="admin-q-options">
                  {q.options.map((o, i) => <li key={i} className={i === q.answer ? 'correct' : ''}>{o}{i === q.answer && ' ✓'}</li>)}
                </ol>
                {q.explanation && <p className="muted small">{q.explanation}</p>}
                <footer className="actions">
                  <button className="link" onClick={() => { setEditing({ ...q }); setTimeout(() => document.getElementById('question-form')?.scrollIntoView({ behavior: 'smooth' })); }}>Edit</button>
                  <button className="link danger" onClick={() => removeQuestion(q)}>Delete</button>
                </footer>
              </li>
            ))}
          </ol>
        </>
      )}
    </>
  );
}
