import { LangSwitch, sectionLabel, useLang } from '../components/QuestionCard.jsx';
import PracticeQuestion from '../components/PracticeQuestion.jsx';
import { useEffect, useState } from 'react';
import { Link, Navigate, useNavigate, useParams } from 'react-router';
import { usePrerenderHandoff } from '../../lib/prerender.js';
import { JEE_HUB, jeeCompanion, jeePageFor } from '../jee.js';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { formatDate, minutesLabel } from '../../lib/format.js';
import { useStudent } from '../student.jsx';
import UpiPayment from '../components/UpiPayment.jsx';
import WhatsAppBuy from '../components/WhatsAppBuy.jsx';
import ShareButtons from '../../shared/ShareButtons.jsx';
import { groupOf, isKids, examHref } from '../exams.js';

/** "1 mark", "1.25 marks", or "2 or 3 marks (see the table)" when sections differ. */
function marksText(exam) {
  const per = [...new Set(exam.sections.map((s) => Number(s.marks) / s.count))];
  if (per.length === 1) return per[0] === 1 ? '1 mark' : `${per[0]} marks`;
  return 'the marks shown in the table above';
}

/**
 * A test's page: facts, sections, instructions and the start box. With `jeePage` (the /jee chapter tests) it also
 * lists every question with its solution, matching what the server renders for search engines (SpaHost.Jee.cs).
 */
export default function GkTest({ slug: slugProp, jeePage }) {
  const params = useParams();
  const slug = slugProp ?? params.slug;
  const moved = !slugProp && jeePageFor('test', slug);
  const navigate = useNavigate();
  const { student } = useStudent();
  const { data: exam, loading, error, reload } = useAsync(() => api.exams.get(slug), [slug, student?.id]);
  const [starting, setStarting] = useState(false);
  const [startError, setStartError] = useState('');
  useTitle(jeePage ? jeePage.title : exam ? `${exam.title} | Curious Minds` : 'Mock test | Curious Minds');
  const solutions = useAsync(() => (jeePage ? api.exams.jeeSolutions(slug) : Promise.resolve(null)), [slug, jeePage]);
  usePrerenderHandoff(Boolean(jeePage) && Boolean(exam) && !solutions.loading);
  const here = jeePage?.path ?? `/gk/tests/${slug}`;
  const notes = jeePage && jeeCompanion(jeePage);

  // While a payment waits for approval, check every 20 seconds so the test unlocks without a refresh.
  const pending = exam?.access === 'pending';
  useEffect(() => {
    if (!pending) return undefined;
    const t = setInterval(reload, 20000);
    return () => clearInterval(t);
  }, [pending, reload]);

  // Old /gk/tests/<slug> links to a JEE chapter test go to its /jee page (the server sends a 301 for direct visits).
  if (moved) return <Navigate to={moved.path} replace />;
  if (loading && !exam) return <div className="gk-wrap gk-empty">Loading…</div>;
  if (error) return <div className="gk-wrap gk-empty"><h1>Test not found</h1><Link to={jeePage ? JEE_HUB : '/gk/tests'}>All tests</Link></div>;

  const running = exam.myAttempts.find((a) => !a.submittedAt);
  const done = exam.myAttempts.filter((a) => a.submittedAt);

  async function start() {
    setStarting(true);
    setStartError('');
    try {
      const { id } = await api.exams.start(slug);
      navigate(`/gk/attempt/${id}`);
    } catch (e) {
      setStartError(e.message);
      setStarting(false);
    }
  }

  return (
    <div className="gk-wrap gk-section gk-test-detail">
      {jeePage
        ? <nav className="gk-crumbs" aria-label="Breadcrumb"><Link to="/gk">Curious Minds</Link> › <Link to={JEE_HUB}>IIT JEE</Link> › {jeePage.crumb}</nav>
        : <nav className="gk-crumbs"><Link to="/gk/tests">Mock tests</Link> › <Link to={examHref(groupOf(exam.category))}>{groupOf(exam.category).name}</Link></nav>}
      <div className="gk-test-detail-grid">
        <div>
          <span className="gk-tag">{exam.kind}{exam.year ? ` · ${exam.year}` : ''}</span>
          <h1>{exam.title}</h1>
          <p className="gk-post-dek">{exam.description}</p>
          {notes && <p><Link to={notes.path} className="gk-btn gk-btn-outline">Revise first: {notes.title}</Link></p>}
          <div className="gk-share-box"><b>Share this test / टेस्ट शेयर करें</b><ShareButtons title={`${exam.title}: ${exam.price > 0 ? 'online mock test' : 'free online mock test'} in English + हिंदी with answer key`} className="share gk-share" /></div>

          <div className="gk-test-facts">
            <div><b>{exam.questionCount}</b><span>Questions</span></div>
            <div><b>{minutesLabel(exam.durationMinutes)}</b><span>Time</span></div>
            <div><b>{Number(exam.maxMarks)}</b><span>Marks</span></div>
            <div><b>{Number(exam.negativeMark) ? `−${exam.negativeMark}` : 'None'}</b><span>Negative marking</span></div>
            <div><b>{exam.price > 0 ? `₹${exam.price}` : 'Free'}</b><span>Price</span></div>
          </div>

          <h2>Sections</h2>
          <table className="gk-simple-table">
            <thead><tr><th>Section</th><th>Questions</th><th>Marks</th></tr></thead>
            <tbody>{exam.sections.map((s) => <tr key={s.section}><td>{sectionLabel(s.section)}</td><td>{s.count}</td><td>{Number(s.marks)}</td></tr>)}</tbody>
          </table>

          <h2>Instructions</h2>
          <ul className="gk-instructions">
            <li>The timer starts when you begin and keeps running even if you close the page. You can come back and continue.</li>
            <li>Each question has one correct answer and carries {marksText(exam)}. {Number(exam.negativeMark) ? `${exam.negativeMark} marks are deducted for each wrong answer.` : 'There is no negative marking.'}</li>
            <li>Your answers are saved automatically. Use “Mark for review” to come back to a question.</li>
            <li>When time runs out the test is submitted automatically. You'll see your score, rank and the correct answers straight away.</li>
            <li>{isKids(exam.category) ? <>Score {exam.passPercent}% or more to earn a certificate 🏅 (Bronze 60%+, Silver 75%+, Gold 90%+).</> : <>Qualifying mark shown in results: {exam.passPercent}% (general category). Pass to earn a certificate.</>}</li>
          </ul>
          {exam.kind === 'Year pattern' && (
            <p className="gk-note">An original practice paper on the {exam.year} pattern: same sections, number of questions and kinds of questions as that year. It is not the official question paper.</p>
          )}
          {exam.kind === 'Expected paper' && (
            <p className="gk-note">An original paper on the latest announced pattern for the {exam.year} exam. Always check the official notification for the final pattern.</p>
          )}
          {exam.kind === 'Previous year' && (
            <p className="gk-note">These are questions from the real exam. Answers were checked against published solutions; if you spot a mistake, please tell us through the contact page.</p>
          )}
        </div>

        <aside className="gk-test-start">
          {student === null ? (
            <>
              <h2>Log in to start</h2>
              <p className="gk-muted">A free account saves your results and shows your progress over time.</p>
              <Link to={`/gk/register?next=${here}`} className="gk-btn">Create free account</Link>
              <Link to={`/gk/login?next=${here}`} className="gk-btn gk-btn-outline">I have an account</Link>
            </>
          ) : exam.access === 'locked' || exam.access === 'rejected' ? (
            <>
              <h2>Unlock for ₹{exam.price}</h2>
              <p className="gk-muted">One-time payment. Take this test as many times as you like.</p>
              {exam.access === 'rejected' && <p className="gk-err">{exam.purchase.note}</p>}
              <WhatsAppBuy exam={exam} student={student} number={exam.payment?.helpWhatsApp} />
              {exam.payment?.upiId && <p className="gk-or">or pay by UPI yourself / या स्वयं UPI से भुगतान करें</p>}
              <UpiPayment exam={exam} onSubmitted={reload} />
            </>
          ) : exam.access === 'pending' ? (
            <>
              <h2>Payment received ✓</h2>
              <p className="gk-muted">
                We're checking your payment (transaction ID <b>{exam.purchase.upiRef}</b>). The test unlocks here automatically once it's approved,
                usually within a few hours. You can close this page and come back later.
              </p>
              <div className="gk-pending-dot" aria-hidden="true" />
              <WhatsAppBuy exam={exam} student={student} number={exam.payment?.helpWhatsApp} />
              <details className="gk-small-details">
                <summary>Entered the wrong transaction ID?</summary>
                <UpiPayment exam={exam} onSubmitted={reload} />
              </details>
            </>
          ) : (
            <>
              <h2>{running ? 'Test in progress' : 'Ready?'}</h2>
              {exam.access === 'unlocked' && <p className="gk-ok">✓ Unlocked</p>}
              {exam.access === 'all-access' && <p className="gk-ok">⭐ Included in your All-Access pass</p>}
              {running
                ? <p className="gk-muted">You started this test on {formatDate(running.startedAt, { day: 'numeric', month: 'short', hour: 'numeric', minute: '2-digit' })}. Continue where you left off.</p>
                : <p className="gk-muted">{exam.attempts > 0 ? `${exam.attempts} attempts so far.` : 'Be the first to take this test!'}</p>}
              <button className="gk-btn" onClick={start} disabled={starting || student === undefined}>{starting ? 'Starting…' : running ? 'Continue test' : done.length ? 'Take again' : 'Start test'}</button>
              {startError && <p className="gk-err">{startError}</p>}
            </>
          )}

          {done.length > 0 && (
            <div className="gk-past">
              <h3>Your attempts</h3>
              <ul>
                {done.map((a) => (
                  <li key={a.id}>
                    <Link to={`/gk/attempt/${a.id}`}>{formatDate(a.submittedAt)}</Link>
                    <b>{Number(a.score)}/{a.maxScore}</b>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </aside>
      </div>

      {jeePage && solutions.data && <Solutions questions={solutions.data} notes={notes} />}
    </div>
  );
}

/** Every question of a free JEE chapter test: tap an option to check it, or open the solution. */
function Solutions({ questions, notes }) {
  const [lang, setLang] = useLang();
  return (
    <section className="gk-solutions" aria-labelledby="gk-solutions-title">
      <div className="gk-result-head">
        <h2 id="gk-solutions-title">All {questions.length} questions with answers and solutions</h2>
        <LangSwitch lang={lang} setLang={setLang} />
      </div>
      <p className="gk-muted">Want a real exam feel? Start the timed test above first. To practise, tap an option to check your answer, or open the solution below each question.</p>
      <ol className="gk-review-list">
        {questions.map((q, i) => <PracticeQuestion key={q.id} q={q} number={i + 1} lang={lang} />)}
      </ol>
      {notes && <p className="gk-result-actions"><Link to={notes.path} className="gk-btn">Read the {notes.topic} notes</Link><Link to={JEE_HUB} className="gk-link">All IIT JEE chapters</Link></p>}
    </section>
  );
}
