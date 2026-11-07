import { api } from '../../lib/api.js';
import { useAsync, useTitle } from '../../lib/hooks.js';
import { BLOG } from '../config.js';
import { CourseCard } from './GkHome.jsx';

const blog = api.blog(BLOG);

export default function GkCourses() {
  useTitle('Courses | Curious Minds');
  const { data, loading } = useAsync(() => blog.courses(), []);

  return (
    <div className="gk-wrap gk-section">
      <h1 className="gk-page-title">Courses</h1>
      <p className="gk-lead">Short, exam-friendly courses. Read a lesson a day and you'll finish in a week.</p>
      {loading && <p className="gk-muted">Loading…</p>}
      {data?.length === 0 && <div className="gk-empty"><p>Courses are coming soon.</p></div>}
      <div className="gk-course-grid">{data?.map((c) => <CourseCard key={c.id} c={c} />)}</div>
    </div>
  );
}
