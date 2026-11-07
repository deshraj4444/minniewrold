import { useCallback, useEffect, useState } from 'react';
import { Link, useParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useTitle } from '../../lib/hooks.js';
import QuestionCard, { LangSwitch, useLang } from '../components/QuestionCard.jsx';
import { groupByKey, examHref } from '../exams.js';
import Confetti from '../components/Confetti.jsx';

const ROUND = 10;
const LIVES = 3;
const SECONDS = 60;

function readBest(key) {
  try { return Number(localStorage.getItem(`gk.best.${key}`)) || 0; } catch { return 0; }
}
function saveBest(key, points) {
  try { localStorage.setItem(`gk.best.${key}`, String(points)); } catch { /* storage unavailable */ }
}

/**
 * Quick quiz game: 10 random questions from an exam's free papers, 3 lives, a 60-second clock per question,
 * points with a streak bonus, and stars at the end. No login needed; the best score is kept on this device.
 */
export default function GkPlay() {
  const { key } = useParams();
  const group = groupByKey(key);
  useTitle(group ? `${group.name} quiz game | Curious Minds` : 'Quiz game | Curious Minds');
  const [lang, setLang] = useLang();
  const [state, setState] = useState('intro'); // intro | loading | playing | over
  const [questions, setQuestions] = useState([]);
  const [index, setIndex] = useState(0);
  const [chosen, setChosen] = useState(null);
  const [lives, setLives] = useState(LIVES);
  const [points, setPoints] = useState(0);
  const [streak, setStreak] = useState(0);
  const [correct, setCorrect] = useState(0);
  const [left, setLeft] = useState(SECONDS);
  const [best, setBest] = useState(() => readBest(key));
  const [error, setError] = useState('');
  const [popup, setPopup] = useState('');

  const start = useCallback(async () => {
    setState('loading');
    setError('');
    try {
      const qs = await api.exams.practice(group.category, ROUND);
      if (!qs.length) throw new Error('No questions for this exam yet.');
      setQuestions(qs);
      setIndex(0); setChosen(null); setLives(LIVES); setPoints(0); setStreak(0); setCorrect(0); setLeft(SECONDS);
      setState('playing');
    } catch (e) {
      setError(e.message);
      setState('intro');
    }
  }, [group?.category]);

  const q = questions[index];
  const answered = chosen !== null;

  const answer = useCallback((option) => {
    if (answered || !q) return;
    setChosen(option);
    if (option === q.answer) {
      const gain = 10 + streak * 2 + Math.round(left / 6);
      setPoints((p) => p + gain);
      setStreak((s) => s + 1);
      setCorrect((c) => c + 1);
      setPopup(`+${gain}${streak >= 2 ? ` 🔥 ${streak + 1} in a row!` : ''}`);
    } else {
      setStreak(0);
      setLives((l) => l - 1);
      setPopup(option === -1 ? '⏰ Time up!' : '💔 Oops!');
    }
  }, [answered, q, streak, left]);

  // Per-question clock; running out counts as a wrong answer.
  useEffect(() => {
    if (state !== 'playing' || answered) return undefined;
    if (left <= 0) { answer(-1); return undefined; }
    const t = setTimeout(() => setLeft((s) => s - 1), 1000);
    return () => clearTimeout(t);
  }, [state, answered, left, answer]);

  function next() {
    setPopup('');
    if (lives <= 0 || index + 1 >= questions.length) {
      setState('over');
      if (points > best) { saveBest(key, points); setBest(points); }
      window.scrollTo(0, 0);
      return;
    }
    setIndex((i) => i + 1);
    setChosen(null);
    setLeft(SECONDS);
    document.querySelector('.gk-play-board')?.scrollIntoView({ block: 'start', behavior: 'smooth' });
  }

  if (!group) return <div className="gk-wrap gk-empty"><h1>Game not found</h1><Link to="/gk/tests">All tests</Link></div>;

  if (state === 'intro' || state === 'loading') {
    return (
      <div className="gk-wrap gk-section gk-play">
        <div className="gk-play-intro">
          <div className="gk-play-big" aria-hidden="true">🎮</div>
          <h1>{group.name} quiz game</h1>
          <p className="gk-title-hi" lang="hi">{group.nameHi} क्विज़ गेम</p>
          <ul className="gk-play-rules">
            <li>❓ {ROUND} random questions from the practice papers · {ROUND} यादृच्छिक प्रश्न</li>
            <li>❤️ {LIVES} lives: a wrong answer costs one · {LIVES} जीवन</li>
            <li>⏱ {SECONDS} seconds per question; answer fast for bonus points</li>
            <li>🔥 Answer in a row to build a streak bonus · लगातार सही उत्तर = बोनस</li>
          </ul>
          {best > 0 && <p className="gk-play-best">🏆 Your best: <b>{best}</b> points</p>}
          {error && <p className="gk-err">{error}</p>}
          <button className="gk-btn gk-btn-play gk-btn-big" onClick={start} disabled={state === 'loading'}>{state === 'loading' ? 'Loading…' : '▶ Start game'}</button>
          <p><Link to={examHref(group)}>or take a full paper →</Link></p>
        </div>
      </div>
    );
  }

  if (state === 'over') {
    const stars = correct >= 9 ? 3 : correct >= 7 ? 2 : correct >= 5 ? 1 : 0;
    const msg = ['Good try! Practise a little more and play again. 💪', 'Well done! You earned a star! ⭐', 'Great job! Two stars! 🌟', 'Superstar! Perfect play! 🏆'][stars];
    return (
      <div className="gk-wrap gk-section gk-play">
        {stars >= 2 && <Confetti />}
        <div className="gk-play-intro">
          <div className="gk-play-stars" aria-label={`${stars} of 3 stars`}>{[0, 1, 2].map((i) => <span key={i} className={i < stars ? 'on' : ''} style={{ animationDelay: `${i * 0.25}s` }}>★</span>)}</div>
          <h1>{msg}</h1>
          <div className="gk-test-facts">
            <div><b>{points}</b><span>Points</span></div>
            <div><b>{correct}/{index + 1}</b><span>Correct</span></div>
            <div><b>{best}</b><span>Best score</span></div>
          </div>
          {points >= best && points > 0 && <p className="gk-play-best">🎉 New best score!</p>}
          <div className="gk-result-actions">
            <button className="gk-btn gk-btn-play gk-btn-big" onClick={start}>🔁 Play again</button>
            <Link className="gk-btn gk-btn-outline" to={examHref(group)}>Take a full paper & earn a certificate</Link>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="gk-wrap gk-section gk-play">
      <div className="gk-play-board">
        <div className="gk-play-hud">
          <span className="gk-play-lives" aria-label={`${lives} lives left`}>{[...Array(LIVES)].map((_, i) => <span key={i} className={i < lives ? '' : 'lost'}>❤️</span>)}</span>
          <span>Q {index + 1}/{questions.length}</span>
          <span className={left <= 10 && !answered ? 'gk-play-time low' : 'gk-play-time'}>⏱ {left}s</span>
          <span className="gk-play-points">⭐ {points}</span>
          {streak >= 2 && <span className="gk-play-streak">🔥 {streak}</span>}
        </div>
        <div className="gk-play-progress"><span style={{ width: `${((index + (answered ? 1 : 0)) * 100) / questions.length}%` }} /></div>
        <div className="gk-exam-qhead"><span>{group.name}</span><LangSwitch lang={lang} setLang={setLang} /></div>
        {popup && <div className={chosen === q.answer ? 'gk-play-pop good' : 'gk-play-pop bad'} role="status">{popup}</div>}
        <QuestionCard key={q.id} q={q} number={index + 1} lang={lang} chosen={answered ? chosen : undefined} onChoose={answer} reveal={answered} />
        {answered && (
          <div className="gk-exam-actions">
            <button className="gk-btn gk-btn-play gk-btn-big" onClick={next} autoFocus>{lives <= 0 ? 'See result' : index + 1 >= questions.length ? 'Finish 🏁' : 'Next question →'}</button>
          </div>
        )}
      </div>
    </div>
  );
}
