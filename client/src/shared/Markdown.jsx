import { Children } from 'react';
import ReactMarkdown from 'react-markdown';
import remarkGfm from 'remark-gfm';

// A run of Devanagari text (with the spaces and punctuation inside it). Same pattern as SpaHost.Jee.cs on the server.
const devanagari = /[\u0900-\u097F](?:[\u0900-\u097F\u200C\u200D\s,.;:()'"\-\u2013]*[\u0900-\u097F])?/g;

/** Wraps each run of Hindi text in <span lang="hi"> so browsers, screen readers and search engines treat it as Hindi. */
function markHindi(children) {
  return Children.map(children, (child) => {
    if (typeof child !== 'string' || !/[\u0900-\u097F]/.test(child)) return child;
    const parts = [];
    let last = 0;
    for (const m of child.matchAll(devanagari)) {
      if (m.index > last) parts.push(child.slice(last, m.index));
      parts.push(<span key={m.index} lang="hi">{m[0]}</span>);
      last = m.index + m[0].length;
    }
    if (last < child.length) parts.push(child.slice(last));
    return parts;
  });
}

// Text-holding elements get their Hindi runs marked. `node` is react-markdown's syntax-tree node; don't pass it to the DOM.
const withHindi = (Tag) => function Hindi({ node, children, ...rest }) {
  return <Tag {...rest}>{markHindi(children)}</Tag>;
};

const components = {
  // External links open in a new tab; internal links stay in the app.
  a: ({ node, href = '', children, ...rest }) =>
    /^https?:\/\//.test(href)
      ? <a href={href} target="_blank" rel="noopener noreferrer" {...rest}>{markHindi(children)}</a>
      : <a href={href} {...rest}>{markHindi(children)}</a>,
  table: ({ children }) => <div className="md-table"><table>{children}</table></div>,
  ...Object.fromEntries(['p', 'li', 'td', 'th', 'h1', 'h2', 'h3', 'h4', 'strong', 'em', 'blockquote'].map((t) => [t, withHindi(t)]))
};

/** Renders Markdown with tables. Raw HTML in the source is ignored, so content can't inject scripts. */
export default function Markdown({ children, className = 'prose' }) {
  return (
    <div className={className}>
      <ReactMarkdown remarkPlugins={[remarkGfm]} components={components}>{children ?? ''}</ReactMarkdown>
    </div>
  );
}
