import { lazy, Suspense, useEffect } from 'react';
import { Route, Routes, useLocation } from 'react-router';
import { dropPrerenderUnless } from './lib/prerender.js';

// Each site is its own bundle, so visitors to one never download the others.
const HubPage = lazy(() => import('./hub/HubPage.jsx'));
const AstroApp = lazy(() => import('./astro/AstroApp.jsx'));
const DharmaApp = lazy(() => import('./dharma/DharmaApp.jsx'));
const GkApp = lazy(() => import('./gk/GkApp.jsx'));
const AdminApp = lazy(() => import('./admin/AdminApp.jsx'));

function ScrollToTop() {
  const { pathname } = useLocation();
  // Braces matter: newer browsers return a Promise from scrollTo, which React would treat as a cleanup function.
  useEffect(() => { window.scrollTo(0, 0); }, [pathname]);
  // A server-rendered page copy (the /jee pages) belongs only to the URL it was rendered for.
  useEffect(() => { dropPrerenderUnless(pathname); }, [pathname]);
  return null;
}

export default function App() {
  return (
    <>
      <ScrollToTop />
      <Suspense fallback={<div className="page-loading" aria-busy="true" />}>
        <Routes>
          <Route path="/" element={<HubPage />} />
          <Route path="/astro/*" element={<AstroApp />} />
          <Route path="/dharma/*" element={<DharmaApp />} />
          <Route path="/gk/*" element={<GkApp />} />
          <Route path="/jee/*" element={<GkApp section="jee" />} />
          <Route path="/cbse/*" element={<GkApp section="cbse" />} />
          <Route path="/admin/*" element={<AdminApp />} />
          <Route path="*" element={<HubPage notFound />} />
        </Routes>
      </Suspense>
    </>
  );
}
