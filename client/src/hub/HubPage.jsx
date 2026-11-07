import { Link } from 'react-router';
import { api } from '../lib/api.js';
import { useAsync, useFonts, useTitle } from '../lib/hooks.js';
import './hub.css';

const isInternal = (path) => path.startsWith('/');

export default function HubPage({ notFound = false }) {
  useFonts('https://fonts.googleapis.com/css2?family=Outfit:wght@400;600;800&display=swap');
  useTitle(notFound ? 'Page not found | MinnieWorld' : 'MinnieWorld');
  const { data: sites, loading, error } = useAsync(() => api.sites(), []);

  return (
    <div className="hub">
      <header className="hub-hero">
        <p className="hub-kicker">MinnieWorld</p>
        <h1>{notFound ? "That page doesn't exist." : 'One place, many worlds.'}</h1>
        <p className="hub-lead">
          {notFound ? 'Pick a site below to keep exploring.' : 'Astrology, spiritual wisdom and general knowledge, each in its own corner. Choose where to go.'}
        </p>
      </header>

      <main className="hub-grid" aria-busy={loading}>
        {error && <p className="hub-error">Couldn't load the sites. Please refresh.</p>}
        {loading && Array.from({ length: 3 }, (_, i) => <div key={i} className="hub-card is-skeleton" />)}
        {sites?.map((site) => {
          const inner = (
            <>
              <div className="hub-thumb" style={{ backgroundImage: site.thumbnailUrl ? `url("${site.thumbnailUrl}")` : undefined }} />
              <div className="hub-card-body">
                <h2>{site.name}</h2>
                <p>{site.tagline}</p>
                <span className="hub-go">Visit {site.name} <span aria-hidden="true">→</span></span>
              </div>
            </>
          );
          const style = { '--accent': site.accentColor };
          return isInternal(site.path)
            ? <Link key={site.id} to={site.path} className="hub-card" style={style}>{inner}</Link>
            : <a key={site.id} href={site.path} className="hub-card" style={style} target="_blank" rel="noopener noreferrer">{inner}</a>;
        })}
      </main>

      <footer className="hub-footer">© {new Date().getFullYear()} MinnieWorld</footer>
    </div>
  );
}
