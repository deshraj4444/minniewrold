import { Link, useSearchParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import { BLOG } from '../config.js';
import { postHref } from '../jee.js';
import SearchBox from '../components/SearchBox.jsx';

const blog = api.blog(BLOG);

export default function GkPosts() {
  useTitle('Explore | Curious Minds');
  const [params, setParams] = useSearchParams();
  const category = params.get('category') ?? '';
  const q = params.get('q') ?? '';
  const page = Number(params.get('page') ?? 1);

  const categories = useAsync(() => blog.categories(), []);
  const posts = useAsync(() => blog.posts({ category, q, page, pageSize: 12 }), [category, q, page]);
  const totalPages = posts.data ? Math.ceil(posts.data.total / posts.data.pageSize) : 1;
  const update = (next) => setParams(Object.fromEntries(Object.entries({ category, q, ...next }).filter(([, v]) => v)));

  return (
    <div className="gk-wrap gk-section">
      <h1 className="gk-page-title">{q ? <>Results for “{q}”</> : category || 'Explore everything'}</h1>
      {/* key: a new search (typed or spoken) resets the box to show it. */}
      <SearchBox key={q} id="gk-facts-q" defaultValue={q} className="gk-search gk-search-big" placeholder="Search facts by typing or speaking…" />
      <div className="gk-tabs" role="list">
        <button role="listitem" className={!category ? 'active' : ''} onClick={() => update({ category: '', page: '' })}>All</button>
        {categories.data?.posts.map((c) => (
          <button role="listitem" key={c} className={c === category ? 'active' : ''} onClick={() => update({ category: c, page: '' })}>{c}</button>
        ))}
      </div>
      {q && <p><button className="gk-link" onClick={() => update({ q: '', page: '' })}>✕ Clear search</button></p>}

      {posts.loading && <p className="gk-muted">Loading…</p>}
      {posts.data?.items.length === 0 && <div className="gk-empty"><p>Nothing here yet. Try another search.</p></div>}

      <div className="gk-grid">
        {posts.data?.items.map((p) => (
          <Link key={p.id} to={postHref(p.slug)} className="gk-card">
            {p.coverImageUrl && <img src={p.coverImageUrl} alt="" loading="lazy" />}
            <div className="gk-card-body">
              <span className="gk-tag">{p.category}</span>
              <h2>{p.title}</h2>
              <p>{p.excerpt}</p>
              <span className="gk-meta">{formatDate(p.publishedAt)} · {p.readMinutes} min read</span>
            </div>
          </Link>
        ))}
      </div>

      {totalPages > 1 && (
        <nav className="gk-pager" aria-label="Pages">
          <button disabled={page <= 1} onClick={() => update({ page: String(page - 1) })}>← Prev</button>
          <span>{page} / {totalPages}</span>
          <button disabled={page >= totalPages} onClick={() => update({ page: String(page + 1) })}>Next →</button>
        </nav>
      )}
    </div>
  );
}
