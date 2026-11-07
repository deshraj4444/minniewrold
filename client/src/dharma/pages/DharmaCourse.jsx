import { Link, useParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import Markdown from '../../shared/Markdown.jsx';
import ShareButtons from '../../shared/ShareButtons.jsx';
import { BLOG } from '../config.js';

const blog = api.blog(BLOG);

export default function DharmaCourse() {
  const { slug } = useParams();
  const { data: c, loading, error } = useAsync(() => blog.course(slug), [slug]);
  useTitle(c ? `${c.title} | Dharma Path` : 'Dharma Path');

  if (loading) return <p className="dh-muted dh-center">Loading…</p>;
  if (error) return <div className="dh-narrow dh-center"><h1>Series not found</h1><Link to="/dharma/courses">All series</Link></div>;

  return (
    <div className="dh-narrow">
      <header className="dh-center">
        <span className="dh-cat">{c.category} · {c.level}</span>
        <h1 className="dh-article-title">{c.title}</h1>
        <p className="dh-standfirst">{c.subtitle}</p>
      </header>
      {c.thumbnailUrl && <img className="dh-article-cover small" src={c.thumbnailUrl} alt="" />}
      <Markdown className="dh-prose">{c.description}</Markdown>

      <div className="dh-divider" aria-hidden="true"><span>❁</span></div>
      <h2 className="dh-heading">Lessons</h2>
      <ol className="dh-toc">
        {c.lessons.map((l, i) => (
          <li key={l.id}>
            <Link to={`/dharma/courses/${c.slug}/${l.slug}`}>
              <span className="dh-toc-num">{i + 1}</span>
              <span className="dh-toc-body">
                <b>{l.title}</b>
                {l.summary && <small>{l.summary}</small>}
              </span>
              <span className="dh-toc-time">{l.durationMinutes} min{l.hasVideo && ' · ▶'}</span>
            </Link>
          </li>
        ))}
      </ol>
      {c.lessons[0] && <p className="dh-center"><Link className="dh-btn" to={`/dharma/courses/${c.slug}/${c.lessons[0].slug}`}>Begin the first lesson</Link></p>}
      <div className="dh-share-row"><span>Share this series</span><ShareButtons title={c.title} className="share dh-share" /></div>
    </div>
  );
}
