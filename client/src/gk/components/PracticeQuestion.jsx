import { useState } from 'react';
import { playClick, playCorrect, playWrong } from '../../lib/sound.js';
import QuestionCard from './QuestionCard.jsx';

const letters = ['A', 'B', 'C', 'D'];

/**
 * A practice question with its solution: tap an option to check it, or open the solution. The solution stays in the
 * page (inside <details>) so search engines can read it, as in the server-rendered HTML.
 */
export default function PracticeQuestion({ q, number, lang }) {
  const [chosen, setChosen] = useState(null);
  const [open, setOpen] = useState(false);
  return (
    <li>
      <QuestionCard q={q} number={number} lang={lang} chosen={chosen} reveal={chosen != null} hideAnswer
        onChoose={(i) => { playClick(); if (i === q.answer) playCorrect(); else playWrong(); setChosen(i); setOpen(true); }} />
      <details className="gk-solution" open={open} onToggle={(e) => setOpen(e.currentTarget.open)}>
        <summary>Answer and solution / <span lang="hi">उत्तर एवं हल</span></summary>
        <p><b>Answer: ({letters[q.answer]}) {q.options[q.answer]}</b>{chosen != null && <> · {chosen === q.answer ? '✓ You got it right' : `✗ Your answer was ${letters[chosen]}`}</>}</p>
        {q.explanation && <p>{q.explanation}</p>}
        {q.explanationHi && <p lang="hi">{q.explanationHi}</p>}
      </details>
    </li>
  );
}
