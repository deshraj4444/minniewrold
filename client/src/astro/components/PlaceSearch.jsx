import { useEffect, useId, useRef, useState } from 'react';
import { api } from '../../lib/api.js';

export const placeLabel = (p) => (p ? [p.name, p.region, p.country].filter(Boolean).join(', ') : '');

/** City search (combobox) backed by /api/astro/places. Calls onChange with { name, region, country, latitude, longitude, timeZone }. */
export default function PlaceSearch({ value, onChange, label = 'Birth place', required = true, id }) {
  const autoId = useId();
  const inputId = id ?? `place-${autoId}`;
  const listId = `${inputId}-list`;
  const [query, setQuery] = useState(placeLabel(value));
  const [results, setResults] = useState([]);
  const [open, setOpen] = useState(false);
  const [active, setActive] = useState(-1);
  const [searching, setSearching] = useState(false);
  const skipNext = useRef(false);

  // Sync the text when a place is set from outside (e.g. restored details); typing clears value, so don't sync on null.
  useEffect(() => { if (value) setQuery(placeLabel(value)); }, [value]);

  useEffect(() => {
    if (skipNext.current) { skipNext.current = false; return undefined; }
    if (query.trim().length < 2 || query === placeLabel(value)) { setResults([]); return undefined; }
    setSearching(true);
    const timer = setTimeout(() => {
      api.astro.places(query)
        .then((r) => { setResults(r); setOpen(true); setActive(r.length ? 0 : -1); })
        .catch(() => setResults([]))
        .finally(() => setSearching(false));
    }, 300);
    return () => clearTimeout(timer);
  }, [query, value]);

  function choose(place) {
    skipNext.current = true;
    onChange(place);
    setQuery(placeLabel(place));
    setOpen(false);
    setResults([]);
  }

  function onKeyDown(e) {
    if (!open || !results.length) return;
    if (e.key === 'ArrowDown') { e.preventDefault(); setActive((a) => (a + 1) % results.length); }
    else if (e.key === 'ArrowUp') { e.preventDefault(); setActive((a) => (a - 1 + results.length) % results.length); }
    else if (e.key === 'Enter' && active >= 0) { e.preventDefault(); choose(results[active]); }
    else if (e.key === 'Escape') setOpen(false);
  }

  return (
    <div className="field place-search">
      <label htmlFor={inputId}>{label}</label>
      <input
        id={inputId}
        role="combobox"
        aria-expanded={open && results.length > 0}
        aria-controls={listId}
        aria-autocomplete="list"
        aria-activedescendant={active >= 0 ? `${listId}-${active}` : undefined}
        autoComplete="off"
        placeholder="Start typing a city…"
        value={query}
        required={required}
        onChange={(e) => { setQuery(e.target.value); if (value) onChange(null); }}
        onKeyDown={onKeyDown}
        onFocus={() => results.length && setOpen(true)}
        onBlur={() => setTimeout(() => setOpen(false), 150)}
      />
      {searching && <span className="place-hint">Searching…</span>}
      {value && <span className="place-hint">{value.latitude.toFixed(2)}°, {value.longitude.toFixed(2)}° · {value.timeZone}</span>}
      {open && results.length > 0 && (
        <ul id={listId} role="listbox" className="place-list">
          {results.map((p, i) => (
            <li
              key={`${p.latitude},${p.longitude}`}
              id={`${listId}-${i}`}
              role="option"
              aria-selected={i === active}
              className={i === active ? 'active' : ''}
              onMouseDown={(e) => { e.preventDefault(); choose(p); }}
            >
              <strong>{p.name}</strong> <span>{[p.region, p.country].filter(Boolean).join(', ')}</span>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
