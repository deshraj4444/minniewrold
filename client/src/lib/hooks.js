import { useCallback, useEffect, useRef, useState } from 'react';

/** Runs an async loader when deps change. Returns { data, error, loading, reload }. Ignores stale responses. */
export function useAsync(loader, deps) {
  const [state, setState] = useState({ data: null, error: null, loading: true });
  const counter = useRef(0);

  const run = useCallback(() => {
    const id = ++counter.current;
    setState((s) => ({ ...s, loading: true, error: null }));
    loader()
      .then((data) => id === counter.current && setState({ data, error: null, loading: false }))
      .catch((error) => id === counter.current && setState({ data: null, error, loading: false }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, deps);

  useEffect(run, [run]);
  return { ...state, reload: run };
}

/** Sets document.title while mounted (the server already sets it for the first load and for link previews). */
export function useTitle(title) {
  useEffect(() => {
    if (title) document.title = title;
  }, [title]);
}

const loadedFonts = new Set();

/** Loads a Google Fonts stylesheet once, only for the sites that use it. */
export function useFonts(href) {
  useEffect(() => {
    if (loadedFonts.has(href)) return;
    loadedFonts.add(href);
    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.href = href;
    document.head.appendChild(link);
  }, [href]);
}

/** Like useState, but remembered in localStorage (per-viewer conveniences such as saved birth details). */
export function useStoredState(key, initial) {
  const [value, setValue] = useState(() => {
    try {
      const raw = localStorage.getItem(key);
      return raw ? JSON.parse(raw) : initial;
    } catch {
      return initial;
    }
  });

  const set = useCallback((next) => {
    setValue((prev) => {
      const v = typeof next === 'function' ? next(prev) : next;
      try { localStorage.setItem(key, JSON.stringify(v)); } catch { /* storage unavailable */ }
      return v;
    });
  }, [key]);

  return [value, set];
}
