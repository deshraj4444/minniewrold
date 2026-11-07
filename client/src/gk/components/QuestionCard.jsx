import { useEffect, useState } from 'react';

const letters = ['A', 'B', 'C', 'D'];

/** Hindi names and chip colours for the usual exam sections. */
const sectionInfo = {
  'Child Development & Pedagogy': ['बाल विकास एवं शिक्षाशास्त्र', '#6d3fc0'],
  'General Awareness': ['सामान्य जागरूकता', '#c2502e'],
  'General Studies': ['सामान्य अध्ययन', '#475569'],
  English: ['अंग्रेज़ी', '#1e6fb8'],
  Hindi: ['हिंदी', '#b8336a'],
  Sanskrit: ['संस्कृत', '#8a5a00'],
  'Social Studies': ['सामाजिक अध्ययन', '#2f7d4f'],
  Mathematics: ['गणित', '#0f766e'],
  Science: ['विज्ञान', '#0369a1'],
  'Environmental Studies': ['पर्यावरण अध्ययन', '#4d7c0f'],
  Reasoning: ['तर्कशक्ति', '#9333ea'],
  'Himachal GK': ['हिमाचल सामान्य ज्ञान', '#b45309'],
  'Current Affairs': ['समसामयिकी', '#be123c'],
  'Mental Ability': ['मानसिक योग्यता', '#9333ea'],
  Arithmetic: ['अंकगणित', '#0f766e'],
  Intelligence: ['बुद्धि परीक्षण', '#9333ea'],
  'General Knowledge': ['सामान्य ज्ञान', '#b45309'],
  Language: ['भाषा', '#1e6fb8'],
};

/** An SVG figure is shown as an image (a data URI), so it can never run scripts. */
export function Figure({ svg, alt = 'Figure for this question' }) {
  if (!svg) return null;
  return <img className="gk-figure" alt={alt} src={`data:image/svg+xml;charset=utf-8,${encodeURIComponent(svg)}`} />;
}

/** Reading passage above a question. Open by default; questions on the same passage repeat it. */
function Passage({ en, hi, lang }) {
  const [p, pHi] = pick(en, hi, lang);
  if (!p) return null;
  return (
    <details className="gk-passage" open>
      <summary>Read the passage / गद्यांश पढ़ें</summary>
      {p.split(/\n+/).map((para, i) => <p key={i}>{para}</p>)}
      {pHi && <div className="gk-passage-hi" lang="hi">{pHi.split(/\n+/).map((para, i) => <p key={i}>{para}</p>)}</div>}
    </details>
  );
}

export function sectionLabel(name) {
  const hi = sectionInfo[name]?.[0];
  return hi ? `${name} | ${hi}` : name || 'General';
}

/** "both" (English with Hindi underneath, as printed in the real papers), "en" or "hi". Remembered per device. */
export function useLang() {
  const [lang, setLang] = useState(() => {
    try { return localStorage.getItem('gk.lang') || 'both'; } catch { return 'both'; }
  });
  useEffect(() => {
    try { localStorage.setItem('gk.lang', lang); } catch { /* storage unavailable */ }
  }, [lang]);
  return [lang, setLang];
}

export function LangSwitch({ lang, setLang }) {
  return (
    <div className="gk-lang" role="group" aria-label="Question language">
      {[['both', 'EN + हिं'], ['en', 'English'], ['hi', 'हिंदी']].map(([k, label]) => (
        <button key={k} type="button" aria-pressed={lang === k} className={lang === k ? 'on' : ''} onClick={() => setLang(k)}>{label}</button>
      ))}
    </div>
  );
}

/** Picks what to show for one piece of text: [primary, secondary]. Falls back to English when there's no Hindi. */
function pick(en, hi, lang) {
  if (!hi) return [en, null];
  if (lang === 'en') return [en, null];
  if (lang === 'hi') return [hi, null];
  return [en, hi];
}

/**
 * One question styled like the bilingual practice cards: section chip, bold English question with the Hindi
 * underneath, and option cards. In the exam, options are radio buttons; in the result, the right answer is
 * highlighted and the explanation box shows.
 */
export default function QuestionCard({ q, number, lang, chosen, onChoose, reveal = false, hideAnswer = false }) {
  // In Hindi-only mode the main text is Hindi, so it carries lang="hi" too.
  const hiFirst = lang === 'hi' ? 'hi' : undefined;
  const color = sectionInfo[q.section]?.[1] ?? '#475569';
  const [text, textHi] = pick(q.text, q.textHi, lang);
  const optsHi = q.optionsHi && q.optionsHi.length === 4 ? q.optionsHi : null;
  // Previous-year papers end the English explanation with "Source: <url>"; that's shown as a small link instead.
  const source = /\s*Source:\s*(https?:\/\/\S+)\s*$/.exec(q.explanation ?? '');
  const explanation = source ? q.explanation.slice(0, source.index) : q.explanation;
  const [expl, explHi] = pick(explanation, q.explanationHi, lang);
  const qid = `q-${q.id}`;

  return (
    <article className="gk-qcard">
      <div className="gk-qcard-top">
        <span className="gk-chip" style={{ '--chip': color }}>{sectionLabel(q.section)}</span>
        <span className="gk-qno">Q.{number}{q.marks && Number(q.marks) !== 1 ? <small> · {Number(q.marks)} marks</small> : null}</span>
      </div>
      <Passage en={q.passage} hi={q.passageHi} lang={lang} />
      <h2 id={qid} className="gk-q-en" lang={q.textHi ? hiFirst : undefined}>{text}</h2>
      {textHi && <p className="gk-q-hi" lang="hi">{textHi}</p>}
      <Figure svg={q.figure} />

      <div className="gk-qopts" role={reveal ? undefined : 'radiogroup'} aria-labelledby={qid}>
        {q.options.map((opt, i) => {
          const [o, oHi] = pick(opt, optsHi?.[i], lang);
          const state = reveal
            ? i === q.answer ? 'right' : i === chosen ? 'wrong' : ''
            : chosen === i ? 'chosen' : '';
          const body = (
            <>
              <span className="gk-qletter">{letters[i]}</span>
              <span className="gk-qopt-text">
                <span lang={optsHi ? hiFirst : undefined}>{o}</span>
                {oHi && <small lang="hi">{oHi}</small>}
              </span>
              {reveal && i === q.answer && <em>✓ {chosen === i ? 'Your answer' : 'Correct'}</em>}
              {reveal && i === chosen && i !== q.answer && <em>✗ Your answer</em>}
            </>
          );
          return reveal
            ? <div key={i} className={`gk-qopt ${state}`}>{body}</div>
            : <button key={i} type="button" role="radio" aria-checked={chosen === i} className={`gk-qopt ${state}`} onClick={() => onChoose(i)}>{body}</button>;
        })}
      </div>

      {reveal && !hideAnswer && (
        <div className="gk-answer">
          <b>Answer / उत्तर: {letters[q.answer]}</b>
          {expl && <p lang={q.explanationHi ? hiFirst : undefined}>{expl}</p>}
          {explHi && <p className="hi" lang="hi">{explHi}</p>}
          {source && <a className="gk-answer-src" href={source[1]} target="_blank" rel="noopener noreferrer nofollow">Source / स्रोत</a>}
        </div>
      )}
    </article>
  );
}
