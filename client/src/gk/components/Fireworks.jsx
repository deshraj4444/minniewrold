import { useEffect, useRef } from 'react';
import { playCheer } from '../../lib/sound.js';

/**
 * Fireworks (crackers) over the page for a few seconds, drawn on one canvas: no images or libraries. It doesn't
 * block clicks, and it's skipped for visitors who ask their device for reduced motion.
 */
export default function Fireworks({ seconds = 6 }) {
  const canvas = useRef(null);

  useEffect(() => {
    if (window.matchMedia?.('(prefers-reduced-motion: reduce)').matches) return undefined;
    const c = canvas.current;
    const g = c.getContext('2d');
    if (!g) return undefined;
    playCheer();

    const colors = ['#f59e0b', '#ef4444', '#22c55e', '#3b82f6', '#a855f7', '#ec4899', '#facc15'];
    const sparks = [];
    let raf = 0;
    let last = performance.now();
    let launchIn = 0;
    const until = last + seconds * 1000;

    const resize = () => { c.width = window.innerWidth * devicePixelRatio; c.height = window.innerHeight * devicePixelRatio; };
    resize();
    window.addEventListener('resize', resize);

    function burst() {
      const x = c.width * (0.15 + Math.random() * 0.7);
      const y = c.height * (0.12 + Math.random() * 0.35);
      const color = colors[Math.floor(Math.random() * colors.length)];
      const n = 60 + Math.floor(Math.random() * 40);
      for (let i = 0; i < n; i++) {
        const angle = (Math.PI * 2 * i) / n;
        const speed = (1.5 + Math.random() * 3.5) * devicePixelRatio;
        sparks.push({ x, y, vx: Math.cos(angle) * speed, vy: Math.sin(angle) * speed, life: 1, color });
      }
    }

    function frame(now) {
      const dt = Math.min(50, now - last) / 16.7;
      last = now;
      launchIn -= dt;
      if (now < until && launchIn <= 0) { burst(); launchIn = 18 + Math.random() * 22; }

      // Fade the previous frame for trails, then clear fully so nothing is left on the page.
      g.globalCompositeOperation = 'destination-out';
      g.fillStyle = 'rgba(0,0,0,0.25)';
      g.fillRect(0, 0, c.width, c.height);
      g.globalCompositeOperation = 'lighter';
      for (let i = sparks.length - 1; i >= 0; i--) {
        const s = sparks[i];
        s.x += s.vx * dt;
        s.y += s.vy * dt;
        s.vy += 0.05 * devicePixelRatio * dt;
        s.vx *= 0.985;
        s.life -= 0.012 * dt;
        if (s.life <= 0) { sparks.splice(i, 1); continue; }
        g.globalAlpha = s.life;
        g.fillStyle = s.color;
        g.beginPath();
        g.arc(s.x, s.y, 2.2 * devicePixelRatio, 0, Math.PI * 2);
        g.fill();
      }
      g.globalAlpha = 1;
      if (now < until || sparks.length) raf = requestAnimationFrame(frame);
      else g.clearRect(0, 0, c.width, c.height);
    }
    raf = requestAnimationFrame(frame);

    return () => { cancelAnimationFrame(raf); window.removeEventListener('resize', resize); };
  }, [seconds]);

  return <canvas ref={canvas} className="gk-fireworks" aria-hidden="true" />;
}
