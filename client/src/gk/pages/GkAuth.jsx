import { useState } from 'react';
import { Link, Navigate, useNavigate, useSearchParams } from 'react-router';
import { api } from '../../lib/api.js';
import { useTitle } from '../../lib/hooks.js';
import { useStudent } from '../student.jsx';

/** Login and registration for students. `?next=` (a /gk path) is where to go afterwards. */
export default function GkAuth({ mode }) {
  const register = mode === 'register';
  useTitle(register ? 'Create account | Curious Minds' : 'Log in | Curious Minds');
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const { student, setStudent } = useStudent();
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState({});
  const [busy, setBusy] = useState(false);

  const nextParam = params.get('next') ?? '';
  const next = nextParam.startsWith('/gk') ? nextParam : '/gk/tests';
  if (student) return <Navigate to={next} replace />;

  async function submit(e) {
    e.preventDefault();
    const body = Object.fromEntries(new FormData(e.currentTarget));
    setBusy(true);
    setError('');
    setFieldErrors({});
    try {
      setStudent(register ? await api.students.register(body) : await api.students.login(body));
      navigate(next, { replace: true });
    } catch (err) {
      setError(err.message);
      setFieldErrors(err.errors ?? {});
      setBusy(false);
    }
  }

  const other = `${register ? '/gk/login' : '/gk/register'}${nextParam ? `?next=${encodeURIComponent(nextParam)}` : ''}`;
  const fe = (k) => fieldErrors[k]?.[0];

  return (
    <div className="gk-wrap gk-section gk-auth">
      <div className="gk-auth-card">
        <h1>{register ? 'Create your free account' : 'Welcome back'}</h1>
        <p className="gk-muted">{register ? 'Take unlimited mock tests, get instant results and track your progress.' : 'Log in to continue your tests.'}</p>
        <form className="gk-form" onSubmit={submit}>
          {error && <p className="gk-err" role="alert">{error}</p>}
          {register && <label>Full name<input name="fullName" required minLength={2} maxLength={120} autoComplete="name" aria-invalid={!!fe('fullName')} /></label>}
          <label>Email<input name="email" type="email" required autoComplete="email" aria-invalid={!!fe('email')} /></label>
          {register && <label>Mobile number <small className="gk-muted">(optional)</small><input name="phone" type="tel" maxLength={30} autoComplete="tel" /></label>}
          <label>
            Password
            <input name="password" type="password" required minLength={register ? 8 : 1} maxLength={128} autoComplete={register ? 'new-password' : 'current-password'} aria-invalid={!!fe('password')} />
            {register && <small className="gk-muted">At least 8 characters.</small>}
          </label>
          <button className="gk-btn" disabled={busy}>{busy ? 'Please wait…' : register ? 'Create account' : 'Log in'}</button>
        </form>
        <p className="gk-auth-switch">
          {register ? 'Already registered?' : 'New here?'} <Link to={other}>{register ? 'Log in' : 'Create a free account'}</Link>
        </p>
        {!register && <p className="gk-muted small">Forgot your password? <Link to="/gk/contact">Contact us</Link> and we'll help you reset it.</p>}
      </div>
    </div>
  );
}
