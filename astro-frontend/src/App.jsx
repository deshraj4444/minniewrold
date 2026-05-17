import { useEffect, useMemo, useState } from 'react';
import { api } from './api';

const zodiacSigns = ['aries', 'taurus', 'gemini', 'cancer', 'leo', 'virgo', 'libra', 'scorpio', 'sagittarius', 'capricorn', 'aquarius', 'pisces'];
const fallbackBlogs = [
  {
    id: 1,
    title: 'How Daily Horoscopes Help You Plan Mindfully',
    excerpt: 'Use zodiac insights as a daily ritual for clarity, intention and balanced choices.',
    category: 'Horoscope',
    imageUrl: 'https://images.unsplash.com/photo-1532968961962-8a0cb3a2d4f5?auto=format&fit=crop&w=1200&q=80'
  }
];
const fallbackProducts = [
  { id: 1, name: 'Rudraksha Mala', category: 'Mala', price: 1499, description: 'Traditional meditation mala for mantra chanting.', imageUrl: 'https://images.unsplash.com/photo-1602173574767-37ac01994b2a?auto=format&fit=crop&w=900&q=80' },
  { id: 2, name: 'Brass Diya Set', category: 'Puja Essentials', price: 899, description: 'Elegant brass diyas for home altar lighting.', imageUrl: 'https://images.unsplash.com/photo-1605368380945-7472b7fb0697?auto=format&fit=crop&w=900&q=80' },
  { id: 3, name: 'Crystal Healing Kit', category: 'Crystals', price: 2199, description: 'Curated crystals for focus and positive energy.', imageUrl: 'https://images.unsplash.com/photo-1515562141207-7a88fb7ce338?auto=format&fit=crop&w=900&q=80' }
];
const fallbackTestimonials = [
  { id: 1, customerName: 'Ananya Sharma', location: 'Delhi', quote: 'The kundali consultation was thoughtful, practical and easy to understand.', rating: 5, avatarUrl: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=300&q=80' },
  { id: 2, customerName: 'Rohit Mehta', location: 'Mumbai', quote: 'Daily guidance helped me bring structure to important decisions.', rating: 5, avatarUrl: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?auto=format&fit=crop&w=300&q=80' }
];

function App() {
  const [mobileOpen, setMobileOpen] = useState(false);
  const [activeSign, setActiveSign] = useState('aries');
  const [horoscope, setHoroscope] = useState(null);
  const [blogs, setBlogs] = useState(fallbackBlogs);
  const [products, setProducts] = useState(fallbackProducts);
  const [testimonials, setTestimonials] = useState(fallbackTestimonials);
  const [status, setStatus] = useState('');
  const [blogForm, setBlogForm] = useState({ title: '', excerpt: '', content: '', category: 'Horoscope', imageUrl: '', isPublished: true });

  useEffect(() => {
    api.getBlogs().then(setBlogs).catch(() => setBlogs(fallbackBlogs));
    api.getProducts().then(setProducts).catch(() => setProducts(fallbackProducts));
    api.getTestimonials().then(setTestimonials).catch(() => setTestimonials(fallbackTestimonials));
  }, []);

  useEffect(() => {
    api.getDailyHoroscope(activeSign)
      .then(setHoroscope)
      .catch(() => setHoroscope({ sign: titleCase(activeSign), date: new Date().toISOString().slice(0, 10), content: 'Today is a good day to slow down, observe your thoughts and choose one meaningful step forward.', source: 'Offline fallback' }));
  }, [activeSign]);

  const navItems = useMemo(() => ['Horoscope', 'Kundali', 'Blogs', 'Products', 'Testimonials', 'Contact', 'Admin'], []);

  async function handleContactSubmit(event) {
    event.preventDefault();
    const data = Object.fromEntries(new FormData(event.currentTarget));
    await api.submitContact(data);
    setStatus('Thank you. Our astrology desk will contact you soon.');
    event.currentTarget.reset();
  }

  async function handleKundaliSubmit(event) {
    event.preventDefault();
    const data = Object.fromEntries(new FormData(event.currentTarget));
    await api.submitKundali(data);
    setStatus('Kundali request submitted successfully.');
    event.currentTarget.reset();
  }

  async function handleBlogSubmit(event) {
    event.preventDefault();
    const created = await api.createBlog(blogForm);
    setBlogs([created, ...blogs]);
    setBlogForm({ title: '', excerpt: '', content: '', category: 'Horoscope', imageUrl: '', isPublished: true });
    setStatus('Blog created from admin panel.');
  }

  return (
    <div className="app-shell">
      <header className="site-header">
        <a href="#home" className="brand"><span className="icon">☾</span> AstroAura</a>
        <nav className={mobileOpen ? 'nav-links open' : 'nav-links'}>
          {navItems.map((item) => <a key={item} href={`#${item.toLowerCase()}`} onClick={() => setMobileOpen(false)}>{item}</a>)}
        </nav>
        <a className="header-cta" href="#kundali">Book Kundali</a>
        <button className="menu-button" onClick={() => setMobileOpen(!mobileOpen)} aria-label="Toggle menu">
          {mobileOpen ? '×' : '☰'}
        </button>
      </header>

      <main>
        <section id="home" className="hero section-grid">
          <div className="hero-copy">
            <span className="eyebrow"><span className="icon">✦</span> Premium astrology platform</span>
            <h1>Daily horoscopes, kundali guidance and spiritual products in one modern portal.</h1>
            <p>AstroAura combines live daily horoscope updates, admin-managed blogs, kundali service requests, testimonials, contact leads and a curated religious products showcase.</p>
            <div className="hero-actions">
              <a className="primary-button" href="#horoscope">View horoscope</a>
              <a className="secondary-button" href="#contact">Contact astrologer</a>
            </div>
            <div className="trust-row">
              <span><span className="icon">★</span> 4.9 customer rating</span>
              <span><span className="icon">◷</span> Daily updates</span>
              <span><span className="icon">◆</span> Authentic remedies</span>
            </div>
          </div>
          <div className="hero-card">
            <div className="hero-icon">☀</div>
            <h2>{horoscope?.sign ?? titleCase(activeSign)}</h2>
            <p>{horoscope?.content}</p>
            <small>Source: {horoscope?.source ?? 'Loading'} • {horoscope?.date}</small>
          </div>
        </section>

        <section id="horoscope" className="content-section">
          <SectionTitle eyebrow="Daily horoscope" title="Choose your zodiac sign" description="Fetched through the .NET API and cached in MSSQL for the day." />
          <div className="zodiac-grid">
            {zodiacSigns.map((sign) => (
              <button key={sign} className={activeSign === sign ? 'zodiac active' : 'zodiac'} onClick={() => setActiveSign(sign)}>{titleCase(sign)}</button>
            ))}
          </div>
        </section>

        <section id="kundali" className="content-section split-panel">
          <div>
            <SectionTitle eyebrow="Kundali services" title="Request a personalized horoscope consultation" description="Customers can submit birth details and service notes directly to the API." />
            <ul className="feature-list">
              <li>Birth chart and planetary house analysis</li>
              <li>Marriage, career, health and relationship consultations</li>
              <li>Stored in MSSQL for admin follow-up</li>
            </ul>
          </div>
          <FormCard onSubmit={handleKundaliSubmit} submitLabel="Submit kundali request">
            <input name="fullName" placeholder="Full name" required />
            <input name="email" type="email" placeholder="Email" required />
            <input name="phone" placeholder="Phone" required />
            <input name="birthDate" type="date" required />
            <input name="birthTime" type="time" required />
            <input name="birthPlace" placeholder="Birth place" required />
            <select name="serviceType" defaultValue="Kundali Consultation"><option>Kundali Consultation</option><option>Marriage Matching</option><option>Career Guidance</option></select>
            <textarea name="notes" placeholder="Questions or notes" />
          </FormCard>
        </section>

        <section id="blogs" className="content-section">
          <SectionTitle eyebrow="Astrology blog" title="Admin-managed spiritual insights" description="Published posts are loaded from the backend, while admin can create new entries below." />
          <div className="card-grid">
            {blogs.map((blog) => <BlogCard key={blog.id} blog={blog} />)}
          </div>
        </section>

        <section id="products" className="content-section">
          <SectionTitle eyebrow="Spiritual store" title="Religious products and remedies" description="Display curated products such as malas, diyas, incense, crystals and puja essentials." />
          <div className="card-grid product-grid">
            {products.map((product) => <ProductCard key={product.id} product={product} />)}
          </div>
        </section>

        <section id="testimonials" className="content-section">
          <SectionTitle eyebrow="Testimonials" title="Trusted by seekers and families" description="Customer reviews are powered by API data seeded in MSSQL." />
          <div className="testimonial-grid">
            {testimonials.map((testimonial) => <TestimonialCard key={testimonial.id} testimonial={testimonial} />)}
          </div>
        </section>

        <section id="contact" className="content-section split-panel">
          <div>
            <SectionTitle eyebrow="Contact us" title="Talk with the AstroAura consultation team" description="Capture customer messages and consultation requests from the public website." />
            <div className="contact-lines"><span><span className="icon">☎</span> +91 96462 22201</span><span><span className="icon">✉</span> care@astroaura.example</span></div>
          </div>
          <FormCard onSubmit={handleContactSubmit} submitLabel="Send message">
            <input name="fullName" placeholder="Full name" required />
            <input name="email" type="email" placeholder="Email" required />
            <input name="phone" placeholder="Phone" required />
            <input name="subject" placeholder="Subject" required />
            <textarea name="message" placeholder="How can we help?" required />
          </FormCard>
        </section>

        <section id="admin" className="content-section admin-panel">
          <SectionTitle eyebrow="Admin" title="Manage blogs" description="Create blog content from the admin side. Connect authentication before production launch." />
          <form className="admin-form" onSubmit={handleBlogSubmit}>
            <input value={blogForm.title} onChange={(event) => setBlogForm({ ...blogForm, title: event.target.value })} placeholder="Blog title" required />
            <input value={blogForm.category} onChange={(event) => setBlogForm({ ...blogForm, category: event.target.value })} placeholder="Category" required />
            <input value={blogForm.imageUrl} onChange={(event) => setBlogForm({ ...blogForm, imageUrl: event.target.value })} placeholder="Image URL" required />
            <textarea value={blogForm.excerpt} onChange={(event) => setBlogForm({ ...blogForm, excerpt: event.target.value })} placeholder="Excerpt" required />
            <textarea value={blogForm.content} onChange={(event) => setBlogForm({ ...blogForm, content: event.target.value })} placeholder="Full content" required />
            <label className="checkbox"><input type="checkbox" checked={blogForm.isPublished} onChange={(event) => setBlogForm({ ...blogForm, isPublished: event.target.checked })} /> Publish now</label>
            <button className="primary-button" type="submit">Create blog</button>
          </form>
        </section>
      </main>

      {status && <div className="toast">{status}<button onClick={() => setStatus('')}>×</button></div>}
      <footer>© 2026 AstroAura. Built with React, .NET Core API and MSSQL.</footer>
    </div>
  );
}

function SectionTitle({ eyebrow, title, description }) {
  return <div className="section-title"><span className="eyebrow">{eyebrow}</span><h2>{title}</h2><p>{description}</p></div>;
}

function FormCard({ children, onSubmit, submitLabel }) {
  return <form className="form-card" onSubmit={onSubmit}>{children}<button className="primary-button" type="submit">{submitLabel}</button></form>;
}

function BlogCard({ blog }) {
  return <article className="content-card"><img src={blog.imageUrl} alt="" /><span>{blog.category}</span><h3>{blog.title}</h3><p>{blog.excerpt}</p></article>;
}

function ProductCard({ product }) {
  return <article className="content-card product-card"><img src={product.imageUrl} alt="" /><span>{product.category}</span><h3>{product.name}</h3><p>{product.description}</p><strong>₹{Number(product.price).toLocaleString('en-IN')}</strong><button className="secondary-button"><span className="icon">🛍</span> Enquire</button></article>;
}

function TestimonialCard({ testimonial }) {
  return <article className="testimonial-card"><span className="quote-icon">“</span><p>{testimonial.quote}</p><div><img src={testimonial.avatarUrl} alt="" /><span><strong>{testimonial.customerName}</strong><small>{testimonial.location}</small></span></div></article>;
}

function titleCase(value) {
  return value.charAt(0).toUpperCase() + value.slice(1);
}

export default App;
