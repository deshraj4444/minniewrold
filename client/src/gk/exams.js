// How the tests are grouped on the site: by audience, then by exam. Each test's "category" (set in admin or in
// the seed file) picks its exam here. A category that isn't listed still shows, under "More tests".

export const audiences = [
  {
    key: 'class-6',
    name: 'Class 6 admission',
    nameHi: 'कक्षा 6 प्रवेश परीक्षा',
    icon: '🎒',
    blurb: 'Navodaya, Military School and Sainik School entrance tests for students of Class 5. All papers are free, and you get a certificate when you pass.',
    blurbHi: 'कक्षा 5 के विद्यार्थियों के लिए नवोदय, मिलिट्री स्कूल और सैनिक स्कूल प्रवेश परीक्षा। सभी पेपर मुफ़्त, पास होने पर प्रमाण पत्र।'
  },
  {
    key: 'teachers',
    name: 'Teacher eligibility (TET)',
    nameHi: 'शिक्षक पात्रता परीक्षा',
    icon: '👩‍🏫',
    blurb: 'HP TET and CTET mock tests and previous year papers for teacher recruitment.',
    blurbHi: 'शिक्षक भर्ती के लिए HP TET और CTET मॉक टेस्ट और पिछले वर्षों के प्रश्नपत्र।'
  },
  {
    key: 'jee',
    name: 'IIT JEE (Engineering entrance)',
    nameHi: 'आईआईटी जेईई',
    icon: '⚛️',
    blurb: 'Free chapter-wise notes and tests for JEE Main and Advanced in Physics, Chemistry and Maths, with JEE marking (+4 / −1) and a full solution to every question.',
    blurbHi: 'JEE Main और Advanced के लिए भौतिकी, रसायन और गणित के मुफ़्त अध्यायवार नोट्स और टेस्ट, JEE अंकन (+4 / −1) और हर प्रश्न का पूरा हल।'
  }
];

export const examGroups = [
  {
    key: 'jnvst-class-6', category: 'JNVST Class 6', audience: 'class-6', icon: '🏫', color: '#0f766e',
    name: 'Navodaya (JNVST) Class 6', nameHi: 'जवाहर नवोदय विद्यालय कक्षा 6',
    pattern: '80 questions · 100 marks · 2 hours · Mental Ability, Arithmetic, Language (EVS added from 2027)',
    patternHi: '80 प्रश्न · 100 अंक · 2 घंटे · मानसिक योग्यता, अंकगणित, भाषा'
  },
  {
    key: 'rms-cet-class-6', category: 'RMS CET Class 6', audience: 'class-6', icon: '🎖️', color: '#4d7c0f',
    name: 'Rashtriya Military School Class 6', nameHi: 'राष्ट्रीय मिलिट्री स्कूल कक्षा 6',
    pattern: '200 questions · 200 marks · 2½ hours · English, Intelligence, Maths, GK',
    patternHi: '200 प्रश्न · 200 अंक · 2½ घंटे · अंग्रेज़ी, बुद्धि परीक्षण, गणित, सामान्य ज्ञान'
  },
  {
    key: 'aissee-class-6', category: 'Sainik School AISSEE Class 6', audience: 'class-6', icon: '⚓', color: '#1e40af',
    name: 'Sainik School (AISSEE) Class 6', nameHi: 'सैनिक स्कूल कक्षा 6',
    pattern: '125 questions · 300 marks · 2½ hours · Language, Maths, Intelligence, GK',
    patternHi: '125 प्रश्न · 300 अंक · 2½ घंटे · भाषा, गणित, बुद्धि परीक्षण, सामान्य ज्ञान'
  },
  {
    key: 'hp-tet-tgt-arts', category: 'HP TET TGT Arts', audience: 'teachers', icon: '📚', color: '#c2502e',
    name: 'HP TET TGT (Arts)', nameHi: 'हिमाचल टेट टीजीटी (आर्ट्स)', pattern: '150 questions · 150 minutes'
  },
  {
    key: 'hp-tet-jbt', category: 'HP TET JBT', audience: 'teachers', icon: '✏️', color: '#6d3fc0',
    name: 'HP TET JBT', nameHi: 'हिमाचल टेट जेबीटी', pattern: '150 questions · 150 minutes'
  },
  {
    key: 'ctet-paper-1', category: 'CTET Paper I', audience: 'teachers', icon: '🧒', color: '#0369a1',
    name: 'CTET Paper I (Classes 1–5)', nameHi: 'सीटेट पेपर I', pattern: '150 questions · 150 minutes'
  },
  {
    key: 'ctet-paper-2-sst', category: 'CTET Paper II (Social Studies)', audience: 'teachers', icon: '🌏', color: '#2f7d4f',
    name: 'CTET Paper II (Social Studies)', nameHi: 'सीटेट पेपर II (सामाजिक अध्ययन)', pattern: '150 questions · 150 minutes'
  },
  {
    key: 'jee-main', category: 'JEE Main', audience: 'jee', href: '/jee', icon: '⚛️', color: '#7c2d12',
    name: 'JEE Main', nameHi: 'जेईई मेन', pattern: 'Chapter tests · +4 / −1 marking · Physics, Chemistry, Maths',
    patternHi: 'अध्यायवार टेस्ट · +4 / −1 अंकन · भौतिकी, रसायन, गणित'
  }
];

/** Kinds of test in the order they're listed on an exam's page. */
export const kindInfo = {
  'Sample test': { icon: '🎁', label: 'Free practice tests', labelHi: 'मुफ़्त अभ्यास टेस्ट' },
  'Expected paper': { icon: '🔮', label: 'Expected papers', labelHi: 'संभावित प्रश्नपत्र' },
  'Year pattern': { icon: '📅', label: 'Past 5 years (year-pattern papers)', labelHi: 'पिछले 5 वर्ष (वर्ष-पैटर्न प्रश्नपत्र)' },
  'Previous year': { icon: '📜', label: 'Previous year papers', labelHi: 'पिछले वर्षों के प्रश्नपत्र' },
  'Mock test': { icon: '📝', label: 'Full mock tests', labelHi: 'पूर्ण मॉक टेस्ट' },
  Sectional: { icon: '🎯', label: 'Sectional tests', labelHi: 'खंडवार टेस्ट' }
};

/** An exam's page: /gk/exams/<key>, or its own section (JEE Main lives at /jee). */
export const examHref = (group) => group.href ?? `/gk/exams/${group.key}`;

export function groupOf(category) {
  return examGroups.find((g) => g.category === category)
    ?? { key: `c-${encodeURIComponent(category)}`, category, audience: 'more', icon: '🧠', color: '#475569', name: category, nameHi: '', pattern: '' };
}

export function groupByKey(key) {
  return examGroups.find((g) => g.key === key) ?? (key.startsWith('c-') ? groupOf(decodeURIComponent(key.slice(2))) : null);
}

/** Orders tests within an exam: by kind (as in kindInfo), then newest year first, then title. */
export function sortTests(list) {
  const order = Object.keys(kindInfo);
  return [...list].sort((a, b) => (order.indexOf(a.kind) - order.indexOf(b.kind)) || ((b.year ?? 0) - (a.year ?? 0)) || a.title.localeCompare(b.title, 'en', { numeric: true }));
}

/** True for the school-admission exams, where pages use the playful style (stars, games, certificates). */
export const isKids = (category) => groupOf(category).audience === 'class-6';
