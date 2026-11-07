import { Link, useSearchParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import { blogLabel, blogOptions, StatusBadge } from '../components.jsx';

export default function CoursesList() {
  const [params, setParams] = useSearchParams();
  const blog = params.get('blog') ?? '';
  const { data, loading, reload } = useAsync(() => api.get(`/admin/courses${blog ? `?blog=${blog}` : ''}`), [blog]);

  async function remove(c) {
    if (!confirm(`Delete "${c.title}" and all ${c.lessonCount} lessons? This cannot be undone.`)) return;
    await api.del(`/admin/courses/${c.id}`);
    reload();
  }

  return (
    <>
      <div className="admin-head">
        <h1 className="admin-h1">Courses</h1>
        <Link to={`/admin/courses/new${blog ? `?blog=${blog}` : ''}`} className="admin-btn">+ New course</Link>
      </div>
      <div className="admin-filter">
        <button className={!blog ? 'active' : ''} onClick={() => setParams({})}>All blogs</button>
        {blogOptions.map((b) => <button key={b.key} className={blog === b.key ? 'active' : ''} onClick={() => setParams({ blog: b.key })}>{b.label}</button>)}
      </div>
      {loading && <p className="muted">Loading…</p>}
      {data?.length === 0 && <p className="muted">No courses yet.</p>}
      {data?.length > 0 && (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead><tr><th>Course</th><th>Blog</th><th>Lessons</th><th>Status</th><th>Updated</th><th /></tr></thead>
            <tbody>
              {data.map((c) => (
                <tr key={c.id}>
                  <td><Link to={`/admin/courses/${c.id}`} className="strong">{c.title}</Link>{c.isFeatured && <span className="pill pill-blue">Featured</span>}</td>
                  <td>{blogLabel(c.blog)}</td>
                  <td>{c.lessonCount}</td>
                  <td><StatusBadge published={c.isPublished} /></td>
                  <td>{formatDate(c.updatedAt)}</td>
                  <td className="actions">
                    <Link to={`/admin/courses/${c.id}`}>Edit</Link>
                    <button className="link danger" onClick={() => remove(c)}>Delete</button>
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
