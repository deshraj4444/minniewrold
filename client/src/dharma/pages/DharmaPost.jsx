import { Link, useParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import Markdown from '../../shared/Markdown.jsx';
import ShareButtons from '../../shared/ShareButtons.jsx';
import { BLOG } from '../config.js';
import { categoryLabel, isHindi } from '../categories.js';

const blog = api.blog(BLOG);

export default function DharmaPost() {
  const { slug } = useParams();
  const { data, loading, error } = useAsync(() => blog.post(slug), [slug]);
  useTitle(data ? `${data.post.title} | Dharma Path` : 'Dharma Path');

  if (loading) return <p className="dh-muted dh-center">Loading…</p>;
  if (error) return <div className="dh-narrow dh-center"><h1>Article not found</h1><Link to="/dharma/posts">See all articles</Link></div>;

  const { post, readMinutes, related } = data;
  return (
    <article className="dh-article" lang={isHindi(post.title) ? 'hi' : undefined}>
      <header className="dh-narrow dh-center">
        <Link to={`/dharma/posts?category=${encodeURIComponent(post.category)}`} className="dh-cat">{categoryLabel(post.category)}</Link>
        <h1 className="dh-article-title">{post.title}</h1>
        {post.excerpt && <p className="dh-standfirst">{post.excerpt}</p>}
        <p className="dh-meta">{post.author && `${post.author} · `}{formatDate(post.publishedAt, { day: 'numeric', month: 'long', year: 'numeric' })} · {readMinutes} min read</p>
      </header>
      {post.coverImageUrl && <img className="dh-article-cover" src={post.coverImageUrl} alt="" />}
      <div className="dh-narrow">
        <Markdown className={isHindi(post.content.slice(0, 400)) ? 'dh-prose is-hindi' : 'dh-prose'}>{post.content}</Markdown>
        <div className="dh-divider" aria-hidden="true"><span>❁</span></div>
        <div className="dh-share-row">
          <span>{isHindi(post.title) ? 'इसे अपने परिवार और मित्रों के साथ साझा करें' : 'Share this with someone it may help'}</span>
          <ShareButtons title={post.title} className="share dh-share" />
        </div>
        {post.tags && <p className="dh-tags">{post.tags.split(',').map((t) => <span key={t}>#{t.trim()}</span>)}</p>}
      </div>

      {related.length > 0 && (
        <aside className="dh-wide">
          <h2 className="dh-heading">Continue reading</h2>
          <div className="dh-post-grid">
            {related.map((r) => (
              <Link key={r.slug} to={`/dharma/posts/${r.slug}`} className="dh-post-card">
                {r.coverImageUrl && <img src={r.coverImageUrl} alt="" loading="lazy" />}
                <span className="dh-cat">{r.category}</span>
                <h3>{r.title}</h3>
                <p>{r.excerpt}</p>
              </Link>
            ))}
          </div>
        </aside>
      )}
    </article>
  );
}
