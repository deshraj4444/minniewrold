import { Link, useSearchParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import { blogLabel, blogOptions, blogPath, StatusBadge } from '../components.jsx';

export default function PostsList() {
  const [params, setParams] = useSearchParams();
  const blog = params.get('blog') ?? '';
  const { data, loading, reload } = useAsync(() => api.get(`/admin/posts${blog ? `?blog=${blog}` : ''}`), [blog]);

  async function remove(p) {
    if (!confirm(`Delete "${p.title}"? This cannot be undone.`)) return;
    await api.del(`/admin/posts/${p.id}`);
    reload();
  }

  return (
    <>
      <div className="admin-head">
        <h1 className="admin-h1">Posts</h1>
        <Link to={`/admin/posts/new${blog ? `?blog=${blog}` : ''}`} className="admin-btn">+ New post</Link>
      </div>
      <div className="admin-filter">
        <button className={!blog ? 'active' : ''} onClick={() => setParams({})}>All blogs</button>
        {blogOptions.map((b) => <button key={b.key} className={blog === b.key ? 'active' : ''} onClick={() => setParams({ blog: b.key })}>{b.label}</button>)}
      </div>
      {loading && <p className="muted">Loading…</p>}
      {data?.length === 0 && <p className="muted">No posts yet.</p>}
      {data?.length > 0 && (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead><tr><th>Title</th><th>Blog</th><th>Category</th><th>Status</th><th>Published</th><th /></tr></thead>
            <tbody>
              {data.map((p) => (
                <tr key={p.id}>
                  <td><Link to={`/admin/posts/${p.id}`} className="strong">{p.title}</Link></td>
                  <td>{blogLabel(p.blog)}</td>
                  <td>{p.category}</td>
                  <td><StatusBadge published={p.isPublished} /></td>
                  <td>{formatDate(p.publishedAt)}</td>
                  <td className="actions">
                    {p.isPublished && <a href={`${blogPath(p.blog)}/posts/${p.slug}`} target="_blank" rel="noreferrer">View</a>}
                    <Link to={`/admin/posts/${p.id}`}>Edit</Link>
                    <button className="link danger" onClick={() => remove(p)}>Delete</button>
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
