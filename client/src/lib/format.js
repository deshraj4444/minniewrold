export const formatDate = (value, opts = { day: 'numeric', month: 'short', year: 'numeric' }) =>
  value ? new Date(value).toLocaleDateString('en-IN', opts) : '';

/** "2026-10-22T06:26:09.44" (a local time from the API) → "6:26 AM". */
export const formatTime = (value) => {
  if (!value) return '—';
  const [, time] = value.split('T');
  const [h, m] = time.split(':').map(Number);
  const suffix = h >= 12 ? 'PM' : 'AM';
  return `${((h + 11) % 12) + 1}:${String(m).padStart(2, '0')} ${suffix}`;
};

/** Like formatTime but adds the date when it's not the reference day ("ends 2:48 PM" vs "ends 23 Oct, 1:10 AM"). */
export const formatEnd = (value, referenceDate) => {
  if (!value) return 'continues past tomorrow';
  const day = value.slice(0, 10);
  if (day === referenceDate) return formatTime(value);
  return `${formatDate(day, { day: 'numeric', month: 'short' })}, ${formatTime(value)}`;
};

export const minutesLabel = (m) => (m >= 60 ? `${Math.floor(m / 60)}h ${m % 60 ? `${m % 60}m` : ''}`.trim() : `${m} min`);

export const todayIso = () => {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
};
