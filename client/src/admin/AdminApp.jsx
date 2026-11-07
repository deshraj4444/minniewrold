import { useEffect, useState } from 'react';
import { Link, NavLink, Navigate, Route, Routes } from 'react-router';
import { api } from '../lib/api.js';
import { useFonts, useTitle } from '../lib/hooks.js';
import Login from './pages/Login.jsx';
import Dashboard from './pages/Dashboard.jsx';
import PostsList from './pages/PostsList.jsx';
import PostEditor from './pages/PostEditor.jsx';
import CoursesList from './pages/CoursesList.jsx';
import CourseEditor from './pages/CourseEditor.jsx';
import LessonEditor from './pages/LessonEditor.jsx';
import Consultations from './pages/Consultations.jsx';
import Messages from './pages/Messages.jsx';
import Testimonials from './pages/Testimonials.jsx';
import Sites from './pages/Sites.jsx';
import ExamsList from './pages/ExamsList.jsx';
import ExamEditor from './pages/ExamEditor.jsx';
import Students from './pages/Students.jsx';
import Settings from './pages/Settings.jsx';
import Payments from './pages/Payments.jsx';
import StudentEditor from './pages/StudentEditor.jsx';
import './admin.css';

const nav = [
  { to: '/admin', label: 'Dashboard', end: true },
  { group: 'Blogs' },
  { to: '/admin/posts', label: 'Posts' },
  { to: '/admin/courses', label: 'Courses & lessons' },
  { group: 'Mock tests' },
  { to: '/admin/exams', label: 'Tests & questions' },
  { to: '/admin/payments', label: 'Payments' },
  { to: '/admin/students', label: 'Students' },
  { group: 'AstroAura' },
  { to: '/admin/consultations', label: 'Consultations' },
  { to: '/admin/testimonials', label: 'Testimonials' },
  { group: 'Site' },
  { to: '/admin/messages', label: 'Messages' },
  { to: '/admin/sites', label: 'Hub tiles' },
  { to: '/admin/settings', label: 'Settings' }
];

export default function AdminApp() {
  useFonts('https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&display=swap');
  useTitle('Admin | MinnieWorld');
  const [user, setUser] = useState(undefined);
  const [menu, setMenu] = useState(false);

  useEffect(() => {
    api.get('/auth/me').then(setUser).catch(() => setUser(null));
  }, []);

  async function logout() {
    await api.post('/auth/logout');
    setUser(null);
  }

  if (user === undefined) return <div className="admin admin-center">Loading…</div>;
  if (user === null) return <div className="admin"><Login onLogin={setUser} /></div>;

  return (
    <div className="admin admin-shell">
      <aside className={menu ? 'admin-side open' : 'admin-side'}>
        <Link to="/admin" className="admin-brand">MinnieWorld <span>admin</span></Link>
        <nav onClick={() => setMenu(false)}>
          {nav.map((n) => n.group
            ? <p key={n.group} className="admin-nav-group">{n.group}</p>
            : <NavLink key={n.to} to={n.to} end={n.end}>{n.label}</NavLink>)}
        </nav>
        <div className="admin-side-foot">
          <a href="/" target="_blank" rel="noreferrer">View site ↗</a>
          <span title={user.email}>{user.displayName}</span>
          <button onClick={logout}>Log out</button>
        </div>
      </aside>
      <div className="admin-body">
        <header className="admin-topbar">
          <button className="admin-burger" onClick={() => setMenu((m) => !m)} aria-expanded={menu}>☰<span className="visually-hidden">Menu</span></button>
          <span>{user.email}</span>
        </header>
        <main className="admin-main">
          <Routes>
            <Route index element={<Dashboard />} />
            <Route path="posts" element={<PostsList />} />
            <Route path="posts/new" element={<PostEditor />} />
            <Route path="posts/:id" element={<PostEditor />} />
            <Route path="courses" element={<CoursesList />} />
            <Route path="courses/new" element={<CourseEditor />} />
            <Route path="courses/:id" element={<CourseEditor />} />
            <Route path="courses/:courseId/lessons/new" element={<LessonEditor />} />
            <Route path="lessons/:id" element={<LessonEditor />} />
            <Route path="consultations" element={<Consultations />} />
            <Route path="messages" element={<Messages />} />
            <Route path="testimonials" element={<Testimonials />} />
            <Route path="sites" element={<Sites />} />
            <Route path="exams" element={<ExamsList />} />
            <Route path="exams/new" element={<ExamEditor />} />
            <Route path="exams/:id" element={<ExamEditor />} />
            <Route path="students" element={<Students />} />
            <Route path="students/new" element={<StudentEditor />} />
            <Route path="students/:id" element={<StudentEditor />} />
            <Route path="payments" element={<Payments />} />
            <Route path="settings" element={<Settings />} />
            <Route path="*" element={<Navigate to="/admin" replace />} />
          </Routes>
        </main>
      </div>
    </div>
  );
}
