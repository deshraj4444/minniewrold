import { useState } from 'react';
import { Link, NavLink, Route, Routes, useLocation } from 'react-router';
import { useFonts } from '../lib/hooks.js';
import AstroHome from './pages/AstroHome.jsx';
import HoroscopePage from './pages/HoroscopePage.jsx';
import KundaliPage from './pages/KundaliPage.jsx';
import MatchingPage from './pages/MatchingPage.jsx';
import PanchangPage from './pages/PanchangPage.jsx';
import NumerologyPage from './pages/NumerologyPage.jsx';
import ConsultationPage from './pages/ConsultationPage.jsx';
import ContactPage from './pages/ContactPage.jsx';
import VarshphalPage from './pages/VarshphalPage.jsx';
import { WhatsAppFloat } from './components/WhatsApp.jsx';
import './astro.css';

export const astroNav = [
  { to: '/astro/horoscope', label: 'Horoscope', icon: '☀' },
  { to: '/astro/kundali', label: 'Free Kundali', icon: '◈' },
  { to: '/astro/varshphal', label: 'Varshphal', icon: '✶' },
  { to: '/astro/matching', label: 'Kundli Milan', icon: '❤' },
  { to: '/astro/panchang', label: 'Panchang', icon: '☾' },
  { to: '/astro/numerology', label: 'Numerology', icon: '#' },
  { to: '/astro/consultation', label: 'Talk to Astrologer', icon: '☎' }
];

export default function AstroApp() {
  useFonts('https://fonts.googleapis.com/css2?family=Poppins:wght@400;500;600;700&family=Tiro+Devanagari+Hindi&display=swap');
  const [menuOpen, setMenuOpen] = useState(false);
  const { pathname } = useLocation();

  return (
    <div className="astro">
      <div className="astro-topbar">
        <span>Free Vedic astrology tools · Lahiri ayanamsa</span>
        <Link to="/">MinnieWorld home</Link>
      </div>
      <header className="astro-header">
        <div className="astro-container astro-header-inner">
          <Link to="/astro" className="astro-logo" onClick={() => setMenuOpen(false)}>
            <span className="astro-logo-mark" aria-hidden="true">ॐ</span>
            <span>Astro<b>Aura</b></span>
          </Link>
          <button className="astro-menu-btn" aria-expanded={menuOpen} aria-controls="astro-nav" onClick={() => setMenuOpen((o) => !o)}>
            <span className="visually-hidden">Menu</span>{menuOpen ? '✕' : '☰'}
          </button>
          <nav id="astro-nav" className={menuOpen ? 'astro-nav open' : 'astro-nav'}>
            {astroNav.map((item) => (
              <NavLink key={item.to} to={item.to} onClick={() => setMenuOpen(false)}>{item.label}</NavLink>
            ))}
          </nav>
        </div>
      </header>

      <main key={pathname.split('/')[2] ?? 'home'}>
        <Routes>
          <Route index element={<AstroHome />} />
          <Route path="horoscope" element={<HoroscopePage />} />
          <Route path="horoscope/:sign" element={<HoroscopePage />} />
          <Route path="kundali" element={<KundaliPage />} />
          <Route path="varshphal" element={<VarshphalPage />} />
          <Route path="matching" element={<MatchingPage />} />
          <Route path="panchang" element={<PanchangPage />} />
          <Route path="numerology" element={<NumerologyPage />} />
          <Route path="consultation" element={<ConsultationPage />} />
          <Route path="contact" element={<ContactPage />} />
          <Route path="*" element={<div className="astro-container astro-section"><h1>Page not found</h1><Link to="/astro">Back to AstroAura</Link></div>} />
        </Routes>
      </main>

      <WhatsAppFloat />

      <footer className="astro-footer">
        <div className="astro-container astro-footer-grid">
          <div>
            <div className="astro-logo"><span className="astro-logo-mark" aria-hidden="true">ॐ</span><span>Astro<b>Aura</b></span></div>
            <p>Free Vedic astrology: Janam Kundali, Kundli Milan, Panchang, horoscopes and numerology.</p>
          </div>
          <div>
            <h3>Free tools</h3>
            {astroNav.slice(0, 6).map((i) => <Link key={i.to} to={i.to}>{i.label}</Link>)}
          </div>
          <div>
            <h3>AstroAura</h3>
            <Link to="/astro/consultation">Book a consultation</Link>
            <Link to="/astro/contact">Contact us</Link>
            <Link to="/dharma">Dharma Path blog</Link>
          </div>
        </div>
        <p className="astro-disclaimer">
          Calculations use standard astronomical algorithms and are intended for guidance. For important decisions, consult a qualified astrologer.
        </p>
      </footer>
    </div>
  );
}
