export const zodiac = [
  { key: 'aries', name: 'Aries', hindi: 'Mesha', symbol: '♈︎', dates: 'Mar 21 – Apr 19', element: 'Fire', lord: 'Mars' },
  { key: 'taurus', name: 'Taurus', hindi: 'Vrishabha', symbol: '♉︎', dates: 'Apr 20 – May 20', element: 'Earth', lord: 'Venus' },
  { key: 'gemini', name: 'Gemini', hindi: 'Mithuna', symbol: '♊︎', dates: 'May 21 – Jun 20', element: 'Air', lord: 'Mercury' },
  { key: 'cancer', name: 'Cancer', hindi: 'Karka', symbol: '♋︎', dates: 'Jun 21 – Jul 22', element: 'Water', lord: 'Moon' },
  { key: 'leo', name: 'Leo', hindi: 'Simha', symbol: '♌︎', dates: 'Jul 23 – Aug 22', element: 'Fire', lord: 'Sun' },
  { key: 'virgo', name: 'Virgo', hindi: 'Kanya', symbol: '♍︎', dates: 'Aug 23 – Sep 22', element: 'Earth', lord: 'Mercury' },
  { key: 'libra', name: 'Libra', hindi: 'Tula', symbol: '♎︎', dates: 'Sep 23 – Oct 22', element: 'Air', lord: 'Venus' },
  { key: 'scorpio', name: 'Scorpio', hindi: 'Vrishchika', symbol: '♏︎', dates: 'Oct 23 – Nov 21', element: 'Water', lord: 'Mars' },
  { key: 'sagittarius', name: 'Sagittarius', hindi: 'Dhanu', symbol: '♐︎', dates: 'Nov 22 – Dec 21', element: 'Fire', lord: 'Jupiter' },
  { key: 'capricorn', name: 'Capricorn', hindi: 'Makara', symbol: '♑︎', dates: 'Dec 22 – Jan 19', element: 'Earth', lord: 'Saturn' },
  { key: 'aquarius', name: 'Aquarius', hindi: 'Kumbha', symbol: '♒︎', dates: 'Jan 20 – Feb 18', element: 'Air', lord: 'Saturn' },
  { key: 'pisces', name: 'Pisces', hindi: 'Meena', symbol: '♓︎', dates: 'Feb 19 – Mar 20', element: 'Water', lord: 'Jupiter' }
];

export const rashiNames = zodiac.map((z) => z.hindi);

export const grahaHindi = {
  Sun: 'Surya', Moon: 'Chandra', Mars: 'Mangal', Mercury: 'Budh', Jupiter: 'Guru',
  Venus: 'Shukra', Saturn: 'Shani', Rahu: 'Rahu', Ketu: 'Ketu'
};

export const services = [
  { to: '/astro/kundali', title: 'Free Janam Kundali', text: 'Birth chart, planets, nakshatra and Vimshottari dasha.', icon: '◈' },
  { to: '/astro/matching', title: 'Kundli Milan', text: '36-guna Ashtakoota matching for marriage.', icon: '❤' },
  { to: '/astro/panchang', title: "Today's Panchang", text: 'Tithi, nakshatra, Rahu Kaal and muhurat.', icon: '☾' },
  { to: '/astro/horoscope', title: 'Daily Horoscope', text: 'Daily, weekly and monthly predictions.', icon: '☀' },
  { to: '/astro/numerology', title: 'Numerology', text: 'Moolank, Bhagyank and lucky numbers.', icon: '#' },
  { to: '/astro/consultation', title: 'Ask an Astrologer', text: 'Personal guidance on career, marriage, health.', icon: '☎' }
];
