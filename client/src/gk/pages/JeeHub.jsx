import { Link } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { usePrerenderHandoff } from '../../lib/prerender.js';
import { BLOG } from '../config.js';
import { jeeHubHeading, jeePages } from '../jee.js';

const blog = api.blog(BLOG);

/**
 * /jee: every JEE chapter, grouped by subject, with its notes and its test. The server renders the same list
 * (SpaHost.Jee.cs) for search engines, so keep the two showing the same pages and text.
 */
export default function JeeHub() {
  useTitle('IIT JEE Main Free Notes & Chapter Tests (English + Hindi)');
  // Only link pages whose test or notes are actually published.
  const available = useAsync(async () => {
    const [tests, notes] = await Promise.all([api.exams.list({ category: 'JEE Main' }), blog.posts({ category: 'IIT JEE', pageSize: 50 })]);
    return new Set([...tests.map((t) => t.slug), ...notes.items.map((p) => p.slug)]);
  }, []);
  usePrerenderHandoff(!available.loading);

  const pages = jeePages.filter((p) => available.data?.has(p.slug));
  const subjects = [...new Set(pages.map((p) => p.subject))];

  return (
    <>
      <section className="gk-tests-hero">
        <div className="gk-wrap">
          <nav className="gk-crumbs" aria-label="Breadcrumb"><Link to="/gk">Curious Minds</Link> › IIT JEE</nav>
          <span className="gk-tag">Free · JEE Main &amp; Advanced</span>
          <h1 className="gk-page-title">{jeeHubHeading}</h1>
          <p className="gk-lead">Free chapter-wise notes and tests for JEE Main and Advanced, with real JEE marking (+4 / −1) and a step-by-step solution to every question in English and Hindi.</p>
          <p className="gk-lead" lang="hi">JEE Main और Advanced के लिए मुफ़्त अध्यायवार नोट्स और टेस्ट, JEE अंकन (+4 / −1) और हर प्रश्न का अंग्रेज़ी व हिंदी में पूरा हल।</p>
        </div>
      </section>

      <div className="gk-wrap gk-section">
        {available.loading && <p className="gk-muted">Loading chapters…</p>}
        {available.error && <p className="gk-err">{available.error.message}</p>}
        {subjects.map((subject) => (
          <section key={subject} className="gk-test-group">
            <h2>{subject}</h2>
            <div className="gk-test-grid">
              {pages.filter((p) => p.subject === subject).map((p) => (
                <Link key={p.path} to={p.path} className="gk-test-card">
                  <span className="gk-test-kind">{p.kind === 'test' ? 'Chapter test' : 'Chapter notes'} · {p.topic}</span>
                  <h3>{p.title}</h3>
                  <p>{p.description}</p>
                  <span className="gk-test-stats">{p.kind === 'test' ? 'Start the test →' : 'Read the notes →'}</span>
                </Link>
              ))}
            </div>
          </section>
        ))}
        {available.data && pages.length === 0 && <p className="gk-empty">Chapters are on their way. Check back soon.</p>}
        <p className="gk-note">More chapters in Physics, Chemistry and Mathematics are being added. Every question here is an original practice question, not a previous year question.</p>
      </div>
    </>
  );
}
