import { Link, useParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { minutesLabel } from '../../lib/format.js';
import Markdown from '../../shared/Markdown.jsx';
import ShareButtons from '../../shared/ShareButtons.jsx';
import { BLOG } from '../config.js';

const blog = api.blog(BLOG);

export default function GkCourse() {
  const { slug } = useParams();
  const { data: c, loading, error } = useAsync(() => blog.course(slug), [slug]);
  useTitle(c ? `${c.title} | Curious Minds` : 'Curious Minds');

  if (loading) return <div className="gk-wrap gk-section"><p className="gk-muted">Loading…</p></div>;
  if (error) return <div className="gk-wrap gk-empty"><h1>Course not found</h1><Link to="/gk/courses">All courses</Link></div>;

  const total = c.lessons.reduce((sum, l) => sum + l.durationMinutes, 0);
  return (
    <>
      <header className="gk-course-hero">
        <div className="gk-wrap gk-course-hero-grid">
          <div>
            <div className="gk-course-tags"><span className="gk-tag">{c.category}</span><span className="gk-level">{c.level}</span></div>
            <h1>{c.title}</h1>
            <p className="gk-post-dek">{c.subtitle}</p>
            <div className="gk-course-stats big"><span>📘 {c.lessons.length} lessons</span><span>⏱ {minutesLabel(total)}</span><span>🆓 Free</span></div>
            {c.lessons[0] && <Link to={`/gk/courses/${c.slug}/${c.lessons[0].slug}`} className="gk-btn">Start course →</Link>}
          </div>
          {c.thumbnailUrl && <img src={c.thumbnailUrl} alt="" />}
        </div>
      </header>

      <div className="gk-wrap gk-course-layout">
        <div>
          <h2>About this course</h2>
          <Markdown className="gk-prose">{c.description}</Markdown>
        </div>
        <aside>
          <h2>Syllabus</h2>
          <ol className="gk-steps">
            {c.lessons.map((l, i) => (
              <li key={l.id}>
                <Link to={`/gk/courses/${c.slug}/${l.slug}`}>
                  <span className="gk-step-num">{String(i + 1).padStart(2, '0')}</span>
                  <span className="gk-step-body"><b>{l.title}</b><small>{l.durationMinutes} min{l.hasVideo ? ' · video' : ''}</small></span>
                </Link>
              </li>
            ))}
          </ol>
          <div className="gk-share-box"><b>Share this course</b><ShareButtons title={c.title} className="share gk-share" /></div>
        </aside>
      </div>
    </>
  );
}
