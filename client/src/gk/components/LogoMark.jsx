/** The Curious Minds mark: an open book with a spark, drawn in the current text colour. */
export default function LogoMark() {
  return (
    <span className="gk-logo-icon" aria-hidden="true">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round">
        <path d="M3 6.5C5.5 5.2 8.7 5.2 12 7v12c-3.3-1.8-6.5-1.8-9-.5z" />
        <path d="M21 6.5c-2.5-1.3-5.7-1.3-9 .5v12c3.3-1.8 6.5-1.8 9-.5z" />
        <path d="M17 1.8v2.4M15.8 3h2.4" />
      </svg>
    </span>
  );
}
