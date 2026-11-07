import { Link } from 'react-router';
import { api } from '../../lib/api.js';
import { useAsync } from '../../lib/hooks.js';

export default function Dashboard() {
  const { data: s } = useAsync(() => api.get('/admin/stats'), []);
  const tiles = s ? [
    { label: 'Payments to approve', value: s.pendingPayments, to: '/admin/payments', alert: s.pendingPayments > 0 },
    { label: 'Earned this month', value: `₹${s.revenueThisMonth}`, to: '/admin/payments' },
    { label: 'New consultations', value: s.newConsultations, to: '/admin/consultations', alert: s.newConsultations > 0 },
    { label: 'Unread messages', value: s.unreadMessages, to: '/admin/messages', alert: s.unreadMessages > 0 },
    { label: 'Students', value: s.students, to: '/admin/students' },
    { label: 'Tests started today', value: s.attemptsToday, to: '/admin/students' },
    { label: 'Mock tests', value: s.exams, to: '/admin/exams' },
    { label: 'Posts', value: s.posts, to: '/admin/posts' },
    { label: 'Courses', value: s.courses, to: '/admin/courses' },
    { label: 'Lessons', value: s.lessons, to: '/admin/courses' },
    { label: 'Testimonials', value: s.testimonials, to: '/admin/testimonials' }
  ] : [];

  return (
    <>
      <h1 className="admin-h1">Dashboard</h1>
      <div className="admin-stats">
        {tiles.map((t) => (
          <Link key={t.label} to={t.to} className={t.alert ? 'admin-stat alert' : 'admin-stat'}>
            <b>{t.value}</b><span>{t.label}</span>
          </Link>
        ))}
      </div>
      <div className="admin-quick">
        <Link to="/admin/posts/new?blog=dharma" className="admin-btn">+ Dharma Path post</Link>
        <Link to="/admin/posts/new?blog=gk" className="admin-btn">+ Curious Minds post</Link>
        <Link to="/admin/courses/new" className="admin-btn admin-btn-ghost">+ New course</Link>
        <Link to="/admin/exams/new" className="admin-btn admin-btn-ghost">+ New mock test</Link>
      </div>
    </>
  );
}
