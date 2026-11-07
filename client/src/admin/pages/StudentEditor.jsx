import { useEffect, useState } from 'react';
import { Link, useLocation, useNavigate, useParams } from 'react-router';
import { api } from '../../lib/api.js';
import { formatDate } from '../../lib/format.js';
import { Field, Notice } from '../components.jsx';

const empty = { fullName: '', email: '', phone: '', password: '', hasAllAccess: false, allAccessUntil: '', isDisabled: false };
const statusPill = { Approved: 'pill pill-green', Pending: 'pill pill-blue', Rejected: 'pill pill-grey', Locked: 'pill pill-grey' };

/** A random, easy-to-read password the admin can pass on to the student. */
function generatePassword() {
  const chars = 'abcdefghjkmnpqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ23456789';
  const bytes = crypto.getRandomValues(new Uint8Array(10));
  return Array.from(bytes, (b) => chars[b % chars.length]).join('');
}

export default function StudentEditor() {
  const { id } = useParams();
  const isNew = !id;
  const navigate = useNavigate();
  const [form, setForm] = useState(isNew ? { ...empty, password: generatePassword() } : null);
  const [detail, setDetail] = useState(null);
  const [notice, setNotice] = useState(null);
  const [saving, setSaving] = useState(false);
  const [busyTest, setBusyTest] = useState(null);
  // A new student's password is handed over from the create step so it can be copied once.
  const { state } = useLocation();
  const [lastPassword, setLastPassword] = useState(state?.password ?? '');

  async function load() {
    const d = await api.get(`/admin/students/${id}`);
    setDetail(d);
    setForm({
      fullName: d.fullName, email: d.email, phone: d.phone, password: '',
      hasAllAccess: d.hasAllAccess, allAccessUntil: d.allAccessUntil ? d.allAccessUntil.slice(0, 10) : '', isDisabled: d.isDisabled
    });
  }

  useEffect(() => {
    if (!isNew) load().catch((e) => setNotice({ type: 'error', message: e.message }));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [id]);

  if (!form) return <p className="muted">Loading…</p>;

  const bind = (key) => ({ value: form[key] ?? '', onChange: (e) => setForm((f) => ({ ...f, [key]: e.target.value })) });
  const check = (key) => ({ checked: !!form[key], onChange: (e) => setForm((f) => ({ ...f, [key]: e.target.checked })) });

  async function save(e) {
    e.preventDefault();
    setSaving(true);
    setNotice(null);
    // The pass runs to the end of the chosen day.
    const body = { ...form, allAccessUntil: form.hasAllAccess && form.allAccessUntil ? `${form.allAccessUntil}T23:59:59Z` : null };
    try {
      if (isNew) {
        const { id: newId } = await api.post('/admin/students', body);
        navigate(`/admin/students/${newId}`, { replace: true, state: { password: form.password } });
        return;
      }
      await api.put(`/admin/students/${id}`, body);
      if (form.password) setLastPassword(form.password);
      setNotice({ type: 'ok', message: form.password ? 'Saved. The new password is shown below; the student has been signed out.' : 'Saved.' });
      await load();
    } catch (err) {
      setNotice({ type: 'error', message: err.message });
    } finally {
      setSaving(false);
    }
  }

  async function toggleTest(t) {
    setBusyTest(t.id);
    try {
      if (t.status === 'Approved' && t.granted) await api.del(`/admin/students/${id}/grants/${t.id}`);
      else await api.post(`/admin/students/${id}/grants`, { examId: t.id });
      await load();
    } catch (err) {
      setNotice({ type: 'error', message: err.message });
    } finally {
      setBusyTest(null);
    }
  }

  async function remove() {
    if (!confirm(`Delete ${detail.fullName}? Their test results and payments records will be deleted too. This cannot be undone.`)) return;
    await api.del(`/admin/students/${id}`);
    navigate('/admin/students', { replace: true });
  }

  const loginText = `Your Curious Minds login\nWebsite: ${window.location.origin}/gk/login\nEmail: ${form.email}\nPassword: ${lastPassword}`;
  const phone = form.phone.replace(/\D/g, '').replace(/^(\d{10})$/, '91$1');

  return (
    <>
      <div className="admin-head">
        <h1 className="admin-h1"><Link to="/admin/students">Students</Link> › {isNew ? 'Add student' : detail?.fullName}</h1>
        {!isNew && <button className="admin-btn admin-btn-ghost danger" onClick={remove}>Delete student</button>}
      </div>
      <Notice notice={notice} />

      {lastPassword && (
        <div className="admin-card admin-credentials">
          <b>Login details to send the student</b>
          <pre>{loginText}</pre>
          <div className="actions">
            <button type="button" className="admin-btn admin-btn-ghost" onClick={() => navigator.clipboard?.writeText(loginText)}>Copy</button>
            {phone.length >= 11 && <a className="admin-btn admin-btn-ghost" href={`https://wa.me/${phone}?text=${encodeURIComponent(loginText)}`} target="_blank" rel="noreferrer">Send on WhatsApp</a>}
          </div>
          <small className="muted">This password is shown only now. If it's lost, set a new one.</small>
        </div>
      )}

      <form className="admin-card admin-inline-form" onSubmit={save}>
        <div className="admin-grid-2">
          <Field label="Full name"><input {...bind('fullName')} required minLength={2} maxLength={120} /></Field>
          <Field label="Email (used to log in)"><input {...bind('email')} type="email" required maxLength={256} /></Field>
        </div>
        <div className="admin-grid-2">
          <Field label="Mobile number"><input {...bind('phone')} type="tel" maxLength={30} /></Field>
          <Field label={isNew ? 'Password' : 'New password'} hint={isNew ? 'At least 8 characters. A random one is filled in for you.' : 'Leave empty to keep the current password.'}>
            <div className="af-inline">
              <input {...bind('password')} required={isNew} minLength={8} maxLength={128} autoComplete="new-password" />
              <button type="button" className="admin-btn admin-btn-ghost" onClick={() => setForm((f) => ({ ...f, password: generatePassword() }))}>Generate</button>
            </div>
          </Field>
        </div>

        <fieldset className="admin-fieldset">
          <legend>Access</legend>
          <label className="af-check"><input type="checkbox" {...check('hasAllAccess')} /> <b>All-Access pass</b>: every paid mock test and previous-year paper is unlocked, without payment</label>
          {form.hasAllAccess && (
            <Field label="Pass valid until (optional)" hint="Leave empty for no end date.">
              <input {...bind('allAccessUntil')} type="date" />
            </Field>
          )}
          <label className="af-check"><input type="checkbox" {...check('isDisabled')} /> Disable this account (the student can't log in)</label>
        </fieldset>

        <div className="actions"><button className="admin-btn" disabled={saving}>{saving ? 'Saving…' : isNew ? 'Create student' : 'Save changes'}</button></div>
      </form>

      {detail && (
        <>
          <h2>Paid tests</h2>
          {form.hasAllAccess
            ? <p className="muted">The All-Access pass unlocks all of these. You can still unlock single tests in case the pass ends.</p>
            : <p className="muted">Unlock a test for this student without a payment, or take back one you unlocked.</p>}
          <div className="admin-table-wrap">
            <table className="admin-table">
              <thead><tr><th>Test</th><th>Price</th><th>Status</th><th /></tr></thead>
              <tbody>
                {detail.tests.map((t) => (
                  <tr key={t.id}>
                    <td>{t.title}</td>
                    <td>₹{t.price}</td>
                    <td><span className={statusPill[t.status] ?? 'pill pill-grey'}>{t.status === 'Approved' ? (t.granted ? 'Unlocked by admin' : 'Paid') : t.status}</span></td>
                    <td className="actions">
                      {t.status !== 'Approved' && <button className="link" disabled={busyTest === t.id} onClick={() => toggleTest(t)}>Unlock</button>}
                      {t.status === 'Approved' && t.granted && <button className="link danger" disabled={busyTest === t.id} onClick={() => toggleTest(t)}>Take back</button>}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <h2>Test history</h2>
          {detail.attempts.length === 0 && <p className="muted">No tests taken yet.</p>}
          {detail.attempts.length > 0 && (
            <div className="admin-table-wrap">
              <table className="admin-table">
                <thead><tr><th>Test</th><th>Started</th><th>Score</th></tr></thead>
                <tbody>
                  {detail.attempts.map((a) => (
                    <tr key={a.id}>
                      <td>{a.exam}</td>
                      <td>{formatDate(a.startedAt)}</td>
                      <td>{a.submittedAt ? `${Number(a.score)}/${a.maxScore}` : 'In progress'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
          <p className="muted small">Joined {formatDate(detail.createdAt)}{detail.lastLoginAt && ` · last login ${formatDate(detail.lastLoginAt)}`}</p>
        </>
      )}
    </>
  );
}
