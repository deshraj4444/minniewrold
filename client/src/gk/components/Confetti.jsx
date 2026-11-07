import { useMemo } from 'react';

const colors = ['#f59e0b', '#ef4444', '#22c55e', '#3b82f6', '#a855f7', '#ec4899', '#facc15'];

/** A one-off burst of falling confetti (pure CSS; hidden for people who prefer reduced motion). */
export default function Confetti({ pieces = 80 }) {
  const bits = useMemo(() => Array.from({ length: pieces }, (_, i) => ({
    left: Math.random() * 100,
    delay: Math.random() * 0.8,
    duration: 2.2 + Math.random() * 1.8,
    size: 6 + Math.random() * 8,
    color: colors[i % colors.length],
    spin: Math.random() * 720 - 360,
    round: Math.random() < 0.3
  })), [pieces]);

  return (
    <div className="gk-confetti" aria-hidden="true">
      {bits.map((b, i) => (
        <i
          key={i}
          style={{
            left: `${b.left}%`, width: b.size, height: b.size * (b.round ? 1 : 0.45), background: b.color,
            borderRadius: b.round ? '50%' : 2, animationDelay: `${b.delay}s`, animationDuration: `${b.duration}s`, '--spin': `${b.spin}deg`
          }}
        />
      ))}
    </div>
  );
}
