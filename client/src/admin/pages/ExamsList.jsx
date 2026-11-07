import { Link } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import { StatusBadge } from '../components.jsx';
import { testHref } from '../../gk/jee.js';

export default function ExamsList() {
  const { data, loading, reload } = useAsync(() => api.get('/admin/exams'), []);

  async function remove(e) {
    if (!confirm(`Delete "${e.title}" with its ${e.questionCount} questions and all student attempts? This cannot be undone.`)) return;
    await api.del(`/admin/exams/${e.id}`);
    reload();
  }

  return (
    <>
      <div className="admin-head">
        <h1 className="admin-h1">Mock tests <span className="muted small">(Curious Minds → /gk/tests)</span></h1>
        <Link to="/admin/exams/new" className="admin-btn">+ New test</Link>
      </div>
      <p className="muted">Create a test, then add questions one by one or import a whole question bank as JSON. Students see published tests that have at least one question.</p>
      {loading && <p className="muted">Loading…</p>}
      {data?.length === 0 && <p className="muted">No tests yet.</p>}
      {data?.length > 0 && (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead><tr><th>Test</th><th>Type</th><th>Questions</th><th>Price</th><th>Time</th><th>Attempts</th><th>Status</th><th>Updated</th><th /></tr></thead>
            <tbody>
              {data.map((e) => (
                <tr key={e.id}>
                  <td><Link to={`/admin/exams/${e.id}`} className="strong">{e.title}</Link><div className="muted small">{e.category}</div></td>
                  <td>{e.kind}{e.year ? ` ${e.year}` : ''}</td>
                  <td>{e.questionCount}</td>
                  <td>{e.price > 0 ? `₹${e.price}` : 'Free'}</td>
                  <td>{e.durationMinutes} min</td>
                  <td>{e.attempts}</td>
                  <td><StatusBadge published={e.isPublished} /></td>
                  <td>{formatDate(e.updatedAt)}</td>
                  <td className="actions">
                    <Link to={`/admin/exams/${e.id}`}>Edit</Link>
                    <a href={testHref(e.slug)} target="_blank" rel="noreferrer">View</a>
                    <button className="link danger" onClick={() => remove(e)}>Delete</button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
