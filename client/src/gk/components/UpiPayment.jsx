import { useEffect, useState } from 'react';
import QRCode from 'qrcode';
import { api } from '../../lib/api.js';

const isPhone = () => /Android|iPhone|iPad/i.test(navigator.userAgent);

/**
 * Pay-by-UPI panel for a locked test. The student pays the site owner's UPI ID (QR on desktop, app link on phones),
 * then submits the 12-digit transaction ID; the owner approves it in admin and the test unlocks.
 */
export default function UpiPayment({ exam, onSubmitted }) {
  const { payment, price } = exam;
  const [qr, setQr] = useState('');
  const [upiRef, setUpiRef] = useState('');
  const [error, setError] = useState('');
  const [sending, setSending] = useState(false);
  const [copied, setCopied] = useState(false);

  const link = payment?.upiId
    ? `upi://pay?${new URLSearchParams({ pa: payment.upiId, pn: payment.payeeName || 'Curious Minds', am: `${price}.00`, cu: 'INR', tn: payment.reference }).toString()}`
    : '';

  useEffect(() => {
    if (link) QRCode.toDataURL(link, { width: 220, margin: 1 }).then(setQr).catch(() => setQr(''));
  }, [link]);

  if (!payment?.upiId) {
    return payment?.helpWhatsApp ? null : <p className="gk-muted">Online payment isn't set up yet. Please check back soon or contact us.</p>;
  }

  async function submit(e) {
    e.preventDefault();
    setSending(true);
    setError('');
    try {
      await api.exams.purchase(exam.slug, upiRef.replace(/\s/g, ''));
      onSubmitted();
    } catch (err) {
      setError(err.message);
    } finally {
      setSending(false);
    }
  }

  async function copy() {
    try {
      await navigator.clipboard.writeText(payment.upiId);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    } catch { /* clipboard unavailable */ }
  }

  return (
    <div className="gk-pay">
      <ol className="gk-pay-steps">
        <li>
          <b>Pay ₹{price}</b> with Google Pay, PhonePe, Paytm or any UPI app.
          {qr && <img src={qr} alt={`UPI QR code to pay ₹${price}`} width={180} height={180} />}
          <div className="gk-pay-id">
            <span>UPI ID: <b>{payment.upiId}</b></span>
            <button type="button" className="gk-link" onClick={copy}>{copied ? 'Copied ✓' : 'Copy'}</button>
          </div>
          {payment.payeeName && <small className="gk-muted">Paying to: {payment.payeeName}</small>}
          {isPhone() && <a className="gk-btn gk-pay-app" href={link}>Open UPI app to pay ₹{price}</a>}
          <small className="gk-muted">If you type the UPI ID yourself, add the note <b>{payment.reference}</b> so we can find your payment quickly.</small>
        </li>
        <li>
          <b>Enter the UPI transaction ID</b> from the payment receipt (12 digits, called “UPI transaction ID” or “UTR”).
          <form onSubmit={submit} className="gk-pay-form">
            <label htmlFor="upi-ref" className="visually-hidden">UPI transaction ID</label>
            <input
              id="upi-ref"
              inputMode="numeric"
              autoComplete="off"
              placeholder="e.g. 412345678901"
              value={upiRef}
              onChange={(e) => setUpiRef(e.target.value.replace(/[^\d\s]/g, ''))}
              maxLength={16}
              required
              pattern="\s*(\d\s*){12}"
              title="12 digits"
            />
            <button className="gk-btn" disabled={sending}>{sending ? 'Sending…' : 'Submit'}</button>
          </form>
          {error && <p className="gk-err">{error}</p>}
        </li>
        <li><b>We check the payment and unlock the test</b>, usually within a few hours. You can then take it as many times as you like.</li>
      </ol>
      {payment.helpWhatsApp && (
        <a className="gk-link" href={`https://wa.me/${payment.helpWhatsApp}?text=${encodeURIComponent(`Hi, I paid ₹${price} for "${exam.title}". Reference ${payment.reference}.`)}`} target="_blank" rel="noreferrer">
          Payment problem? Message us on WhatsApp
        </a>
      )}
    </div>
  );
}
