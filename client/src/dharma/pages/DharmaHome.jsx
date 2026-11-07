import { Link } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import { BLOG } from '../config.js';

const blog = api.blog(BLOG);

const verses = [
  { sa: 'कर्मण्येवाधिकारस्ते मा फलेषु कदाचन ।', en: 'You have a right to your actions, never to their fruits.', ref: 'Bhagavad Gita 2.47' },
  { sa: 'असतो मा सद्गमय । तमसो मा ज्योतिर्गमय ।', en: 'Lead me from the unreal to the real, from darkness to light.', ref: 'Brihadaranyaka Upanishad 1.3.28' },
  { sa: 'वसुधैव कुटुम्बकम् ।', en: 'The whole world is one family.', ref: 'Maha Upanishad 6.72' },
  { sa: 'योगः कर्मसु कौशलम् ।', en: 'Yoga is skill in action.', ref: 'Bhagavad Gita 2.50' }
];

export default function DharmaHome() {
  useTitle('Dharma Path');
  const posts = useAsync(() => blog.posts({ pageSize: 7 }), []);
  const courses = useAsync(() => blog.courses(), []);
  const verse = verses[new Date().getDate() % verses.length];

  const [featured, ...rest] = posts.data?.items ?? [];

  return (
    <>
      <section className="dh-verse">
        <p className="dh-verse-label">Verse of the day</p>
        <p className="dh-verse-sa" lang="sa">{verse.sa}</p>
        <p className="dh-verse-en">“{verse.en}”</p>
        <p className="dh-verse-ref">{verse.ref}</p>
      </section>

      <div className="dh-wide">
        {posts.loading && <p className="dh-muted dh-center">Loading…</p>}
        {featured && (
          <Link to={`/dharma/posts/${featured.slug}`} className="dh-featured">
            {featured.coverImageUrl && <img src={featured.coverImageUrl} alt="" loading="eager" />}
            <div className="dh-featured-body">
              <span className="dh-cat">{featured.category}</span>
              <h1>{featured.title}</h1>
              <p>{featured.excerpt}</p>
              <span className="dh-meta">{formatDate(featured.publishedAt)} · {featured.readMinutes} min read</span>
            </div>
          </Link>
        )}

        {rest.length > 0 && (
          <>
            <div className="dh-divider" aria-hidden="true"><span>❁</span></div>
            <h2 className="dh-heading">Recent writings</h2>
            <div className="dh-post-grid">
              {rest.map((p) => (
                <Link key={p.id} to={`/dharma/posts/${p.slug}`} className="dh-post-card">
                  {p.coverImageUrl && <img src={p.coverImageUrl} alt="" loading="lazy" />}
                  <span className="dh-cat">{p.category}</span>
                  <h3>{p.title}</h3>
                  <p>{p.excerpt}</p>
                  <span className="dh-meta">{formatDate(p.publishedAt)}</span>
                </Link>
              ))}
            </div>
            <p className="dh-center"><Link className="dh-btn" to="/dharma/posts">All articles</Link></p>
          </>
        )}

        {courses.data?.length > 0 && (
          <section className="dh-series">
            <div className="dh-divider" aria-hidden="true"><span>❁</span></div>
            <h2 className="dh-heading">Learning series</h2>
            <p className="dh-muted dh-center">Study the scriptures step by step, one short lesson at a time.</p>
            <div className="dh-series-list">
              {courses.data.map((c) => (
                <Link key={c.id} to={`/dharma/courses/${c.slug}`} className="dh-series-card">
                  {c.thumbnailUrl && <img src={c.thumbnailUrl} alt="" loading="lazy" />}
                  <div>
                    <h3>{c.title}</h3>
                    <p>{c.subtitle}</p>
                    <span className="dh-meta">{c.lessonCount} lessons · {c.totalMinutes} min</span>
                  </div>
                </Link>
              ))}
            </div>
          </section>
        )}
      </div>
    </>
  );
}
