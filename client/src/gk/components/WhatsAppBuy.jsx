/**
 * "Buy on WhatsApp": opens a chat with the site owner with a ready-made message naming the test and the student's
 * account, so the owner can unlock it in admin (Students → Unlock) straight away. Messages only, no calls.
 */
export default function WhatsAppBuy({ exam, student, number }) {
  const digits = (number || '').replace(/\D/g, '');
  if (!digits) return null;

  const message = [
    `Hi, I want to buy this mock test / मुझे यह टेस्ट खरीदना है:`,
    `📝 ${exam.title}`,
    `💰 ₹${exam.price}`,
    student && `👤 ${student.fullName} (${student.email})`,
    exam.payment?.reference && `Ref: ${exam.payment.reference}`,
  ].filter(Boolean).join('\n');

  return (
    <div className="gk-wa-buy">
      <a className="gk-wa-btn" href={`https://wa.me/${digits}?text=${encodeURIComponent(message)}`} target="_blank" rel="noopener noreferrer">
        <svg viewBox="0 0 24 24" width="22" height="22" aria-hidden="true"><path fill="currentColor" d="M12.04 2a9.9 9.9 0 0 0-8.5 15l-1.4 5 5.2-1.36A9.9 9.9 0 1 0 12.04 2Zm0 18.1a8.2 8.2 0 0 1-4.2-1.15l-.3-.18-3.08.8.82-3-.2-.31a8.2 8.2 0 1 1 6.96 3.84Zm4.5-6.14c-.25-.12-1.46-.72-1.69-.8-.23-.08-.39-.12-.55.12-.16.25-.63.8-.78.97-.14.16-.29.18-.53.06a6.7 6.7 0 0 1-3.34-2.92c-.25-.43.25-.4.72-1.34.08-.16.04-.3-.02-.42-.06-.12-.55-1.33-.76-1.82-.2-.48-.4-.41-.55-.42h-.47a.9.9 0 0 0-.65.3 2.73 2.73 0 0 0-.85 2.03 4.74 4.74 0 0 0 1 2.52 10.86 10.86 0 0 0 4.16 3.67c1.55.67 2.16.73 2.93.61.47-.07 1.46-.6 1.66-1.17.2-.58.2-1.07.14-1.17-.06-.1-.22-.16-.47-.28Z" /></svg>
        <span><b>Buy on WhatsApp</b><small>व्हाट्सएप पर खरीदें</small></span>
      </a>
      <p className="gk-wa-note">
        Send us a message and we'll unlock the test on your account quickly, no waiting for payment checks.
        <br /><b>📩 Message only, no calls please.</b> <span lang="hi">केवल मैसेज करें, कॉल न करें।</span>
      </p>
    </div>
  );
}
