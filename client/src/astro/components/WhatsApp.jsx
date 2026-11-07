import { useEffect, useState } from 'react';
import { api } from '../../lib/api.js';

let request;
let cached = null;

/** The astrologer's WhatsApp number and greeting, set in admin → Settings. Fetched once per page load. */
export function useContactInfo() {
  const [info, setInfo] = useState(cached);
  useEffect(() => {
    if (cached) return;
    request ??= api.astro.contactInfo().catch(() => ({ whatsAppNumber: '' })).then((i) => (cached = i));
    let live = true;
    request.then((i) => live && setInfo(i));
    return () => { live = false; };
  }, []);
  return info;
}

export const whatsAppUrl = (number, text) => `https://wa.me/${number}${text ? `?text=${encodeURIComponent(text)}` : ''}`;

export function WhatsAppIcon() {
  return (
    <svg viewBox="0 0 24 24" aria-hidden="true" fill="currentColor">
      <path d="M12.04 2C6.58 2 2.13 6.45 2.13 11.91c0 1.75.46 3.45 1.32 4.95L2.05 22l5.25-1.38a9.9 9.9 0 0 0 4.74 1.21h.01c5.46 0 9.9-4.45 9.9-9.91A9.85 9.85 0 0 0 12.04 2Zm5.8 14.13c-.24.68-1.42 1.3-1.95 1.35-.5.05-.98.23-3.3-.69-2.8-1.1-4.56-3.96-4.7-4.15-.13-.18-1.12-1.49-1.12-2.84s.71-2.01.96-2.29c.25-.27.55-.34.73-.34h.52c.17 0 .4-.06.62.47.23.55.78 1.9.85 2.04.07.14.11.3.02.48-.09.18-.14.3-.27.46l-.41.48c-.14.14-.28.29-.12.56.16.27.71 1.17 1.52 1.9 1.05.93 1.93 1.22 2.2 1.36.28.14.44.11.6-.07.16-.18.69-.8.87-1.08.18-.27.37-.23.62-.14.25.09 1.6.75 1.87.89.28.14.46.2.53.32.07.12.07.66-.17 1.33Z" />
    </svg>
  );
}

/** Floating "Chat on WhatsApp" button shown on every AstroAura page once a number is configured. */
export function WhatsAppFloat() {
  const info = useContactInfo();
  if (!info?.whatsAppNumber) return null;
  const name = info.astrologerName || 'our astrologer';
  return (
    <a className="astro-wa-float" href={whatsAppUrl(info.whatsAppNumber, info.greeting || 'Namaste, I would like to consult the astrologer.')} target="_blank" rel="noreferrer" aria-label={`Chat with ${name} on WhatsApp`}>
      <WhatsAppIcon /><span>Chat on WhatsApp<small className="astro-wa-note">Message only, no calls</small></span>
    </a>
  );
}
