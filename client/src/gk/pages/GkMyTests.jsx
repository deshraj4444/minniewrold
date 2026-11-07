import { Link, Navigate } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import { useStudent } from '../student.jsx';
import { levelInfo } from './GkCertificate.jsx';

export default function GkMyTests() {
  useTitle('My results | Curious Minds');
  const { student, logout } = useStudent();
  const { data, loading } = useAsync(() => (student ? api.students.attempts() : Promise.resolve([])), [student?.id]);

  if (student === null) return <Navigate to="/gk/login?next=/gk/my-tests" replace />;
  if (!student) return <div className="gk-wrap gk-empty">Loading…</div>;

  const done = (data ?? []).filter((a) => a.submittedAt);
  const running = (data ?? []).filter((a) => !a.submittedAt);
  const avg = done.length ? Math.round(done.reduce((s, a) => s + (a.maxScore ? (a.score * 100) / a.maxScore : 0), 0) / done.length) : 0;
  const certs = done.filter((a) => a.certificate);
  const best = done.length ? Math.round(Math.max(...done.map((a) => (a.maxScore ? (a.score * 100) / a.maxScore : 0)))) : 0;

  return (
    <div className="gk-wrap gk-section">
      <div className="gk-section-head">
        <h1 className="gk-page-title">Hi, {student.fullName.split(' ')[0]} 👋</h1>
        <button className="gk-link" onClick={logout}>Log out</button>
      </div>
      {student.allAccess && (
        <p className="gk-ok">⭐ You have an All-Access pass{student.allAccessUntil ? ` until ${formatDate(student.allAccessUntil, { day: 'numeric', month: 'long', year: 'numeric' })}` : ''}: every mock test is unlocked.</p>
      )}

      <div className="gk-test-facts">
        <div><b>{done.length}</b><span>Tests taken</span></div>
        <div><b>{avg}%</b><span>Average score</span></div>
        <div><b>{best}%</b><span>Best score</span></div>
        <div><b>{certs.length}</b><span>Certificates 🏅</span></div>
      </div>

      {certs.length > 0 && (
        <>
          <h2>My certificates / मेरे प्रमाण पत्र</h2>
          <div className="gk-cert-shelf">
            {certs.map((a) => {
              const pct = a.maxScore ? (a.score * 100) / a.maxScore : 0;
              const level = pct >= 90 ? 'Gold' : pct >= 75 ? 'Silver' : 'Bronze';
              return (
                <Link key={a.id} to={`/gk/certificate/${a.certificate}`} className={`gk-cert-mini ${level.toLowerCase()}`}>
                  <span aria-hidden="true">{levelInfo[level].medal}</span>
                  <b>{a.exam.title}</b>
                  <small>{level} · {Math.round(pct)}% · {formatDate(a.submittedAt)}</small>
                </Link>
              );
            })}
          </div>
        </>
      )}

      {running.length > 0 && (
        <>
          <h2>In progress</h2>
          <ul className="gk-history">
            {running.map((a) => (
              <li key={a.id}>
                <div><b>{a.exam.title}</b><small>Started {formatDate(a.startedAt)}</small></div>
                <Link className="gk-btn" to={`/gk/attempt/${a.id}`}>Continue</Link>
              </li>
            ))}
          </ul>
        </>
      )}

      <h2>Results</h2>
      {loading && <p className="gk-muted">Loading…</p>}
      {data && done.length === 0 && <p className="gk-muted">No tests yet. <Link to="/gk/tests">Take your first mock test →</Link></p>}
      <ul className="gk-history">
        {done.map((a) => {
          const pct = a.maxScore ? Math.round((a.score * 100) / a.maxScore) : 0;
          return (
            <li key={a.id}>
              <div><b>{a.exam.title}</b><small>{formatDate(a.submittedAt)} · {a.correct} correct, {a.wrong} wrong, {a.unanswered} skipped</small></div>
              <div className="gk-history-score"><b>{Number(a.score)}/{a.maxScore}</b><div className="gk-meter"><span style={{ width: `${pct}%` }} /></div></div>
              <div className="gk-history-links">
                <Link className="gk-btn gk-btn-outline" to={`/gk/attempt/${a.id}`}>View</Link>
                {a.certificate && <Link className="gk-link" to={`/gk/certificate/${a.certificate}`}>🏅 Certificate</Link>}
              </div>
            </li>
          );
        })}
      </ul>
    </div>
  );
}
