import { Link, useSearchParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import { BLOG } from '../config.js';
import { categoryLabel } from '../categories.js';

const blog = api.blog(BLOG);

export default function DharmaPosts() {
  useTitle('Articles | Dharma Path');
  const [params, setParams] = useSearchParams();
  const category = params.get('category') ?? '';
  const q = params.get('q') ?? '';
  const page = Number(params.get('page') ?? 1);

  const categories = useAsync(() => blog.categories(), []);
  const posts = useAsync(() => blog.posts({ category, q, page, pageSize: 10 }), [category, q, page]);
  const totalPages = posts.data ? Math.ceil(posts.data.total / posts.data.pageSize) : 1;

  const update = (next) => setParams(Object.fromEntries(Object.entries({ category, q, ...next }).filter(([, v]) => v)));

  return (
    <div className="dh-wide">
      <h1 className="dh-page-title">Articles</h1>
      <form className="dh-search" role="search" onSubmit={(e) => { e.preventDefault(); update({ q: new FormData(e.currentTarget).get('q'), page: '' }); }}>
        <label htmlFor="dh-q" className="visually-hidden">Search articles</label>
        <input id="dh-q" name="q" defaultValue={q} placeholder="मंत्र, स्तोत्र, मंदिर खोजें… / Search mantras, temples…" />
        <button className="dh-btn">Search</button>
      </form>

      <div className="dh-chips" role="list">
        <button role="listitem" className={!category ? 'active' : ''} onClick={() => update({ category: '', page: '' })}>All</button>
        {categories.data?.posts.map((c) => (
          <button role="listitem" key={c} className={c === category ? 'active' : ''} onClick={() => update({ category: c, page: '' })}>{categoryLabel(c)}</button>
        ))}
      </div>

      {posts.loading && <p className="dh-muted dh-center">Loading…</p>}
      {posts.data?.items.length === 0 && <p className="dh-muted dh-center">No articles found.</p>}
      <div className="dh-list">
        {posts.data?.items.map((p) => (
          <article key={p.id} className="dh-list-item">
            {p.coverImageUrl && <Link to={`/dharma/posts/${p.slug}`} tabIndex={-1}><img src={p.coverImageUrl} alt="" loading="lazy" /></Link>}
            <div>
              <span className="dh-cat">{categoryLabel(p.category)}</span>
              <h2><Link to={`/dharma/posts/${p.slug}`}>{p.title}</Link></h2>
              <p>{p.excerpt}</p>
              <span className="dh-meta">{formatDate(p.publishedAt)} · {p.readMinutes} min read</span>
            </div>
          </article>
        ))}
      </div>

      {totalPages > 1 && (
        <nav className="dh-pager" aria-label="Pages">
          <button disabled={page <= 1} onClick={() => update({ page: String(page - 1) })}>← Newer</button>
          <span>Page {page} of {totalPages}</span>
          <button disabled={page >= totalPages} onClick={() => update({ page: String(page + 1) })}>Older →</button>
        </nav>
      )}
    </div>
  );
}
