import { useEffect, useState } from 'react';
import { api } from '../../lib/api.js';

// Fetched once per page load and shared by every button.
let info;
const siteInfo = () => (info ??= api.siteInfo().catch(() => ({})));

/** "Rate us on Google", shown only when the admin has set the Google review link (admin → Settings). */
export default function GoogleReview({ className = 'gk-review' }) {
  const [url, setUrl] = useState(null);
  useEffect(() => { siteInfo().then((s) => setUrl(s?.googleReviewUrl ?? null)); }, []);
  if (!url) return null;
  return (
    <a className={className} href={url} target="_blank" rel="noopener noreferrer">
      <span aria-hidden="true">★★★★★</span> Rate us on Google <span lang="hi">/ Google पर रेटिंग दें</span>
    </a>
  );
}
