// North Indian (diamond) chart. Houses are fixed in place, starting with house 1 at the top centre and
// running anticlockwise; the number in each house is the rashi (1 = Mesha … 12 = Meena).
// Coordinates are for a 400×400 viewBox: where grahas are written, and where the rashi number goes.
const layout = {
  1: { g: [200, 92], n: [200, 182] },
  2: { g: [100, 34], n: [100, 84] },
  3: { g: [36, 100], n: [84, 100] },
  4: { g: [100, 192], n: [182, 200] },
  5: { g: [36, 300], n: [84, 300] },
  6: { g: [100, 360], n: [100, 318] },
  7: { g: [200, 292], n: [200, 222] },
  8: { g: [300, 360], n: [300, 318] },
  9: { g: [364, 300], n: [316, 300] },
  10: { g: [300, 192], n: [218, 200] },
  11: { g: [364, 100], n: [316, 100] },
  12: { g: [300, 34], n: [300, 84] }
};

const isDiamond = (house) => house % 3 === 1;

export default function NorthIndianChart({ houses, title, showAscendant = true }) {
  return (
    <figure className="nchart">
      <svg viewBox="0 0 400 400" role="img" aria-label={`${title} (North Indian style)`}>
        <rect x="1" y="1" width="398" height="398" className="nchart-frame" />
        <path d="M1 1 L399 399 M399 1 L1 399 M200 1 L1 200 L200 399 L399 200 Z" className="nchart-lines" />
        {houses.map((h) => {
          const { g, n } = layout[h.house];
          const items = showAscendant && h.house === 1 ? ['Asc', ...h.grahas] : h.grahas;
          // Triangles are narrow: two grahas per line; diamonds fit three.
          const perLine = isDiamond(h.house) ? 3 : 2;
          const lines = [];
          for (let i = 0; i < items.length; i += perLine) lines.push(items.slice(i, i + perLine).join(' '));
          const startY = g[1] - ((lines.length - 1) * 8);
          return (
            <g key={h.house}>
              <text x={n[0]} y={n[1]} className="nchart-num" textAnchor="middle" dominantBaseline="middle">{h.rashi + 1}</text>
              {lines.map((line, i) => (
                <text key={i} x={g[0]} y={startY + (i * 16)} className={line.includes('Asc') ? 'nchart-graha asc' : 'nchart-graha'} textAnchor="middle" dominantBaseline="middle">{line}</text>
              ))}
            </g>
          );
        })}
      </svg>
      {title && <figcaption>{title}</figcaption>}
    </figure>
  );
}
