import { Link } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { BLOG } from '../config.js';

const blog = api.blog(BLOG);

export default function DharmaCourses() {
  useTitle('Learning Series | Dharma Path');
  const { data, loading } = useAsync(() => blog.courses(), []);

  return (
    <div className="dh-wide">
      <h1 className="dh-page-title">Learning series</h1>
      <p className="dh-muted dh-center">Structured study of scriptures, stotras and traditions, in short lessons you can read in one sitting.</p>
      {loading && <p className="dh-muted dh-center">Loading…</p>}
      {data?.length === 0 && <p className="dh-muted dh-center">New series are on the way.</p>}
      <div className="dh-series-list">
        {data?.map((c) => (
          <Link key={c.id} to={`/dharma/courses/${c.slug}`} className="dh-series-card">
            {c.thumbnailUrl && <img src={c.thumbnailUrl} alt="" loading="lazy" />}
            <div>
              <span className="dh-cat">{c.category}</span>
              <h2>{c.title}</h2>
              <p>{c.subtitle}</p>
              <span className="dh-meta">{c.lessonCount} lessons · {c.totalMinutes} min · {c.level}</span>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
}
