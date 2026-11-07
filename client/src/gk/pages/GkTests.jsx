import { Link } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import { useStudent } from '../student.jsx';
import ShareButtons from '../../shared/ShareButtons.jsx';
import { audiences, examGroups, groupOf, examHref } from '../exams.js';

const titles = {
  'class-6': ['Free Class 6 Admission Tests: JNV, RMS, Sainik School | Curious Minds', 'Class 6 admission tests', 'कक्षा 6 प्रवेश परीक्षा'],
  teachers: ['HP TET & CTET Mock Tests | Curious Minds', 'Teacher eligibility tests', 'शिक्षक पात्रता परीक्षा'],
  all: ['Free Mock Tests | Curious Minds', 'Mock tests', 'मॉक टेस्ट']
};

/**
 * All tests, in sections by audience (Class 6 admission, teachers) and then by exam. Each exam tile opens that
 * exam's page with its papers. With `audience`, only that section is shown (e.g. /gk/class-6).
 */
export default function GkTests({ audience = 'all' }) {
  const [pageTitle, heading, headingHi] = titles[audience] ?? titles.all;
  useTitle(pageTitle);
  const { student } = useStudent();
  const { data, loading, error } = useAsync(() => api.exams.list(), []);

  // Count each exam's papers by kind so the tiles can say what's inside.
  const stats = {};
  for (const e of data ?? []) {
    const s = (stats[e.category] ??= { total: 0, free: 0, kinds: {} });
    s.total++;
    if (e.price === 0) s.free++;
    s.kinds[e.kind] = (s.kinds[e.kind] ?? 0) + 1;
  }
  const known = new Set(examGroups.map((g) => g.category));
  const extra = Object.keys(stats).filter((c) => !known.has(c)).map(groupOf);
  const shownAudiences = [...audiences, { key: 'more', name: 'More tests', nameHi: 'अन्य टेस्ट', icon: '🧠', blurb: '' }]
    .filter((a) => audience === 'all' || a.key === audience);

  return (
    <>
      <section className={audience === 'class-6' ? 'gk-tests-hero kids' : 'gk-tests-hero'}>
        <div className="gk-wrap">
          <span className="gk-tag">{audience === 'class-6' ? '100% free · Certificates' : 'Instant results'}</span>
          <h1 className="gk-page-title">{heading} <span className="gk-title-hi" lang="hi">{headingHi}</span></h1>
          <p className="gk-lead">
            {audience === 'class-6'
              ? 'Practise for Navodaya (JNVST), Rashtriya Military School and Sainik School entrance tests with papers on the past 5 years’ patterns and 5 expected papers for each exam, in English and Hindi. Pass a paper to earn a certificate, and warm up with the quick quiz game.'
              : 'Timed tests on the real exam pattern in English and Hindi (हिंदी). Submit to see your score, rank, section-wise analysis and the answer to every question with an explanation.'}
          </p>
          {audience === 'all' && (
            <nav className="gk-audience-jump" aria-label="Choose your exam type">
              {audiences.map((a) => (
                <Link key={a.key} to={`/gk/${a.key}`} className="gk-audience-chip">
                  <span aria-hidden="true">{a.icon}</span> {a.name} <small lang="hi">{a.nameHi}</small>
                </Link>
              ))}
            </nav>
          )}
          <div className="gk-share-box"><b>Share with friends / मित्रों के साथ शेयर करें</b><ShareButtons title={audience === 'class-6' ? 'Free JNV, RMS & Sainik School Class 6 admission practice papers with certificates' : 'Free HP TET, CTET & JBT mock tests and previous year papers in English + हिंदी'} url={window.location.href} className="share gk-share" /></div>
          {student === null && (
            <div className="gk-hero-cta">
              <Link to={`/gk/register?next=${encodeURIComponent(window.location.pathname)}`} className="gk-btn">Create free account</Link>
              <Link to={`/gk/login?next=${encodeURIComponent(window.location.pathname)}`} className="gk-btn gk-btn-outline">Log in</Link>
            </div>
          )}
          {student && (
            <p className="gk-muted">
              Welcome back, <b>{student.fullName}</b>.{' '}
              {student.allAccess && <span className="gk-pass">⭐ All-Access pass{student.allAccessUntil ? ` until ${formatDate(student.allAccessUntil)}` : ''}</span>}{' '}
              <Link to="/gk/my-tests">See your results and certificates →</Link>
            </p>
          )}
        </div>
      </section>

      <div className="gk-wrap gk-section">
        {loading && <p className="gk-muted">Loading tests…</p>}
        {error && <p className="gk-err">{error.message}</p>}

        {data && shownAudiences.map((a) => {
          const groups = a.key === 'more' ? extra : examGroups.filter((g) => g.audience === a.key && stats[g.category]);
          if (!groups.length) return null;
          return (
            <section key={a.key} id={a.key} className="gk-audience">
              <header className="gk-audience-head">
                <span className="gk-audience-icon" aria-hidden="true">{a.icon}</span>
                <div>
                  <h2>{a.name} <small lang="hi">{a.nameHi}</small></h2>
                  {a.blurb && <p className="gk-muted">{a.blurb}</p>}
                </div>
              </header>
              <div className="gk-exam-tiles">
                {groups.map((g) => <ExamTile key={g.key} group={g} stats={stats[g.category]} kids={a.key === 'class-6'} />)}
              </div>
            </section>
          );
        })}
        {data && data.length === 0 && <p className="gk-empty">No tests yet. Check back soon.</p>}
      </div>
    </>
  );
}

function ExamTile({ group, stats, kids }) {
  const k = stats.kinds;
  const bits = [
    k['Year pattern'] && `📅 ${k['Year pattern']} past-year pattern`,
    k['Previous year'] && `📜 ${k['Previous year']} previous year`,
    k['Expected paper'] && `🔮 ${k['Expected paper']} expected`,
    k['Mock test'] && `📝 ${k['Mock test']} mock`,
    (k['Sample test'] || k.Sectional) && `🎁 ${(k['Sample test'] ?? 0) + (k.Sectional ?? 0)} practice`
  ].filter(Boolean);
  return (
    <article className="gk-exam-tile" style={{ '--tile': group.color }}>
      <Link to={examHref(group)} className="gk-exam-tile-main">
        <span className="gk-exam-tile-icon" aria-hidden="true">{group.icon}</span>
        <h3>{group.name}</h3>
        {group.nameHi && <p className="gk-exam-tile-hi" lang="hi">{group.nameHi}</p>}
        {group.pattern && <p className="gk-exam-tile-pattern">{group.pattern}</p>}
        <ul className="gk-exam-tile-counts">{bits.map((b) => <li key={b}>{b}</li>)}</ul>
        <span className="gk-exam-tile-free">{stats.free === stats.total ? `All ${stats.total} papers free` : `${stats.total} papers · ${stats.free} free`}</span>
      </Link>
      <div className="gk-exam-tile-actions">
        <Link to={examHref(group)} className="gk-btn">Open papers →</Link>
        {kids && <Link to={`/gk/play/${group.key}`} className="gk-btn gk-btn-play">🎮 Quiz game</Link>}
      </div>
    </article>
  );
}
