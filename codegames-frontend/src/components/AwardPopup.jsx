import { useEffect, useRef } from 'react'
import AwardMark from './AwardMark.jsx'

export default function AwardPopup({ awards, onClose }) {
  const closeRef = useRef(null)
  const several = awards.length > 1

  useEffect(() => {
    closeRef.current?.focus()

    function onKey(event) {
      if (event.key === 'Escape') {
        onClose()
      }
    }

    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [onClose])

  return (
    <div className="dialog-backdrop award-pop" role="presentation">
      <div
        className="dialog award-dialog"
        role="dialog"
        aria-modal="true"
        aria-labelledby="award-title"
      >
        <h2 id="award-title">{several ? 'Nove nagrade' : 'Nova nagrada'}</h2>
        <ul className="award-pop-list">
          {awards.map((award) => (
            <li key={`${award.kind}-${award.threshold}`}>
              <span className="award-pop-icon">
                <AwardMark kind={award.kind} threshold={award.threshold} size={36} />
              </span>
              <strong>{award.title}</strong>
              <p>{award.description}</p>
            </li>
          ))}
        </ul>
        <div className="dialog-actions">
          <button ref={closeRef} type="button" className="button" onClick={onClose}>
            U redu
          </button>
        </div>
      </div>
    </div>
  )
}
