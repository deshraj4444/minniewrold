import { Link, useParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import Markdown from '../../shared/Markdown.jsx';
import ShareButtons from '../../shared/ShareButtons.jsx';
import VideoEmbed from '../../shared/VideoEmbed.jsx';
import { BLOG } from '../config.js';

const blog = api.blog(BLOG);

export default function GkLesson() {
  const { slug, lessonSlug } = useParams();
  const { data, loading, error } = useAsync(() => blog.lesson(slug, lessonSlug), [slug, lessonSlug]);
  useTitle(data ? `${data.lesson.title} · ${data.course.title}` : 'Curious Minds');

  if (loading) return <div className="gk-wrap gk-section"><p className="gk-muted">Loading…</p></div>;
  if (error) return <div className="gk-wrap gk-empty"><h1>Lesson not found</h1><Link to={`/gk/courses/${slug}`}>Back to course</Link></div>;

  const { course, lesson, position, previous, next } = data;
  const pct = Math.round((position / course.lessons.length) * 100);

  return (
    <div className="gk-wrap gk-lesson-layout">
      <aside className="gk-lesson-side">
        <Link to={`/gk/courses/${course.slug}`} className="gk-back">← {course.title}</Link>
        <div className="gk-meter" aria-label={`Lesson ${position} of ${course.lessons.length}`}><span style={{ width: `${pct}%` }} /></div>
        <ol className="gk-steps compact">
          {course.lessons.map((l, i) => (
            <li key={l.slug} className={l.slug === lesson.slug ? 'current' : ''}>
              <Link to={`/gk/courses/${course.slug}/${l.slug}`} aria-current={l.slug === lesson.slug ? 'page' : undefined}>
                <span className="gk-step-num">{String(i + 1).padStart(2, '0')}</span>
                <span className="gk-step-body"><b>{l.title}</b></span>
              </Link>
            </li>
          ))}
        </ol>
      </aside>

      <article className="gk-lesson-main">
        <span className="gk-tag">Lesson {position} of {course.lessons.length}</span>
        <h1>{lesson.title}</h1>
        {lesson.summary && <p className="gk-post-dek">{lesson.summary}</p>}
        <VideoEmbed url={lesson.videoUrl} title={lesson.title} className="gk-video" />
        <Markdown className="gk-prose">{lesson.content}</Markdown>

        <nav className="gk-lesson-nav" aria-label="Lessons">
          {previous ? <Link to={`/gk/courses/${course.slug}/${previous.slug}`} rel="prev">← {previous.title}</Link> : <span />}
          {next ? <Link to={`/gk/courses/${course.slug}/${next.slug}`} rel="next" className="gk-btn">Next: {next.title} →</Link>
            : <Link to={`/gk/courses/${course.slug}`} className="gk-btn">🎉 Finish course</Link>}
        </nav>
        <div className="gk-share-box"><b>Share this lesson</b><ShareButtons title={`${lesson.title} · ${course.title}`} className="share gk-share" /></div>
      </article>
    </div>
  );
}
