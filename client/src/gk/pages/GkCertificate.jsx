import { Link, useParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import ShareButtons from '../../shared/ShareButtons.jsx';
import { groupOf, examHref } from '../exams.js';
import LogoMark from '../components/LogoMark.jsx';

export const levelInfo = {
  Gold: { medal: '🥇', hi: 'स्वर्ण', stars: 3, title: 'Certificate of Excellence', titleHi: 'उत्कृष्टता प्रमाण पत्र' },
  Silver: { medal: '🥈', hi: 'रजत', stars: 2, title: 'Certificate of Merit', titleHi: 'योग्यता प्रमाण पत्र' },
  Bronze: { medal: '🥉', hi: 'कांस्य', stars: 1, title: 'Certificate of Achievement', titleHi: 'उपलब्धि प्रमाण पत्र' }
};

/**
 * A printable certificate for a passed test. The page is public so it can be shared and checked: the code on it
 * is verified by the server, so a made-up link shows "not found".
 */
export default function GkCertificate() {
  const { code } = useParams();
  const { data: c, loading, error } = useAsync(() => api.exams.certificate(code), [code]);
  useTitle(c ? `${c.studentName}: ${c.exam.title} certificate | Curious Minds` : 'Certificate | Curious Minds');

  if (loading) return <div className="gk-wrap gk-empty">Loading…</div>;
  if (error || !c) {
    return (
      <div className="gk-wrap gk-empty">
        <h1>Certificate not found</h1>
        <p>The code <b>{code}</b> doesn’t match any certificate. Check the link and try again.</p>
        <Link to="/gk/tests">Practice tests</Link>
      </div>
    );
  }

  const level = levelInfo[c.level] ?? levelInfo.Bronze;
  const group = groupOf(c.exam.category);
  const date = formatDate(c.date, { day: 'numeric', month: 'long', year: 'numeric' });

  return (
    <div className="gk-wrap gk-section gk-cert-page">
      <div className="gk-cert-actions">
        <button className="gk-btn" onClick={() => window.print()}>🖨️ Print / Save as PDF</button>
        <Link className="gk-btn gk-btn-outline" to={examHref(group)}>More papers</Link>
        <ShareButtons title={`🏅 ${c.studentName} passed ${c.exam.title} with ${c.percent}%! Practise free:`} url={window.location.href} className="share gk-share" />
      </div>

      <article className={`gk-cert ${c.level.toLowerCase()}`} aria-label="Certificate">
        <div className="gk-cert-inner">
          <div className="gk-cert-brand"><LogoMark /> Curious Minds · MinnieWorld</div>
          <div className="gk-cert-medal" aria-hidden="true">{level.medal}</div>
          <h1>{level.title}</h1>
          <p className="gk-cert-hi" lang="hi">{level.titleHi}</p>
          <p className="gk-cert-small">This is proudly presented to · यह प्रमाण पत्र गर्व से प्रदान किया जाता है</p>
          <p className="gk-cert-name">{c.studentName}</p>
          <p className="gk-cert-text">
            for passing <b>{c.exam.title}</b> with <b>{Number(c.score)}/{c.maxScore}</b> marks (<b>{c.percent}%</b>)
            and earning the <b>{c.level}</b> level.
          </p>
          <p className="gk-cert-text" lang="hi">
            <b>{c.exam.title}</b> में <b>{Number(c.score)}/{c.maxScore}</b> अंक (<b>{c.percent}%</b>) प्राप्त कर {level.hi} स्तर के साथ उत्तीर्ण होने के लिए।
          </p>
          <div className="gk-cert-stars" aria-label={`${level.stars} of 3 stars`}>{'★'.repeat(level.stars)}<span>{'★'.repeat(3 - level.stars)}</span></div>
          <footer className="gk-cert-foot">
            <div><b>{date}</b><span>Date · दिनांक</span></div>
            <div className="gk-cert-seal" aria-hidden="true">{group.icon}</div>
            <div><b>{c.code}</b><span>Certificate no. · प्रमाण पत्र संख्या</span></div>
          </footer>
          <p className="gk-cert-verify">Verify at {window.location.origin}/gk/certificate/{c.code}. This is a practice-test certificate from Curious Minds, not an official exam result.</p>
        </div>
      </article>
    </div>
  );
}
