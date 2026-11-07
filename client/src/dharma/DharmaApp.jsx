import { useState } from 'react';
import { Link, NavLink, Route, Routes, useLocation } from 'react-router';
import { useFonts } from '../lib/hooks.js';
import DharmaHome from './pages/DharmaHome.jsx';
import DharmaPosts from './pages/DharmaPosts.jsx';
import DharmaPost from './pages/DharmaPost.jsx';
import DharmaCourses from './pages/DharmaCourses.jsx';
import DharmaCourse from './pages/DharmaCourse.jsx';
import DharmaLesson from './pages/DharmaLesson.jsx';
import DharmaContact from './pages/DharmaContact.jsx';
import './dharma.css';


const nav = [
  { to: '/dharma', label: 'Home', end: true },
  { to: '/dharma/posts', label: 'Articles' },
  { to: '/dharma/posts?category=Mantras', label: 'मंत्र' },
  { to: '/dharma/posts?category=Stotras', label: 'स्तोत्र' },
  { to: '/dharma/posts?category=Temples', label: 'मंदिर' },
  { to: '/dharma/courses', label: 'Learning Series' },
  { to: '/dharma/contact', label: 'Contact' }
];

export default function DharmaApp() {
  useFonts('https://fonts.googleapis.com/css2?family=Cormorant+Garamond:wght@500;600;700&family=Lora:ital,wght@0,400;0,600;1,400&family=Tiro+Devanagari+Sanskrit&family=Noto+Serif+Devanagari:wght@400;600;700&display=swap');
  const [open, setOpen] = useState(false);
  const { pathname, search } = useLocation();
  // Several menu items point at /dharma/posts with a different ?category, so match the query too.
  const isActive = (to) => {
    const [path, query = ''] = to.split('?');
    if (pathname !== path) return false;
    return query ? search === `?${query}` : !new URLSearchParams(search).get('category');
  };

  return (
    <div className="dharma">
      <header className="dh-masthead">
        <Link to="/dharma" className="dh-brand">
          <span className="dh-om" aria-hidden="true">ॐ</span>
          <span className="dh-title">Dharma Path</span>
          <span className="dh-tagline">Sanatan wisdom, in simple words</span>
        </Link>
        <button className="dh-menu" aria-expanded={open} aria-controls="dh-nav" onClick={() => setOpen((o) => !o)}>{open ? 'Close' : 'Menu'}</button>
        <nav id="dh-nav" className={open ? 'dh-nav open' : 'dh-nav'}>
          {nav.map((n) => n.to.startsWith('/dharma/posts')
            ? <Link key={n.to} to={n.to} className={isActive(n.to) ? 'active' : ''} aria-current={isActive(n.to) ? 'page' : undefined} onClick={() => setOpen(false)}>{n.label}</Link>
            : <NavLink key={n.to} to={n.to} end={n.end} onClick={() => setOpen(false)}>{n.label}</NavLink>)}
        </nav>
        <div className="dh-border" aria-hidden="true" />
      </header>

      <main className="dh-main">
        <Routes>
          <Route index element={<DharmaHome />} />
          <Route path="posts" element={<DharmaPosts />} />
          <Route path="posts/:slug" element={<DharmaPost />} />
          <Route path="courses" element={<DharmaCourses />} />
          <Route path="courses/:slug" element={<DharmaCourse />} />
          <Route path="courses/:slug/:lessonSlug" element={<DharmaLesson />} />
          <Route path="contact" element={<DharmaContact />} />
          <Route path="*" element={<div className="dh-narrow dh-center"><h1>Page not found</h1><Link to="/dharma">Return home</Link></div>} />
        </Routes>
      </main>

      <footer className="dh-footer">
        <div className="dh-divider" aria-hidden="true"><span>❁</span></div>
        <p className="dh-shloka" lang="sa">सर्वे भवन्तु सुखिनः सर्वे सन्तु निरामयाः</p>
        <p className="dh-shloka-tr">May all be happy, may all be free from illness.</p>
        <nav className="dh-footer-nav">
          <Link to="/dharma/posts">Articles</Link>
          <Link to="/dharma/courses">Learning Series</Link>
          <Link to="/astro/panchang">Today's Panchang</Link>
          <Link to="/">MinnieWorld</Link>
        </nav>
        <p className="dh-copy">© {new Date().getFullYear()} Dharma Path</p>
      </footer>
    </div>
  );
}
