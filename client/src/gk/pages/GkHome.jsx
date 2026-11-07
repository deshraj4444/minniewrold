import { audiences, examGroups, examHref } from '../exams.js';
import { useState } from 'react';
import { Link } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { BLOG } from '../config.js';
import { postHref } from '../jee.js';

const blog = api.blog(BLOG);

const facts = [
  'Honey never spoils: edible honey has been found in ancient Egyptian tombs.',
  'Octopuses have three hearts and blue blood.',
  'India has 22 languages in the Eighth Schedule of its Constitution.',
  'A day on Venus is longer than a year on Venus.',
  'The Great Wall of China is not visible to the naked eye from space.',
  'Bananas are berries, but strawberries are not.',
  'Lightning is about five times hotter than the surface of the Sun.'
];

const quiz = [
  { q: 'Who was the chairman of the Drafting Committee of the Indian Constitution?', options: ['Jawaharlal Nehru', 'Dr B. R. Ambedkar', 'Dr Rajendra Prasad', 'Sardar Patel'], answer: 1 },
  { q: 'Which is the hottest planet in our Solar System?', options: ['Mercury', 'Mars', 'Venus', 'Jupiter'], answer: 2 },
  { q: 'India\'s national aquatic animal is the…', options: ['Olive ridley turtle', 'Ganges river dolphin', 'Blue whale', 'Mahseer'], answer: 1 }
];

/** The four entry points: one per kind of learner. */
const paths = [
  { to: '/gk/class-6', icon: '🎒', color: '#0f766e', title: 'Below Class 6', titleHi: 'कक्षा 6 से नीचे', cta: 'Class 6 admission tests',
    text: 'Class 5 students: Navodaya, Sainik School and Military School Class 6 entrance practice, with certificates and a quiz game.' },
  { to: '/cbse/class-6', icon: '📘', color: '#1d4ed8', title: 'CBSE Class 6', titleHi: 'सीबीएसई कक्षा 6', cta: 'Notes, games & practice',
    text: 'Mathematics, English and Personality Development: notes, learning games and practice with solutions. No login needed.' },
  { to: '/jee', icon: '⚛️', color: '#7c2d12', title: 'IIT JEE', titleHi: 'आईआईटी जेईई', cta: 'Chapter tests & notes',
    text: 'JEE Main and Advanced chapter notes and timed tests with +4 / −1 marking and full solutions.' },
  { to: '/gk/teachers', icon: '👩‍🏫', color: '#6d3fc0', title: 'Teachers: TET & TGT', titleHi: 'शिक्षक: टेट और टीजीटी', cta: 'Mock tests & PYQs',
    text: 'HP TET (TGT Arts, JBT) and CTET mock tests and previous year papers on the real exam pattern.' }
];

export default function GkHome() {
  useTitle('Curious Minds | HP TET, CTET & Class 6 Mock Tests in English + Hindi');
  const posts = useAsync(() => blog.posts({ pageSize: 7 }), []);
  const courses = useAsync(() => blog.courses(), []);
  const categories = useAsync(() => blog.categories(), []);
  const exams = useAsync(() => api.exams.list(), []);
  const [factIndex, setFactIndex] = useState(() => Math.floor(Math.random() * facts.length));
  const counts = {};
  for (const e of exams.data ?? []) counts[e.category] = (counts[e.category] ?? 0) + 1;

  const [lead, ...more] = posts.data?.items ?? [];

  return (
    <>
      <section className="gk-hero">
        <div className="gk-wrap gk-hero-grid">
          <div>
            <span className="gk-eyebrow">Below Class 6 · CBSE Class 6 · IIT JEE · Teachers</span>
            <h1>Learn, practise and <mark>get exam-ready</mark></h1>
            <p>From Class 6 admission tests to CBSE chapters, IIT JEE and teacher eligibility exams: notes, games, timed mock tests and clear explanations, in English and हिंदी.</p>
            <div className="gk-hero-cta">
              <a href="#paths" className="gk-btn">Choose your path</a>
              <Link to="/gk/tests" className="gk-btn gk-btn-outline">All mock tests</Link>
            </div>
            <ul className="gk-hero-points">
              <li>Bilingual questions</li>
              <li>Section-wise analysis</li>
              <li>Answer explanations</li>
            </ul>
          </div>
          <div className="gk-fact" aria-live="polite">
            <span className="gk-fact-label">Did you know?</span>
            <p>{facts[factIndex]}</p>
            <button onClick={() => setFactIndex((i) => (i + 1) % facts.length)}>Next fact →</button>
          </div>
        </div>
      </section>

      <Stats exams={exams.data} />

      <section id="paths" className="gk-wrap gk-section gk-paths-section" aria-labelledby="paths-h">
        <div className="gk-section-head"><h2 id="paths-h">Who are you preparing for? <small lang="hi">आप किसकी तैयारी कर रहे हैं?</small></h2></div>
        <div className="gk-paths">
          {paths.map((p) => (
            <Link key={p.to} to={p.to} className="gk-path" style={{ '--tile': p.color }}>
              <span className="gk-path-icon" aria-hidden="true">{p.icon}</span>
              <b>{p.title}</b>
              <small lang="hi">{p.titleHi}</small>
              <span className="gk-path-text">{p.text}</span>
              <span className="gk-path-go">{p.cta} →</span>
            </Link>
          ))}
        </div>
      </section>

      <section className="gk-wrap gk-section">
        <div className="gk-section-head"><h2>Choose your exam <small lang="hi">अपनी परीक्षा चुनें</small></h2><Link to="/gk/tests">All tests →</Link></div>
        <div className="gk-home-exam-grid">
          {examGroups.map((g) => (
            <Link key={g.key} to={examHref(g)} className="gk-home-exam" style={{ '--tile': g.color }}>
              <span className="gk-home-exam-aud">{audiences.find((a) => a.key === g.audience)?.name}</span>
              <b>{g.name}</b>
              <small lang="hi">{g.nameHi}</small>
              <span className="gk-home-exam-meta">{(counts[g.category] ?? 0) > 0 ? `${counts[g.category]} papers` : 'Coming soon'} · {g.pattern.split(' · ').slice(0, 2).join(' · ')}</span>
            </Link>
          ))}
        </div>
      </section>

      <section className="gk-band">
        <div className="gk-wrap gk-section">
          <div className="gk-section-head"><h2>How it works</h2></div>
          <ol className="gk-how">
            <li><b>Pick a paper</b><span>Previous year papers, full mocks or short sectional tests for your exam.</span></li>
            <li><b>Attempt it on the clock</b><span>Same number of questions and time as the real exam, with a question palette and mark-for-review.</span></li>
            <li><b>Review and improve</b><span>Score, section-wise accuracy and an explanation for every answer, in English and Hindi.</span></li>
          </ol>
        </div>
      </section>

      {categories.data?.posts.length > 0 && (
        <div className="gk-wrap gk-cats">
          {categories.data.posts.map((c) => (
            <Link key={c} to={`/gk/posts?category=${encodeURIComponent(c)}`} className="gk-cat-pill">{c}</Link>
          ))}
        </div>
      )}

      <section className="gk-wrap gk-section">
        <div className="gk-section-head"><h2>Latest study articles</h2><Link to="/gk/posts">See all →</Link></div>
        {posts.loading && <p className="gk-muted">Loading…</p>}
        <div className="gk-mag">
          {lead && (
            <Link to={postHref(lead.slug)} className="gk-tile gk-tile-lead">
              {lead.coverImageUrl && <img src={lead.coverImageUrl} alt="" />}
              <div className="gk-tile-body">
                <span className="gk-tag">{lead.category}</span>
                <h3>{lead.title}</h3>
                <p>{lead.excerpt}</p>
                <span className="gk-meta">{lead.readMinutes} min read</span>
              </div>
            </Link>
          )}
          {more.slice(0, 4).map((p) => (
            <Link key={p.id} to={postHref(p.slug)} className="gk-tile">
              {p.coverImageUrl && <img src={p.coverImageUrl} alt="" loading="lazy" />}
              <div className="gk-tile-body">
                <span className="gk-tag">{p.category}</span>
                <h3>{p.title}</h3>
                <span className="gk-meta">{p.readMinutes} min read</span>
              </div>
            </Link>
          ))}
        </div>
      </section>

      {courses.data?.length > 0 && (
        <section className="gk-band">
          <div className="gk-wrap gk-section">
            <div className="gk-section-head"><h2>Courses</h2><Link to="/gk/courses">All courses →</Link></div>
            <div className="gk-course-grid">
              {courses.data.slice(0, 3).map((c) => <CourseCard key={c.id} c={c} />)}
            </div>
          </div>
        </section>
      )}

      <section className="gk-wrap gk-section">
        <div className="gk-section-head"><h2>Quick quiz</h2></div>
        <Quiz />
      </section>
    </>
  );
}

function Stats({ exams }) {
  if (!exams?.length) return null;
  const free = exams.filter((e) => e.price === 0).length;
  const examCount = new Set(exams.map((e) => e.category)).size;
  const items = [
    [exams.length, 'Papers & mock tests'],
    [free, 'Free to attempt'],
    [examCount, 'Exams covered'],
    ['2', 'Languages: English + हिंदी']
  ];
  return (
    <div className="gk-wrap">
      <dl className="gk-stats">
        {items.map(([n, label]) => <div key={label}><dt>{label}</dt><dd>{n}</dd></div>)}
      </dl>
    </div>
  );
}

export function CourseCard({ c }) {
  return (
    <Link to={`/gk/courses/${c.slug}`} className="gk-course">
      {c.thumbnailUrl && <img src={c.thumbnailUrl} alt="" loading="lazy" />}
      <div className="gk-course-body">
        <div className="gk-course-tags"><span className="gk-tag">{c.category}</span><span className="gk-level">{c.level}</span></div>
        <h3>{c.title}</h3>
        <p>{c.subtitle}</p>
        <div className="gk-course-stats"><span>📘 {c.lessonCount} lessons</span><span>⏱ {c.totalMinutes} min</span></div>
      </div>
    </Link>
  );
}

function Quiz() {
  const [answers, setAnswers] = useState({});
  const score = quiz.filter((item, i) => answers[i] === item.answer).length;
  const done = Object.keys(answers).length === quiz.length;

  return (
    <div className="gk-quiz">
      {quiz.map((item, i) => (
        <fieldset key={item.q} className="gk-quiz-q">
          <legend><span>Q{i + 1}.</span> {item.q}</legend>
          <div className="gk-quiz-options">
            {item.options.map((opt, j) => {
              const picked = answers[i] === j;
              const reveal = answers[i] !== undefined;
              const cls = reveal && j === item.answer ? 'correct' : picked ? 'wrong' : '';
              return (
                <button key={opt} type="button" className={cls} disabled={reveal} aria-pressed={picked} onClick={() => setAnswers((a) => ({ ...a, [i]: j }))}>
                  {opt}
                </button>
              );
            })}
          </div>
        </fieldset>
      ))}
      {done && (
        <div className="gk-quiz-result" role="status">
          You scored <b>{score}/{quiz.length}</b>. {score === quiz.length ? 'Brilliant! 🎉' : 'Keep exploring to learn more.'}
          <button type="button" onClick={() => setAnswers({})}>Try again</button>
        </div>
      )}
    </div>
  );
}
