import { useEffect } from 'react';

// Some pages (the /jee section) arrive with their content rendered on the server in <div id="ssr"> next to the React
// root, so search engines can read it without JavaScript. Once the React page has rendered the same content, the
// server copy is removed so it isn't shown twice.

/** Removes the server-rendered copy of the page, if there is one. */
export function dropPrerender() {
  document.getElementById('ssr')?.remove();
}

/** Removes the server copy when the visitor has navigated away from the page it was rendered for. */
export function dropPrerenderUnless(pathname) {
  const ssr = document.getElementById('ssr');
  if (ssr && ssr.dataset.path !== pathname.replace(/\/+$/, '')) ssr.remove();
}

/** Call in a page that the server pre-renders: removes the server copy once `ready` (the page's data has loaded). */
export function usePrerenderHandoff(ready) {
  useEffect(() => { if (ready) dropPrerender(); }, [ready]);
}
