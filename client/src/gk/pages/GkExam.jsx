import { useState } from 'react';
import { Link, Navigate, useParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { minutesLabel } from '../../lib/format.js';
import ShareButtons from '../../shared/ShareButtons.jsx';
import { audiences, groupByKey, kindInfo, sortTests } from '../exams.js';
import { testHref } from '../jee.js';

/** One exam (e.g. JNVST Class 6): its papers in sections by type: past-year patterns, expected papers, free tests. */
export default function GkExam() {
  const { key } = useParams();
  const group = groupByKey(key);
  const { data, loading, error } = useAsync(() => (group ? api.exams.list({ category: group.category }) : Promise.resolve([])), [key]);
  const [kind, setKind] = useState('all');
  useTitle(group ? `${group.name} free practice papers | Curious Minds` : 'Exam | Curious Minds');

  // Exams with their own section (JEE Main at /jee) live there; the server sends a 301 for direct visits.
  if (group?.href) return <Navigate to={group.href} replace />;
  if (!group) return <div className="gk-wrap gk-empty"><h1>Exam not found</h1><Link to="/gk/tests">All tests</Link></div>;

  const tests = sortTests(data ?? []);
  const kinds = Object.keys(kindInfo).filter((k) => tests.some((t) => t.kind === k));
  const shownKinds = kind === 'all' ? kinds : [kind];
  const audience = audiences.find((a) => a.key === group.audience);
  const kids = group.audience === 'class-6';

  return (
    <>
      <section className="gk-exam-hero" style={{ '--tile': group.color }}>
        <div className="gk-wrap">
          <nav className="gk-crumbs">
            <Link to="/gk/tests">Mock tests</Link> › {audience ? <Link to={`/gk/${audience.key}`}>{audience.name}</Link> : 'More tests'} › {group.name}
          </nav>
          <h1 className="gk-page-title"><span aria-hidden="true">{group.icon}</span> {group.name}</h1>
          {group.nameHi && <p className="gk-title-hi" lang="hi">{group.nameHi}</p>}
          {group.pattern && <p className="gk-exam-pattern">{group.pattern}{group.patternHi && <><br /><span lang="hi">{group.patternHi}</span></>}</p>}
          {kids && (
            <div className="gk-kids-banner">
              <div><b>🏅 Earn a certificate</b><span>Score 60% or more in any paper: Bronze 60%+, Silver 75%+, Gold 90%+.</span></div>
              <Link to={`/gk/play/${group.key}`} className="gk-btn gk-btn-play">🎮 Play the quick quiz</Link>
            </div>
          )}
          <div className="gk-share-box"><b>Share / शेयर करें</b><ShareButtons title={`${group.name}: free practice papers in English + हिंदी`} url={window.location.href} className="share gk-share" /></div>
        </div>
      </section>

      <div className="gk-wrap gk-section">
        {kinds.length > 1 && (
          <div className="gk-tabs" role="tablist">
            {['all', ...kinds].map((k) => (
              <button key={k} role="tab" aria-selected={kind === k} className={kind === k ? 'active' : ''} onClick={() => setKind(k)}>
                {k === 'all' ? `All papers (${tests.length})` : `${kindInfo[k].icon} ${kindInfo[k].label.replace(/ \(.*\)$/, '')} (${tests.filter((t) => t.kind === k).length})`}
              </button>
            ))}
          </div>
        )}
        {loading && <p className="gk-muted">Loading papers…</p>}
        {error && <p className="gk-err">{error.message}</p>}
        {data && tests.length === 0 && <p className="gk-empty">No papers here yet. Check back soon.</p>}

        {shownKinds.map((k) => (
          <section key={k} className="gk-test-group">
            <h2>{kindInfo[k].icon} {kindInfo[k].label} <small lang="hi">{kindInfo[k].labelHi}</small></h2>
            {k === 'Year pattern' && <p className="gk-muted">Original papers that follow each year’s exam pattern and topic mix, to practise exactly what that year asked. They are not copies of the official papers.</p>}
            {k === 'Expected paper' && <p className="gk-muted">Original papers on the latest pattern, with the kinds of questions most likely to come in the next exam.</p>}
            <div className="gk-test-grid">
              {tests.filter((t) => t.kind === k).map((e) => (
                <Link key={e.id} to={testHref(e.slug)} className="gk-test-card">
                  <span className="gk-test-kind">{kindInfo[e.kind]?.icon} {e.kind}{e.year ? ` · ${e.year}` : ''}<span className={e.price > 0 ? 'gk-price' : 'gk-price free'}>{e.price > 0 ? `₹${e.price}` : 'Free'}</span></span>
                  <h3>{e.title}</h3>
                  <span className="gk-bilingual">English + हिंदी</span>
                  <p>{e.description}</p>
                  <div className="gk-test-stats">
                    <span>❓ {e.questionCount} questions</span>
                    <span>🏆 {Number(e.maxMarks)} marks</span>
                    <span>⏱ {minutesLabel(e.durationMinutes)}</span>
                    {e.attempts > 0 && <span>👥 {e.attempts} attempts</span>}
                  </div>
                </Link>
              ))}
            </div>
          </section>
        ))}
      </div>
    </>
  );
}
