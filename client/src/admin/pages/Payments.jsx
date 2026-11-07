import { useState } from 'react';
import { api } from '../../lib/api.js';
import { useAsync } from '../../lib/hooks.js';
import { formatDate } from '../../lib/format.js';
import { Notice } from '../components.jsx';

const filters = [['Pending', 'Waiting'], ['Approved', 'Approved'], ['Rejected', 'Rejected'], ['', 'All']];
const statusPill = { Pending: 'pill pill-blue', Approved: 'pill pill-green', Rejected: 'pill pill-grey' };
const when = (v) => formatDate(v, { day: 'numeric', month: 'short', hour: 'numeric', minute: '2-digit' });

export default function Payments() {
  const [status, setStatus] = useState('Pending');
  const { data, loading, reload } = useAsync(() => api.get(`/admin/payments${status ? `?status=${status}` : ''}`), [status]);
  const [notice, setNotice] = useState(null);
  const [busy, setBusy] = useState(null);

  async function act(p, action) {
    let body;
    if (action === 'reject') {
      const note = prompt('Reason shown to the student (optional):', "We couldn't find this payment. Please check the transaction ID and try again.");
      if (note === null) return;
      body = { note };
    }
    setBusy(p.id);
    try {
      await api.put(`/admin/payments/${p.id}/${action}`, body);
      setNotice({ type: 'ok', message: `${action === 'approve' ? 'Approved' : 'Rejected'}: ${p.student.fullName}, ${p.exam.title}.` });
      reload();
    } catch (e) {
      setNotice({ type: 'error', message: e.message });
    } finally {
      setBusy(null);
    }
  }

  const phone = (p) => p.replace(/\D/g, '').replace(/^(\d{10})$/, '91$1');

  return (
    <>
      <div className="admin-head">
        <h1 className="admin-h1">Payments</h1>
        <button className="admin-btn admin-btn-ghost" onClick={reload}>Refresh</button>
      </div>
      <p className="muted">
        Open Google Pay → your transaction history and find a payment of the right amount with the same <b>UPI transaction ID</b> (and the note
        MW…T…). If it's there, approve it: the test unlocks for that student straight away. If it isn't, reject it with a reason.
      </p>
      <Notice notice={notice} />
      <div className="admin-filter">
        {filters.map(([k, label]) => <button key={label} className={status === k ? 'active' : ''} onClick={() => setStatus(k)}>{label}</button>)}
      </div>
      {loading && <p className="muted">Loading…</p>}
      {data?.length === 0 && <p className="muted">{status === 'Pending' ? 'Nothing waiting. 🎉' : 'No payments.'}</p>}
      {data?.length > 0 && (
        <div className="admin-table-wrap">
          <table className="admin-table">
            <thead><tr><th>Submitted</th><th>Student</th><th>Test</th><th>Amount</th><th>UPI transaction ID</th><th>Status</th><th /></tr></thead>
            <tbody>
              {data.map((p) => (
                <tr key={p.id}>
                  <td>{when(p.createdAt)}</td>
                  <td>
                    <span className="strong">{p.student.fullName}</span>
                    <div className="muted small">{p.student.email}{p.student.phone && <> · <a href={`https://wa.me/${phone(p.student.phone)}`} target="_blank" rel="noreferrer">{p.student.phone}</a></>}</div>
                  </td>
                  <td>{p.exam.title}</td>
                  <td>₹{p.amount}</td>
                  <td><code className="admin-utr">{p.upiRef}</code></td>
                  <td>
                    <span className={statusPill[p.status]}>{p.status}</span>
                    {p.reviewedAt && <div className="muted small">{when(p.reviewedAt)}</div>}
                    {p.status === 'Rejected' && p.note && <div className="muted small">{p.note}</div>}
                  </td>
                  <td className="actions">
                    {p.status !== 'Approved' && <button className="admin-btn" disabled={busy === p.id} onClick={() => act(p, 'approve')}>Approve</button>}
                    {p.status !== 'Rejected' && <button className="link danger" disabled={busy === p.id} onClick={() => act(p, 'reject')}>Reject</button>}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
