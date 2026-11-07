import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { Link, useParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useTitle } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import ShareButtons from '../../shared/ShareButtons.jsx';
import { useStudent } from '../student.jsx';
import QuestionCard, { LangSwitch, sectionLabel, useLang } from '../components/QuestionCard.jsx';
import Confetti from '../components/Confetti.jsx';
import Fireworks from '../components/Fireworks.jsx';
import GoogleReview from '../components/GoogleReview.jsx';
import { SoundToggle } from '../games/common.jsx';
import { playClick } from '../../lib/sound.js';
import { levelInfo } from './GkCertificate.jsx';
import { groupOf, isKids, examHref } from '../exams.js';
import { jeeCompanion, jeePageFor, testHref } from '../jee.js';

/** Loads an attempt and shows the exam screen while it runs, or the result once submitted. */
export default function GkAttempt() {
  const { id } = useParams();
  const { student } = useStudent();
  const [attempt, setAttempt] = useState(null);
  const [error, setError] = useState('');

  useEffect(() => {
    if (!student) return;
    api.exams.attempt(id).then(setAttempt).catch((e) => setError(e.status === 404 ? 'This attempt was not found.' : e.message));
  }, [id, student]);

  if (student === null) {
    return <div className="gk-wrap gk-empty"><h1>Please log in</h1><Link className="gk-btn" to={`/gk/login?next=/gk/attempt/${id}`}>Log in</Link></div>;
  }
  if (error) return <div className="gk-wrap gk-empty"><h1>{error}</h1><Link to="/gk/tests">All tests</Link></div>;
  if (!attempt) return <div className="gk-wrap gk-empty">Loading…</div>;

  return attempt.status === 'submitted'
    ? <Result result={attempt} />
    : <ExamScreen attempt={attempt} onFinished={setAttempt} />;
}

function ExamScreen({ attempt, onFinished }) {
  useTitle(`${attempt.exam.title} | Test in progress`);
  const questions = attempt.questions;
  const [answers, setAnswers] = useState(() => ({ ...attempt.answers }));
  const [index, setIndex] = useState(0);
  const reviewKey = `gk.review.${attempt.id}`;
  const [review, setReview] = useState(() => {
    try { return new Set(JSON.parse(localStorage.getItem(reviewKey) ?? '[]')); } catch { return new Set(); }
  });
  const [visited, setVisited] = useState(() => new Set([questions[0]?.id]));
  const [saveState, setSaveState] = useState('saved');
  const [confirming, setConfirming] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [paletteOpen, setPaletteOpen] = useState(false);
  const [lang, setLang] = useLang();

  // The server's clock decides when time is up; this offset corrects for a wrong clock on the student's device.
  const offset = useMemo(() => Date.parse(attempt.serverNow) - Date.now(), [attempt.serverNow]);
  const endsAt = Date.parse(attempt.endsAt);
  const [remaining, setRemaining] = useState(() => endsAt - (Date.now() + offset));

  const answersRef = useRef(answers);
  answersRef.current = answers;
  const dirty = useRef(false);

  const submit = useCallback(async () => {
    setSubmitting(true);
    try {
      onFinished(await api.exams.submit(attempt.id, answersRef.current));
      try { localStorage.removeItem(reviewKey); } catch { /* storage unavailable */ }
      window.scrollTo(0, 0);
    } catch (e) {
      setSubmitting(false);
      setConfirming(false);
      alert(`Could not submit: ${e.message}. Please check your connection and try again.`);
    }
  }, [attempt.id, onFinished, reviewKey]);

  // Clock; submits automatically at zero.
  useEffect(() => {
    const t = setInterval(() => {
      const left = endsAt - (Date.now() + offset);
      setRemaining(left);
      if (left <= 0) { clearInterval(t); submit(); }
    }, 1000);
    return () => clearInterval(t);
  }, [endsAt, offset, submit]);

  // Autosave every few seconds when something changed.
  useEffect(() => {
    const t = setInterval(async () => {
      if (!dirty.current) return;
      dirty.current = false;
      setSaveState('saving');
      try {
        await api.exams.save(attempt.id, answersRef.current);
        setSaveState('saved');
      } catch {
        dirty.current = true;
        setSaveState('offline');
      }
    }, 4000);
    return () => clearInterval(t);
  }, [attempt.id]);

  // Warn before leaving with unsaved answers.
  useEffect(() => {
    const warn = (e) => { if (dirty.current) { e.preventDefault(); e.returnValue = ''; } };
    window.addEventListener('beforeunload', warn);
    return () => window.removeEventListener('beforeunload', warn);
  }, []);

  useEffect(() => {
    try { localStorage.setItem(reviewKey, JSON.stringify([...review])); } catch { /* storage unavailable */ }
  }, [review, reviewKey]);

  const q = questions[index];
  const sections = useMemo(() => {
    const list = [];
    questions.forEach((x, i) => {
      const name = x.section || 'General';
      if (!list.length || list[list.length - 1].name !== name) list.push({ name, start: i });
    });
    return list;
  }, [questions]);

  function go(i) {
    const next = Math.max(0, Math.min(questions.length - 1, i));
    setIndex(next);
    setVisited((v) => new Set(v).add(questions[next].id));
    setPaletteOpen(false);
    document.querySelector('.gk-exam-question')?.scrollIntoView({ block: 'start', behavior: 'smooth' });
  }

  function choose(option) {
    // A neutral tick only: a right/wrong sound here would give the answer away during the test.
    playClick();
    setAnswers((a) => ({ ...a, [q.id]: option }));
    dirty.current = true;
    setSaveState('unsaved');
  }

  function clear() {
    setAnswers((a) => { const n = { ...a }; delete n[q.id]; return n; });
    dirty.current = true;
    setSaveState('unsaved');
  }

  function toggleReview() {
    setReview((r) => { const n = new Set(r); n.has(q.id) ? n.delete(q.id) : n.add(q.id); return n; });
  }

  const answeredCount = Object.keys(answers).length;
  const statusOf = (x) => {
    const a = answers[x.id] !== undefined;
    if (review.has(x.id)) return a ? 'review answered' : 'review';
    if (a) return 'answered';
    return visited.has(x.id) ? 'skipped' : '';
  };
  const low = remaining < 5 * 60 * 1000;
  const currentSection = [...sections].reverse().find((s) => s.start <= index);

  return (
    <div className="gk-exam">
      <div className="gk-exam-bar">
        <div className="gk-wrap gk-exam-bar-inner">
          <div className="gk-exam-title">
            <b>{attempt.exam.title}</b>
            <small className={`gk-save ${saveState}`}>{{ saved: 'All answers saved', saving: 'Saving…', unsaved: 'Saving soon…', offline: 'Offline: will retry' }[saveState]}</small>
          </div>
          <div className={low ? 'gk-timer low' : 'gk-timer'} role="timer" aria-live={low ? 'polite' : 'off'}>⏱ {clock(remaining)}</div>
          <button className="gk-btn" onClick={() => setConfirming(true)} disabled={submitting}>Submit</button>
        </div>
        {sections.length > 1 && (
          <div className="gk-wrap gk-exam-sections">
            {sections.map((s) => (
              <button key={s.name} className={currentSection?.name === s.name ? 'active' : ''} onClick={() => go(s.start)}>{sectionLabel(s.name)}</button>
            ))}
          </div>
        )}
      </div>

      <div className="gk-wrap gk-exam-layout">
        <section className="gk-exam-question" aria-labelledby="q-text">
          <div className="gk-exam-qhead">
            <span>Question {index + 1} of {questions.length}</span>
            <span className="gk-qhead-tools"><SoundToggle /><LangSwitch lang={lang} setLang={setLang} /></span>
          </div>
          <QuestionCard q={q} number={index + 1} lang={lang} chosen={answers[q.id]} onChoose={choose} />
          <div className="gk-exam-actions">
            <button className="gk-btn gk-btn-outline" onClick={() => go(index - 1)} disabled={index === 0}>← Previous</button>
            <button className="gk-link" onClick={clear} disabled={answers[q.id] === undefined}>Clear answer</button>
            <button className={review.has(q.id) ? 'gk-review-btn on' : 'gk-review-btn'} onClick={toggleReview} aria-pressed={review.has(q.id)}>⚑ {review.has(q.id) ? 'Marked for review' : 'Mark for review'}</button>
            {index < questions.length - 1
              ? <button className="gk-btn" onClick={() => go(index + 1)}>Save & next →</button>
              : <button className="gk-btn" onClick={() => setConfirming(true)}>Finish</button>}
          </div>
        </section>

        <aside className={paletteOpen ? 'gk-palette open' : 'gk-palette'}>
          <button className="gk-palette-toggle" onClick={() => setPaletteOpen((o) => !o)} aria-expanded={paletteOpen}>
            Question palette · {answeredCount}/{questions.length} answered
          </button>
          <div className="gk-palette-body">
            <div className="gk-legend">
              <span><i className="answered" />Answered</span>
              <span><i className="skipped" />Not answered</span>
              <span><i className="review" />For review</span>
              <span><i />Not visited</span>
            </div>
            {sections.map((s, si) => {
              const end = sections[si + 1]?.start ?? questions.length;
              return (
                <div key={s.name}>
                  {sections.length > 1 && <h4>{sectionLabel(s.name)}</h4>}
                  <div className="gk-palette-grid">
                    {questions.slice(s.start, end).map((x, j) => {
                      const i = s.start + j;
                      return <button key={x.id} className={`${statusOf(x)} ${i === index ? 'current' : ''}`} onClick={() => go(i)} aria-label={`Question ${i + 1}`}>{i + 1}</button>;
                    })}
                  </div>
                </div>
              );
            })}
          </div>
        </aside>
      </div>

      {confirming && (
        <div className="gk-modal" role="dialog" aria-modal="true" aria-labelledby="confirm-title">
          <div className="gk-modal-box">
            <h2 id="confirm-title">Submit the test?</h2>
            <ul>
              <li>Answered: <b>{answeredCount}</b></li>
              <li>Not answered: <b>{questions.length - answeredCount}</b></li>
              <li>Marked for review: <b>{review.size}</b></li>
              <li>Time left: <b>{clock(remaining)}</b></li>
            </ul>
            <p className="gk-muted">You can't change your answers after submitting.</p>
            <div className="gk-modal-actions">
              <button className="gk-btn gk-btn-outline" onClick={() => setConfirming(false)} disabled={submitting}>Keep going</button>
              <button className="gk-btn" onClick={submit} disabled={submitting}>{submitting ? 'Submitting…' : 'Submit now'}</button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

function Result({ result }) {
  const { student } = useStudent();
  useTitle(`Result: ${result.exam.title} | Curious Minds`);
  const [filter, setFilter] = useState('all');
  const [lang, setLang] = useLang();
  const qs = result.questions.map((q, i) => ({ ...q, n: i + 1, status: q.chosen == null ? 'skipped' : q.chosen === q.answer ? 'correct' : 'wrong' }));
  const shown = filter === 'all' ? qs : qs.filter((q) => q.status === filter);
  const minutes = Math.floor(result.timeTakenSeconds / 60);
  // A JEE chapter test links to its chapter's notes.
  const jeePage = jeePageFor('test', result.exam.slug);
  const notes = jeePage && jeeCompanion(jeePage);
  const accuracy = result.correct + result.wrong ? Math.round((result.correct * 100) / (result.correct + result.wrong)) : 0;

  return (
    <div className="gk-wrap gk-section gk-result">
      <nav className="gk-crumbs"><Link to="/gk/tests">Mock tests</Link> › <Link to={examHref(groupOf(result.exam.category))}>{groupOf(result.exam.category).name}</Link> › <Link to={testHref(result.exam.slug)}>{result.exam.title}</Link> › Result</nav>

      <div className={result.passed ? 'gk-score-card pass' : 'gk-score-card'}>
        <div className="gk-score-main">
          <span>Your score</span>
          <b>{Number(result.score)}<small>/{result.maxScore}</small></b>
          <span className="gk-score-verdict">{result.passed ? `🎉 Qualified (${result.passPercent}% needed)` : `Needs ${result.passPercent}% to qualify. Keep practising!`}</span>
        </div>
        <div className="gk-score-stats">
          <div><b>{result.percent}%</b><span>Percentage</span></div>
          <div><b>#{result.rank}</b><span>Rank of {result.candidates}</span></div>
          <div><b>{result.percentile}</b><span>Percentile</span></div>
          <div><b>{accuracy}%</b><span>Accuracy</span></div>
          <div><b className="ok">{result.correct}</b><span>Correct</span></div>
          <div><b className="bad">{result.wrong}</b><span>Wrong</span></div>
          <div><b>{result.unanswered}</b><span>Skipped</span></div>
          <div><b>{minutes}m {result.timeTakenSeconds % 60}s</b><span>Time taken</span></div>
        </div>
      </div>

      {result.passed && (
        <div className="gk-congrats" role="status">
          <Fireworks />
          <span aria-hidden="true">🎆</span>
          <div>
            <h2>Congratulations{student?.fullName ? `, ${student.fullName.split(' ')[0]}` : ''}! You qualified.</h2>
            <p lang="hi">बधाई हो! आपने यह टेस्ट उत्तीर्ण कर लिया है।</p>
          </div>
        </div>
      )}
      <Reward result={result} />

      <h2>Section-wise analysis</h2>
      <div className="gk-sections">
        {result.sections.map((s) => {
          const pct = Math.round((s.correct * 100) / s.total);
          return (
            <div key={s.section} className="gk-section-score">
              <div className="gk-section-score-head"><b>{sectionLabel(s.section)}</b><span>{s.correct}/{s.total}{Number(s.maxMarks) !== s.total ? ` · ${Number(s.marks)}/${Number(s.maxMarks)} marks` : ''}</span></div>
              <div className="gk-meter"><span style={{ width: `${pct}%` }} /></div>
              <small>{s.correct} correct · {s.wrong} wrong · {s.unanswered} skipped</small>
            </div>
          );
        })}
      </div>

      <div className="gk-result-actions">
        <Link to={testHref(result.exam.slug)} className="gk-btn">Retake test</Link>
        <Link to={examHref(groupOf(result.exam.category))} className="gk-btn gk-btn-outline">More papers</Link>
        {notes && <Link to={notes.path} className="gk-btn gk-btn-outline">Revise: {notes.title}</Link>}
        <Link to="/gk/my-tests" className="gk-link">All my results</Link>
        <GoogleReview />
        <ShareButtons title={`I scored ${Number(result.score)}/${result.maxScore} in ${result.exam.title}. Try it free!`} url={`${window.location.origin}${testHref(result.exam.slug)}`} />
      </div>

      <div className="gk-result-head"><h2>Answers & explanations / उत्तर एवं व्याख्या</h2><LangSwitch lang={lang} setLang={setLang} /></div>
      <div className="gk-tabs" role="tablist">
        {[['all', `All (${qs.length})`], ['wrong', `Wrong (${result.wrong})`], ['skipped', `Skipped (${result.unanswered})`], ['correct', `Correct (${result.correct})`]].map(([k, label]) => (
          <button key={k} role="tab" aria-selected={filter === k} className={filter === k ? 'active' : ''} onClick={() => setFilter(k)}>{label}</button>
        ))}
      </div>
      <ol className="gk-review-list">
        {shown.map((q) => (
          <li key={q.id} className={q.status}>
            <div className="gk-review-head"><span className={`gk-badge ${q.status}`}>{{ correct: '✓ Correct / सही', wrong: '✗ Wrong / गलत', skipped: 'Not answered / अनुत्तरित' }[q.status]}</span></div>
            <QuestionCard q={q} number={q.n} lang={lang} chosen={q.chosen} reveal />
          </li>
        ))}
      </ol>
      <p className="gk-muted">Submitted {formatDate(result.submittedAt, { day: 'numeric', month: 'long', year: 'numeric', hour: 'numeric', minute: '2-digit' })}.</p>
    </div>
  );
}

/** Stars, confetti and the certificate link for a pass; a friendly nudge and progress bar otherwise. */
function Reward({ result }) {
  const kids = isKids(result.exam.category);
  const group = groupOf(result.exam.category);
  if (result.certificate) {
    const level = levelInfo[result.level] ?? levelInfo.Bronze;
    return (
      <div className={`gk-reward ${result.level.toLowerCase()}`}>
        <Confetti />
        <div className="gk-play-stars" aria-label={`${level.stars} of 3 stars`}>{[0, 1, 2].map((i) => <span key={i} className={i < level.stars ? 'on' : ''} style={{ animationDelay: `${i * 0.25}s` }}>★</span>)}</div>
        <div>
          <h2>{level.medal} {kids ? 'Hurray! ' : ''}You earned a {result.level} certificate!</h2>
          <p lang="hi">बधाई हो! आपने {level.hi} प्रमाण पत्र जीता है।</p>
        </div>
        <Link to={`/gk/certificate/${result.certificate}`} className="gk-btn gk-btn-play gk-btn-big">🏅 View & print certificate</Link>
      </div>
    );
  }
  const toGo = Math.max(0, result.passPercent - result.percent);
  return (
    <div className="gk-reward try">
      <div>
        <h2>{kids ? '💪 So close! ' : ''}Score {result.passPercent}% to earn a certificate</h2>
        <p>You need {toGo.toFixed(1)}% more. Read the explanations below, then try again{kids ? ' or warm up with the quiz game' : ''}. · प्रमाण पत्र के लिए {result.passPercent}% अंक लाएँ।</p>
        <div className="gk-meter big"><span style={{ width: `${Math.min(100, (result.percent * 100) / result.passPercent)}%` }} /></div>
      </div>
      {kids && <Link to={`/gk/play/${group.key}`} className="gk-btn gk-btn-play">🎮 Play quiz game</Link>}
    </div>
  );
}

function clock(ms) {
  const s = Math.max(0, Math.floor(ms / 1000));
  const h = Math.floor(s / 3600);
  const m = Math.floor((s % 3600) / 60);
  return `${h ? `${h}:` : ''}${String(m).padStart(2, '0')}:${String(s % 60).padStart(2, '0')}`;
}
