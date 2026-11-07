import { Link, Navigate, Route, Routes, useParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { usePrerenderHandoff } from '../../lib/prerender.js';
import Markdown from '../../shared/Markdown.jsx';
import { LangSwitch, useLang } from '../components/QuestionCard.jsx';
import PracticeQuestion from '../components/PracticeQuestion.jsx';
import Game from '../games/Game.jsx';

// The CBSE section (/cbse/class-6/...: Mathematics, English, Personality Development). Content comes from the chapter files via /api/cbse. The server renders the
// same pages for search engines (MinnieWorld.Web/Services/SpaHost.Cbse.cs), so keep the wording of the two in step.

export const CBSE_HUB = '/cbse';
// The classes offered (keep in step with CbseCatalog.Classes on the server). With one class, /cbse opens it.
const CLASSES = [6];
const classPath = (n) => `${CBSE_HUB}/class-${n}`;
export const CBSE_ENTRY = CLASSES.length === 1 ? classPath(CLASSES[0]) : CBSE_HUB;
const parseClass = (seg) => { const m = /^class-(\d+)$/.exec(seg ?? ''); const n = m ? Number(m[1]) : 0; return CLASSES.includes(n) ? n : null; };

export function CbseRoutes() {
  return (
    <Routes>
      <Route index element={CLASSES.length === 1 ? <Navigate to={CBSE_ENTRY} replace /> : <CbseHome />} />
      <Route path=":cls" element={<CbseClass />} />
      <Route path=":cls/:subject" element={<CbseSubject />} />
      <Route path=":cls/:subject/:slug" element={<CbseChapter />} />
      <Route path="*" element={<NotFound />} />
    </Routes>
  );
}

function NotFound() {
  usePrerenderHandoff(true);
  return <div className="gk-wrap gk-empty"><h1>Page not found</h1><Link to={CBSE_HUB}>All CBSE classes</Link></div>;
}

function Crumbs({ items }) {
  return (
    <nav className="gk-crumbs" aria-label="Breadcrumb">
      <Link to="/gk">Curious Minds</Link>
      {items.map(([label, to]) => <span key={label}> › {to ? <Link to={to}>{label}</Link> : label}</span>)}
    </nav>
  );
}

const chapterCount = (count) => (count > 0 ? `${count} chapter${count === 1 ? '' : 's'}` : 'Coming soon');

function CbseHome() {
  useTitle('CBSE Notes, Games & Practice (English + Hindi)');
  const { data, loading, error } = useAsync(() => api.cbse.classes(), []);
  usePrerenderHandoff(!loading);
  return (
    <>
      <section className="gk-tests-hero">
        <div className="gk-wrap">
          <Crumbs items={[['CBSE']]} />
          <span className="gk-tag">Free · No login needed</span>
          <h1 className="gk-page-title">CBSE: notes, games and practice</h1>
          <p className="gk-lead">Short concept notes, learning games and practice questions with solutions for every chapter, in English and Hindi. Free, and no login needed.</p>
          <p className="gk-lead" lang="hi">हर अध्याय के लिए छोटे नोट्स, सीखने वाले खेल और हल सहित अभ्यास प्रश्न, अंग्रेज़ी और हिंदी में। मुफ़्त, लॉगिन की ज़रूरत नहीं।</p>
        </div>
      </section>
      <div className="gk-wrap gk-section">
        {loading && <p className="gk-muted">Loading…</p>}
        {error && <p className="gk-err">{error.message}</p>}
        <div className="gk-test-grid">
          {data?.classes.map((c) => {
            const count = c.subjects.reduce((sum, s) => sum + s.chapters, 0);
            return (
              <Link key={c.class} to={classPath(c.class)} className="gk-test-card">
                <span className="gk-test-kind">CBSE</span>
                <h3>Class {c.class}</h3>
                <p>{c.subjects.map((s) => s.name).join(', ')}</p>
                <span className="gk-test-stats">{chapterCount(count)}</span>
              </Link>
            );
          })}
        </div>
      </div>
    </>
  );
}

function CbseClass() {
  const { cls } = useParams();
  const n = parseClass(cls);
  useTitle(n ? `CBSE Class ${n} Maths, English & Personality Development` : 'Page not found');
  const { data, loading } = useAsync(() => (n ? api.cbse.classes() : Promise.resolve(null)), [n]);
  usePrerenderHandoff(!loading);
  if (!n) return <NotFound />;
  const subjects = data?.classes.find((c) => c.class === n)?.subjects ?? [];
  return (
    <div className="gk-wrap gk-section">
      <Crumbs items={[[`CBSE Class ${n}`]]} />
      <h1 className="gk-page-title">CBSE Class {n}: notes, games and practice</h1>
      <p className="gk-lead">Short concept notes, learning games and practice questions with solutions, in English and Hindi. Free, and no login needed.</p>
      <p className="gk-lead" lang="hi">छोटे नोट्स, सीखने वाले खेल और हल सहित अभ्यास प्रश्न, अंग्रेज़ी और हिंदी में। मुफ़्त, लॉगिन की ज़रूरत नहीं।</p>
      {loading && <p className="gk-muted">Loading…</p>}
      <div className="gk-test-grid">
        {subjects.map((s) => (
          <Link key={s.key} to={`${classPath(n)}/${s.key}`} className="gk-test-card">
            <span className="gk-test-kind">Class {n}</span>
            <h3>{s.name} <small lang="hi">({s.nameHi})</small></h3>
            <span className="gk-test-stats">{chapterCount(s.chapters)}</span>
          </Link>
        ))}
      </div>
    </div>
  );
}

function CbseSubject() {
  const { cls, subject } = useParams();
  const n = parseClass(cls);
  const { data, loading, error } = useAsync(() => (n ? api.cbse.subject(n, subject) : Promise.reject(new Error('Not found'))), [n, subject]);
  useTitle(data ? `CBSE Class ${n} ${data.subject.name}: Chapter Notes, Games & Practice` : 'CBSE | Curious Minds');
  usePrerenderHandoff(!loading);
  if (!n || error) return <NotFound />;
  if (!data) return <div className="gk-wrap gk-empty">Loading…</div>;
  return (
    <div className="gk-wrap gk-section">
      <Crumbs items={[[`CBSE Class ${n}`, classPath(n)], [data.subject.name]]} />
      <h1 className="gk-page-title">CBSE Class {n} {data.subject.name} <span lang="hi">({data.subject.nameHi})</span>: chapters</h1>
      {data.chapters.length === 0 && <p className="gk-empty">Chapters are on their way. Check back soon.</p>}
      <ol className="gk-steps">
        {data.chapters.map((c) => (
          <li key={c.slug}>
            <Link to={c.path}>
              <span className="gk-step-num">{c.number}</span>
              <span className="gk-step-body"><b>Chapter {c.number}: {c.title}</b>{c.titleHi && <small lang="hi">{c.titleHi}</small>}<small>{c.summary}</small></span>
            </Link>
          </li>
        ))}
      </ol>
    </div>
  );
}

/** Turns a chapter question into the shape QuestionCard draws (the case text is shown as its passage). */
const toCard = (q, i) => ({
  ...q,
  id: `cq${i}`,
  section: q.kind === 'case' ? 'Case-based question' : 'Practice question',
  optionsHi: q.optionsHi?.length === 4 ? q.optionsHi : null,
  passage: q.kind === 'case' ? q.case : null,
  passageHi: q.kind === 'case' ? q.caseHi : null
});

function CbseChapter() {
  const { cls, subject, slug } = useParams();
  const n = parseClass(cls);
  const { data, loading, error } = useAsync(() => (n ? api.cbse.chapter(n, subject, slug) : Promise.reject(new Error('Not found'))), [n, subject, slug]);
  const [lang, setLang] = useLang();
  const c = data?.chapter;
  const s = data?.subject;
  const heading = c && `Class ${c.class} ${s.name} Chapter ${c.number}: ${c.title} – Notes, Games & Practice`;
  useTitle(c ? `Class ${c.class} ${s.name} Ch ${c.number}: ${c.title} | Curious Minds` : 'CBSE | Curious Minds');
  usePrerenderHandoff(!loading);

  if (!n || error) return <NotFound />;
  if (!c) return <div className="gk-wrap gk-empty">Loading…</div>;
  const showEn = lang !== 'hi' || !c.notesHi;
  const showHi = lang !== 'en' && c.notesHi;

  return (
    <article className="gk-wrap gk-section gk-cbse">
      <Crumbs items={[[`CBSE Class ${c.class}`, classPath(c.class)], [s.name, `${classPath(c.class)}/${s.key}`], [`Chapter ${c.number}`]]} />
      <div className="gk-result-head">
        <h1 className="gk-cbse-title">{heading}</h1>
        <LangSwitch lang={lang} setLang={setLang} />
      </div>
      {c.titleHi && <p lang="hi"><strong>{c.titleHi}</strong></p>}
      <p className="gk-post-dek">{c.summary}</p>
      {c.summaryHi && <p className="gk-post-dek" lang="hi">{c.summaryHi}</p>}
      {c.book && <p className="gk-meta">Textbook: {c.book}</p>}

      <nav className="gk-cbse-jump" aria-label="On this page">
        <a href="#notes">Notes</a>{c.games.length > 0 && <a href="#games">Games</a>}{c.questions.length > 0 && <a href="#practice">Practice</a>}{c.faqs.length > 0 && <a href="#faqs">FAQs</a>}
      </nav>

      <section id="notes" aria-labelledby="notes-h">
        <h2 id="notes-h">Concept notes</h2>
        {showEn && <Markdown className="gk-prose">{c.notes}</Markdown>}
        {showHi && <div lang="hi"><h3>हिंदी में नोट्स</h3><Markdown className="gk-prose">{c.notesHi}</Markdown></div>}
      </section>

      {c.games.length > 0 && (
        <section id="games" aria-labelledby="games-h">
          <h2 id="games-h">Learning games</h2>
          {c.games.map((g) => <Game key={g.id} game={g} lang={lang} storageKey={`${data.path}#${g.id}`} />)}
        </section>
      )}

      {c.questions.length > 0 && (
        <section id="practice" aria-labelledby="practice-h">
          <h2 id="practice-h">{c.questions.length} practice questions with solutions</h2>
          <ol className="gk-review-list">{c.questions.map((q, i) => <PracticeQuestion key={i} q={toCard(q, i)} number={i + 1} lang={lang} />)}</ol>
        </section>
      )}

      {c.faqs.length > 0 && (
        <section id="faqs" aria-labelledby="faqs-h" className="gk-faqs">
          <h2 id="faqs-h">Frequently asked questions</h2>
          {c.faqs.map((f) => (
            <div key={f.q}>
              {lang !== 'hi' && <><h3>{f.q}</h3><p>{f.a}</p></>}
              {lang !== 'en' && f.qHi && <p lang="hi"><strong>{f.qHi}</strong></p>}
              {lang !== 'en' && f.aHi && <p lang="hi">{f.aHi}</p>}
            </div>
          ))}
        </section>
      )}

      <nav className="gk-lesson-nav" aria-label="Chapters">
        {data.previous ? <Link to={data.previous.path} rel="prev">← Chapter {data.previous.number}: {data.previous.title}</Link> : <span />}
        <Link to={`${classPath(c.class)}/${s.key}`}>All Class {c.class} {s.name} chapters</Link>
        {data.next ? <Link to={data.next.path} rel="next">Chapter {data.next.number}: {data.next.title} →</Link> : <span />}
      </nav>
    </article>
  );
}
