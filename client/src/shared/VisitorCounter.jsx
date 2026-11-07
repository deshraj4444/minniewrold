import { useEffect, useState } from 'react';
import { api } from '../lib/api.js';

// One request per page load even when several footers mount; the server counts each browser once a day.
let request;

function load() {
  request ??= api.post('/visits').catch(() => api.get('/visits')).catch(() => null);
  return request;
}

/** "👥 Total visitors / कुल आगंतुक: 2,345", shown in the site footers. */
export default function VisitorCounter({ className = 'visitor-counter' }) {
  const [total, setTotal] = useState(null);

  useEffect(() => {
    let alive = true;
    load().then((r) => { if (alive && r?.total) setTotal(r.total); });
    return () => { alive = false; };
  }, []);

  if (!total) return null;
  return (
    <p className={className} aria-live="polite">
      <span aria-hidden="true">👥</span> Total visitors <span lang="hi">/ कुल आगंतुक</span>: <b>{Number(total).toLocaleString('en-IN')}</b>
    </p>
  );
}
