// The IIT JEE section at /jee. Keep this list in step with MinnieWorld.Web/Services/Seo/JeePages.cs, which uses
// the same pages for the server-rendered HTML, the sitemap and the 301s from the old /gk URLs.

export const JEE_HUB = '/jee';
export const jeeHubHeading = 'IIT JEE preparation: free notes and chapter tests';

export const jeePages = [
  {
    path: '/jee/physics/kinematics-test', kind: 'test', slug: 'jee-main-physics-kinematics-chapter-test', subject: 'Physics', topic: 'Kinematics',
    title: 'JEE Main Kinematics Test: 30 Questions with Solutions', crumb: 'Kinematics test',
    description: 'Free 30-question JEE Main Physics Kinematics chapter test with +4/−1 marking, 60 minutes, and step-by-step solutions in English and Hindi.'
  },
  {
    path: '/jee/physics/kinematics-notes', kind: 'notes', slug: 'jee-physics-kinematics-notes', subject: 'Physics', topic: 'Kinematics',
    title: 'JEE Physics Kinematics Notes: Formulas, Graphs, Projectiles', crumb: 'Kinematics notes',
    description: 'Kinematics notes for JEE Main & Advanced: equations of motion, graphs, projectile and relative motion, circular motion and common mistakes.'
  }
];

export const jeePageFor = (kind, slug) => jeePages.find((p) => p.kind === kind && p.slug === slug);

/** The notes for a test's chapter, or the test for a chapter's notes. */
export const jeeCompanion = (page) => jeePages.find((p) => p.kind !== page.kind && p.subject === page.subject && p.topic === page.topic);

/** Where a test lives: its /jee page for JEE chapter tests, /gk/tests/<slug> for everything else. */
export const testHref = (slug) => jeePageFor('test', slug)?.path ?? `/gk/tests/${slug}`;

/** Where a Curious Minds article lives: its /jee page for JEE notes, /gk/posts/<slug> otherwise. */
export const postHref = (slug) => jeePageFor('notes', slug)?.path ?? `/gk/posts/${slug}`;

/** Rewrites links to the old /gk URLs inside content written before the move to /jee. */
export function rewriteOldLinks(text = '') {
  let out = text;
  for (const p of jeePages) out = out.replaceAll(p.kind === 'test' ? `/gk/tests/${p.slug}` : `/gk/posts/${p.slug}`, p.path);
  return out.replaceAll('/gk/jee', JEE_HUB);
}
