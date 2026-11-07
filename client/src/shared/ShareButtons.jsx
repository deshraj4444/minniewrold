import { useState } from 'react';

const icons = {
  facebook: <path d="M14 8h3V4h-3c-2.8 0-5 2.2-5 5v2H7v4h2v7h4v-7h3l1-4h-4V9c0-.6.4-1 1-1z" />,
  whatsapp: <path d="M12 2a10 10 0 0 0-8.6 15.1L2 22l5-1.3A10 10 0 1 0 12 2zm5.3 14.1c-.2.6-1.3 1.2-1.8 1.2-.5.1-1 .2-3.3-.7-2.8-1.1-4.5-3.9-4.7-4.1-.1-.2-1.1-1.5-1.1-2.8s.7-2 1-2.3c.2-.3.5-.3.7-.3h.5c.2 0 .4 0 .6.5l.8 2c.1.2.1.4 0 .5l-.4.6-.4.4c-.1.2-.3.3-.1.6.2.3.8 1.3 1.7 2.1 1.2 1 2.1 1.4 2.4 1.5.3.1.5.1.6-.1l.9-1c.2-.3.4-.2.6-.1l1.9.9c.3.1.5.2.5.3.1.2.1.6-.1 1.2z" />,
  x: <path d="M17.7 3H21l-7.2 8.2L22 21h-6.6l-5.2-6.8L4.3 21H1l7.7-8.8L1 3h6.8l4.7 6.2L17.7 3zm-1.2 16h1.8L6.6 4.9H4.7L16.5 19z" />,
  telegram: <path d="M21.9 4.3 18.8 19c-.2 1-.9 1.3-1.7.8l-4.7-3.5-2.3 2.2c-.3.3-.5.5-1 .5l.3-4.8 8.8-7.9c.4-.3-.1-.5-.6-.2L6.8 13l-4.6-1.4c-1-.3-1-1 .2-1.5l18-6.9c.8-.3 1.6.2 1.5 1.1z" />,
  linkedin: <path d="M4.98 3.5a2.5 2.5 0 1 1 0 5 2.5 2.5 0 0 1 0-5zM3 9h4v12H3zM9 9h3.8v1.7h.1c.5-1 1.8-2 3.8-2 4 0 4.8 2.7 4.8 6.1V21h-4v-5.4c0-1.3 0-3-1.8-3s-2.1 1.4-2.1 2.9V21H9z" />,
  instagram: <path d="M12 7a5 5 0 1 0 0 10 5 5 0 0 0 0-10zm0 8.2a3.2 3.2 0 1 1 0-6.4 3.2 3.2 0 0 1 0 6.4zM17.3 5.5a1.2 1.2 0 1 0 0 2.4 1.2 1.2 0 0 0 0-2.4zM12 2c-2.7 0-3 0-4.1.1C4.3 2.3 2.3 4.3 2.1 7.9 2 9 2 9.3 2 12s0 3 .1 4.1c.2 3.6 2.2 5.6 5.8 5.8 1.1.1 1.4.1 4.1.1s3 0 4.1-.1c3.6-.2 5.6-2.2 5.8-5.8.1-1.1.1-1.4.1-4.1s0-3-.1-4.1c-.2-3.6-2.2-5.6-5.8-5.8C15 2 14.7 2 12 2zm0 1.8c2.7 0 3 0 4 .1 2.7.1 4 1.4 4.1 4.1.1 1 .1 1.3.1 4s0 3-.1 4c-.1 2.7-1.4 4-4.1 4.1-1 .1-1.3.1-4 .1s-3 0-4-.1c-2.7-.1-4-1.4-4.1-4.1-.1-1-.1-1.3-.1-4s0-3 .1-4C4 5.3 5.3 4 8 3.9c1-.1 1.3-.1 4-.1z" />,
  link: <path d="M10.6 13.4a1 1 0 0 0 1.4 0l4-4a3 3 0 1 0-4.2-4.2l-1.5 1.5 1.4 1.4 1.5-1.5a1 1 0 1 1 1.4 1.4l-4 4a1 1 0 0 0 0 1.4zm2.8-2.8a1 1 0 0 0-1.4 0l-4 4a3 3 0 1 0 4.2 4.2l1.5-1.5-1.4-1.4-1.5 1.5a1 1 0 1 1-1.4-1.4l4-4a1 1 0 0 0 0-1.4z" />
};

const Icon = ({ name }) => <svg viewBox="0 0 24 24" width="18" height="18" fill="currentColor" aria-hidden="true">{icons[name]}</svg>;

/**
 * Share links for Facebook, WhatsApp, X, Telegram and LinkedIn, plus copy-link.
 * Instagram has no web share URL: on phones the Instagram button opens the native share sheet
 * (which includes Instagram); elsewhere it copies the link for pasting into a bio or Story.
 */
export default function ShareButtons({ title, url, className = 'share' }) {
  const [copied, setCopied] = useState('');
  const link = url ?? (typeof window !== 'undefined' ? window.location.href : '');
  const u = encodeURIComponent(link);
  const t = encodeURIComponent(title ?? '');

  const targets = [
    { key: 'facebook', label: 'Facebook', href: `https://www.facebook.com/sharer/sharer.php?u=${u}` },
    { key: 'whatsapp', label: 'WhatsApp', href: `https://wa.me/?text=${t}%20${u}` },
    { key: 'x', label: 'X', href: `https://twitter.com/intent/tweet?text=${t}&url=${u}` },
    { key: 'telegram', label: 'Telegram', href: `https://t.me/share/url?url=${u}&text=${t}` },
    { key: 'linkedin', label: 'LinkedIn', href: `https://www.linkedin.com/sharing/share-offsite/?url=${u}` }
  ];

  async function copy(message) {
    try {
      await navigator.clipboard.writeText(link);
      setCopied(message);
    } catch {
      setCopied('Copy failed. Please copy the address bar.');
    }
    setTimeout(() => setCopied(''), 3000);
  }

  async function instagram() {
    if (navigator.share) {
      try {
        await navigator.share({ title, url: link });
        return;
      } catch (e) {
        if (e?.name === 'AbortError') return;
      }
    }
    copy('Link copied. Paste it in your Instagram bio or Story link sticker.');
  }

  return (
    <div className={className}>
      {targets.map((s) => (
        <a key={s.key} className={`share-btn share-${s.key}`} href={s.href} target="_blank" rel="noopener noreferrer" aria-label={`Share on ${s.label}`} title={s.label}>
          <Icon name={s.key} />
        </a>
      ))}
      <button type="button" className="share-btn share-instagram" onClick={instagram} aria-label="Share to Instagram" title="Instagram">
        <Icon name="instagram" />
      </button>
      <button type="button" className="share-btn share-link" onClick={() => copy('Link copied!')} aria-label="Copy link" title="Copy link">
        <Icon name="link" />
      </button>
      <span className="share-status" role="status">{copied}</span>
    </div>
  );
}
