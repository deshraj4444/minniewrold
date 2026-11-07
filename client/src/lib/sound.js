import { useEffect, useState } from 'react';

// Small sound effects made with the Web Audio API: no audio files to download. Visitors can switch them off; the
// choice is remembered on this device only (localStorage, which may be unavailable, so every access is guarded).

const KEY = 'gk.sound';
let ctx = null;
let enabled = (() => {
  try { return localStorage.getItem(KEY) !== 'off'; } catch { return true; }
})();
const listeners = new Set();

function audio() {
  if (!enabled || typeof window === 'undefined') return null;
  const Ctx = window.AudioContext || window.webkitAudioContext;
  if (!Ctx) return null;
  try {
    ctx ??= new Ctx();
    if (ctx.state === 'suspended') ctx.resume();
    return ctx;
  } catch {
    return null;
  }
}

/** Plays one note: frequency in Hz, start offset and length in seconds. */
function tone(freq, start = 0, length = 0.12, type = 'sine', volume = 0.08) {
  const a = audio();
  if (!a) return;
  const t = a.currentTime + start;
  const osc = a.createOscillator();
  const gain = a.createGain();
  osc.type = type;
  osc.frequency.setValueAtTime(freq, t);
  gain.gain.setValueAtTime(0.0001, t);
  gain.gain.exponentialRampToValueAtTime(volume, t + 0.01);
  gain.gain.exponentialRampToValueAtTime(0.0001, t + length);
  osc.connect(gain).connect(a.destination);
  osc.start(t);
  osc.stop(t + length + 0.02);
}

/** A soft tick when an option is chosen. */
export const playClick = () => tone(660, 0, 0.06, 'triangle', 0.06);
/** Two rising notes for a right answer. */
export const playCorrect = () => { tone(660, 0, 0.1); tone(990, 0.09, 0.16); };
/** A low buzz for a wrong answer. */
export const playWrong = () => { tone(220, 0, 0.18, 'square', 0.04); tone(180, 0.12, 0.2, 'square', 0.035); };
/** A short fanfare and crackle for a result worth celebrating. */
export function playCheer() {
  [523, 659, 784, 1047].forEach((f, i) => tone(f, i * 0.12, 0.22, 'triangle', 0.07));
  for (let i = 0; i < 6; i++) tone(1800 + Math.random() * 1200, 0.6 + i * 0.18 + Math.random() * 0.1, 0.05, 'square', 0.02);
}

export function setSoundEnabled(on) {
  enabled = on;
  try { localStorage.setItem(KEY, on ? 'on' : 'off'); } catch { /* storage unavailable */ }
  listeners.forEach((l) => l(on));
}

/** [soundOn, setSoundOn]: shared by every sound toggle on the page. */
export function useSound() {
  const [on, setOn] = useState(enabled);
  useEffect(() => {
    listeners.add(setOn);
    return () => listeners.delete(setOn);
  }, []);
  return [on, setSoundEnabled];
}
