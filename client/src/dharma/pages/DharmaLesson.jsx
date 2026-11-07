import { Link, useParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import Markdown from '../../shared/Markdown.jsx';
import ShareButtons from '../../shared/ShareButtons.jsx';
import VideoEmbed from '../../shared/VideoEmbed.jsx';
import { BLOG } from '../config.js';

const blog = api.blog(BLOG);

export default function DharmaLesson() {
  const { slug, lessonSlug } = useParams();
  const { data, loading, error } = useAsync(() => blog.lesson(slug, lessonSlug), [slug, lessonSlug]);
  useTitle(data ? `${data.lesson.title} · ${data.course.title}` : 'Dharma Path');

  if (loading) return <p className="dh-muted dh-center">Loading…</p>;
  if (error) return <div className="dh-narrow dh-center"><h1>Lesson not found</h1><Link to={`/dharma/courses/${slug}`}>Back to the series</Link></div>;

  const { course, lesson, position, previous, next } = data;
  return (
    <article className="dh-narrow">
      <p className="dh-crumb"><Link to={`/dharma/courses/${course.slug}`}>{course.title}</Link> · Lesson {position} of {course.lessons.length}</p>
      <h1 className="dh-article-title">{lesson.title}</h1>
      {lesson.summary && <p className="dh-standfirst">{lesson.summary}</p>}
      <VideoEmbed url={lesson.videoUrl} title={lesson.title} className="dh-video" />
      <Markdown className="dh-prose">{lesson.content}</Markdown>

      <nav className="dh-lesson-nav" aria-label="Lessons">
        {previous ? <Link to={`/dharma/courses/${course.slug}/${previous.slug}`} rel="prev"><small>Previous</small>{previous.title}</Link> : <span />}
        {next ? <Link to={`/dharma/courses/${course.slug}/${next.slug}`} rel="next" className="next"><small>Next</small>{next.title}</Link>
          : <Link to={`/dharma/courses/${course.slug}`} className="next"><small>Complete</small>Back to the series</Link>}
      </nav>
      <div className="dh-share-row"><span>Share this lesson</span><ShareButtons title={`${lesson.title} · ${course.title}`} className="share dh-share" /></div>
    </article>
  );
}
