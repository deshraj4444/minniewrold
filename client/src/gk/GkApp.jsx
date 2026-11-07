import { useState } from 'react';
import { Link, Navigate, NavLink, Route, Routes } from 'react-router';
import { useFonts } from '../lib/hooks.js';
import GkHome from './pages/GkHome.jsx';
import GkPosts from './pages/GkPosts.jsx';
import GkPost from './pages/GkPost.jsx';
import GkCourses from './pages/GkCourses.jsx';
import GkCourse from './pages/GkCourse.jsx';
import GkLesson from './pages/GkLesson.jsx';
import GkContact from './pages/GkContact.jsx';
import GkTests from './pages/GkTests.jsx';
import GkTest from './pages/GkTest.jsx';
import GkAttempt from './pages/GkAttempt.jsx';
import GkAuth from './pages/GkAuth.jsx';
import GkMyTests from './pages/GkMyTests.jsx';
import GkExam from './pages/GkExam.jsx';
import GkPlay from './pages/GkPlay.jsx';
import GkCertificate from './pages/GkCertificate.jsx';
import { StudentProvider, useStudent } from './student.jsx';
import './gk.css';
import './class6.css';
import VisitorCounter from '../shared/VisitorCounter.jsx';
import LogoMark from './components/LogoMark.jsx';
import JeeHub from './pages/JeeHub.jsx';
import { JEE_HUB, jeePages } from './jee.js';
import { CBSE_ENTRY, CbseRoutes } from './cbse/CbsePages.jsx';
import SearchBox from './components/SearchBox.jsx';
import GoogleReview from './components/GoogleReview.jsx';

// The four entry points first (one per kind of learner), then the general pages.
const nav = [
  { to: '/gk/class-6', label: 'Below Class 6' },
  { to: CBSE_ENTRY, label: 'CBSE Class 6' },
  { to: JEE_HUB, label: 'IIT JEE' },
  { to: '/gk/teachers', label: 'Teachers (TET)' },
  { to: '/gk/posts', label: 'Explore' },
  { to: '/gk/contact', label: 'Contact' }
];

/** Curious Minds. Mounted at /gk, and again at /jee and /cbse for those sections (same header, footer and student login). */
export default function GkApp({ section = 'gk' }) {
  return <StudentProvider><GkShell section={section} /></StudentProvider>;
}

/** The /jee routes: the hub and one page per catalog entry (see jee.js). */
function JeeRoutes() {
  return (
    <Routes>
      <Route index element={<JeeHub />} />
      {jeePages.map((p) => (
        <Route key={p.path} path={p.path.slice(JEE_HUB.length + 1)}
          element={p.kind === 'test' ? <GkTest slug={p.slug} jeePage={p} /> : <GkPost slug={p.slug} jeePage={p} />} />
      ))}
      <Route path="*" element={<div className="gk-wrap gk-empty"><h1>Page not found</h1><Link to={JEE_HUB}>All IIT JEE chapters</Link></div>} />
    </Routes>
  );
}

function AccountLink({ onClick }) {
  const { student } = useStudent();
  if (student === undefined) return null;
  return student
    ? <NavLink to="/gk/my-tests" className="gk-account" onClick={onClick}>👤 {student.fullName.split(' ')[0]}</NavLink>
    : <NavLink to="/gk/login" className="gk-account" onClick={onClick}>Log in</NavLink>;
}

function GkShell({ section }) {
  useFonts('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Poppins:wght@400;500;600;700&family=Hind:wght@400;500;600&display=swap');
  const [open, setOpen] = useState(false);

  return (
    <div className="gk">
      <header className="gk-header">
        <div className="gk-wrap gk-header-inner">
          <Link to="/gk" className="gk-logo" onClick={() => setOpen(false)}>
            <LogoMark />Curious<span>Minds</span>
          </Link>
          <button className="gk-burger" aria-expanded={open} aria-controls="gk-nav" onClick={() => setOpen((o) => !o)}>
            <span className="visually-hidden">Menu</span><span aria-hidden="true">{open ? '✕' : '≡'}</span>
          </button>
          <div id="gk-nav" className={open ? 'gk-nav open' : 'gk-nav'}>
            <nav>{nav.map((n) => <NavLink key={n.to} to={n.to} onClick={() => setOpen(false)}>{n.label}</NavLink>)}</nav>
            <AccountLink onClick={() => setOpen(false)} />
            <SearchBox id="gk-q" placeholder="Search topics…" onSearched={() => setOpen(false)} />
            <Link to="/gk/teachers" className="gk-btn gk-header-cta" onClick={() => setOpen(false)}>Start free test</Link>
          </div>
        </div>
      </header>

      <main className="gk-main">
        {section === 'jee' ? <JeeRoutes /> : section === 'cbse' ? <CbseRoutes /> : (
        <Routes>
          <Route index element={<GkHome />} />
          <Route path="posts" element={<GkPosts />} />
          <Route path="posts/:slug" element={<GkPost />} />
          <Route path="courses" element={<GkCourses />} />
          <Route path="courses/:slug" element={<GkCourse />} />
          <Route path="courses/:slug/:lessonSlug" element={<GkLesson />} />
          <Route path="contact" element={<GkContact />} />
          <Route path="tests" element={<GkTests />} />
          <Route path="tests/:slug" element={<GkTest />} />
          <Route path="class-6" element={<GkTests audience="class-6" />} />
          <Route path="teachers" element={<GkTests audience="teachers" />} />
          <Route path="jee" element={<Navigate to={JEE_HUB} replace />} />
          <Route path="exams/:key" element={<GkExam />} />
          <Route path="play/:key" element={<GkPlay />} />
          <Route path="certificate/:code" element={<GkCertificate />} />
          <Route path="attempt/:id" element={<GkAttempt />} />
          <Route path="login" element={<GkAuth mode="login" />} />
          <Route path="register" element={<GkAuth mode="register" />} />
          <Route path="my-tests" element={<GkMyTests />} />
          <Route path="*" element={<div className="gk-wrap gk-empty"><h1>404: fact not found</h1><Link to="/gk">Go home</Link></div>} />
        </Routes>
        )}
      </main>

      <footer className="gk-footer">
        <div className="gk-wrap gk-footer-grid">
          <div className="gk-footer-brand">
            <Link to="/gk" className="gk-logo"><LogoMark />Curious<span>Minds</span></Link>
            <p>Bilingual (English + हिंदी) mock tests, previous year papers and study notes for teacher eligibility and school admission exams.</p>
            <VisitorCounter className="visitor-counter gk-visitors" />
            <GoogleReview />
          </div>
          <nav aria-label="Exams">
            <h3>Exams</h3>
            <Link to="/gk/exams/hp-tet-tgt-arts">HP TET TGT (Arts)</Link>
            <Link to="/gk/exams/hp-tet-jbt">HP TET JBT</Link>
            <Link to="/gk/exams/ctet-paper-1">CTET Paper I</Link>
            <Link to="/gk/exams/ctet-paper-2-sst">CTET Paper II (SST)</Link>
            <Link to={JEE_HUB}>IIT JEE Main</Link>
            <Link to="/gk/class-6">Class 6 admission (below Class 6)</Link>
            <Link to={CBSE_ENTRY}>CBSE Class 6</Link>
          </nav>
          <nav aria-label="Resources">
            <h3>Resources</h3>
            <Link to="/gk/tests">All mock tests</Link>
            <Link to="/gk/posts">Study articles</Link>
            <Link to="/gk/courses">Courses</Link>
            <Link to="/gk/my-tests">My results</Link>
          </nav>
          <nav aria-label="Company">
            <h3>Company</h3>
            <Link to="/gk/contact">Contact us</Link>
            <Link to="/">MinnieWorld</Link>
          </nav>
        </div>
        <div className="gk-wrap gk-footer-bottom">© {new Date().getFullYear()} Curious Minds · MinnieWorld. All rights reserved.</div>
      </footer>
    </div>
  );
}
