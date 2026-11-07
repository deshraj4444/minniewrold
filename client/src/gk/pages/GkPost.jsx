import { useEffect, useState } from 'react';
import { Link, Navigate, useParams } from 'react-router';
import { usePrerenderHandoff } from '../../lib/prerender.js';
import { JEE_HUB, jeeCompanion, jeePageFor, postHref, rewriteOldLinks } from '../jee.js';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import Markdown from '../../shared/Markdown.jsx';
import ShareButtons from '../../shared/ShareButtons.jsx';
import { BLOG } from '../config.js';

const blog = api.blog(BLOG);

/** Thin bar at the top showing how far through the article the reader is. */
function ReadingProgress() {
  const [pct, setPct] = useState(0);
  useEffect(() => {
    const onScroll = () => {
      const max = document.documentElement.scrollHeight - window.innerHeight;
      setPct(max > 0 ? Math.min(100, (window.scrollY / max) * 100) : 0);
    };
    onScroll();
    window.addEventListener('scroll', onScroll, { passive: true });
    return () => window.removeEventListener('scroll', onScroll);
  }, []);
  return <div className="gk-progress" style={{ width: `${pct}%` }} aria-hidden="true" />;
}

/**
 * An article. With `jeePage` (the /jee chapter notes) it links to the chapter's test at the top and the bottom,
 * matching what the server renders for search engines (SpaHost.Jee.cs).
 */
export default function GkPost({ slug: slugProp, jeePage }) {
  const params = useParams();
  const slug = slugProp ?? params.slug;
  const moved = !slugProp && jeePageFor('notes', slug);
  const { data, loading, error } = useAsync(() => blog.post(slug), [slug]);
  useTitle(jeePage ? jeePage.title : data ? `${data.post.title} | Curious Minds` : 'Curious Minds');
  usePrerenderHandoff(Boolean(jeePage) && !loading);
  const test = jeePage && jeeCompanion(jeePage);

  // Old /gk/posts/<slug> links to JEE notes go to their /jee page (the server sends a 301 for direct visits).
  if (moved) return <Navigate to={moved.path} replace />;
  if (loading) return <div className="gk-wrap gk-section"><p className="gk-muted">Loading…</p></div>;
  if (error) return <div className="gk-wrap gk-empty"><h1>Article not found</h1><Link to="/gk/posts">Explore articles</Link></div>;

  const { post, readMinutes, related } = data;
  return (
    <article>
      <ReadingProgress />
      <header className="gk-post-head">
        <div className="gk-wrap gk-post-head-grid">
          <div>
            {jeePage && <nav className="gk-crumbs" aria-label="Breadcrumb"><Link to="/gk">Curious Minds</Link> › <Link to={JEE_HUB}>IIT JEE</Link> › {jeePage.crumb}</nav>}
            <Link to={`/gk/posts?category=${encodeURIComponent(post.category)}`} className="gk-tag">{post.category}</Link>
            <h1>{post.title}</h1>
            {post.excerpt && <p className="gk-post-dek">{post.excerpt}</p>}
            <p className="gk-meta">{post.author && <b>{post.author}</b>} · {formatDate(post.publishedAt)} · {readMinutes} min read</p>
            {test && <p><Link to={test.path} className="gk-btn">Practise now: {test.title}</Link></p>}
          </div>
          {post.coverImageUrl && <img src={post.coverImageUrl} alt="" />}
        </div>
      </header>

      <div className="gk-wrap gk-post-layout">
        <aside className="gk-post-aside">
          <span>Share</span>
          <ShareButtons title={post.title} className="share gk-share" />
        </aside>
        <div className="gk-post-body">
          <Markdown className="gk-prose">{jeePage ? rewriteOldLinks(post.content) : post.content}</Markdown>
          {test && <p className="gk-result-actions"><Link to={test.path} className="gk-btn">Practise now: {test.title}</Link><Link to={JEE_HUB} className="gk-link">All IIT JEE chapters</Link></p>}
          {post.tags && <div className="gk-tags">{post.tags.split(',').map((t) => <Link key={t} to={`/gk/posts?q=${encodeURIComponent(t.trim())}`}>#{t.trim()}</Link>)}</div>}
          <div className="gk-share-box">
            <b>Found this useful?</b> Share it with a friend who loves facts.
            <ShareButtons title={post.title} className="share gk-share" />
          </div>
        </div>
      </div>

      {related.length > 0 && (
        <section className="gk-band">
          <div className="gk-wrap gk-section">
            <div className="gk-section-head"><h2>Keep learning</h2></div>
            <div className="gk-grid">
              {related.map((r) => (
                <Link key={r.slug} to={postHref(r.slug)} className="gk-card">
                  {r.coverImageUrl && <img src={r.coverImageUrl} alt="" loading="lazy" />}
                  <div className="gk-card-body"><span className="gk-tag">{r.category}</span><h2>{r.title}</h2><p>{r.excerpt}</p></div>
                </Link>
              ))}
            </div>
          </div>
        </section>
      )}
    </article>
  );
}
