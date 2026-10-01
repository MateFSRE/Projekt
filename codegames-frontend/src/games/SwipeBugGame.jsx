import { useRef } from 'react'

const swipeThreshold = 48

export default function SwipeBugGame({ challenge, onSubmit, result, loading }) {
  const payload = challenge.payload ?? {}
  const locked = Boolean(result) || loading
  const touchStart = useRef(null)
  const bugLine = result?.revealedSolution?.bugLine
  const hasBug = result?.revealedSolution?.hasBug
  const lines = String(payload.code ?? '')
    .replaceAll('\r\n', '\n')
    .replaceAll('\r', '\n')
    .replace(/\n$/, '')
    .split('\n')

  async function choose(hasBugAnswer) {
    if (locked) {
      return
    }

    await onSubmit({ hasBug: hasBugAnswer })
  }

  function onTouchStart(event) {
    if (locked) {
      return
    }

    const touch = event.changedTouches[0]
    touchStart.current = { x: touch.clientX, y: touch.clientY }
  }

  function onTouchEnd(event) {
    if (!touchStart.current) {
      return
    }

    const touch = event.changedTouches[0]
    const dx = touch.clientX - touchStart.current.x
    const dy = touch.clientY - touchStart.current.y
    touchStart.current = null

    if (Math.abs(dx) < swipeThreshold || Math.abs(dx) < Math.abs(dy)) {
      return
    }

    choose(dx < 0)
  }

  const tone = result ? (result.isCorrect ? 'swipe-correct' : 'swipe-wrong') : ''

  return (
    <article className={`swipe-card ${tone}`.trim()} onTouchStart={onTouchStart} onTouchEnd={onTouchEnd}>
      <p className="challenge-question">Ima li ovaj kod bug?</p>
      <p className="swipe-hint">Povuci lijevo za „Ima bug”, desno za „Ispravno”.</p>
      <div className="challenge-code" aria-label={payload.language}>
        {lines.map((line, index) => {
          const number = index + 1
          const marked = result && bugLine === number
          return (
            <div key={number} className={marked ? 'code-line bug-line' : 'code-line'}>
              <span className="line-number">{number}</span>
              <span>{line}</span>
            </div>
          )
        })}
      </div>
      <div className="swipe-actions">
        <button type="button" className="swipe-button swipe-bug nudge" disabled={locked} onClick={() => choose(true)}>
          Ima bug
        </button>
        <button type="button" className="swipe-button swipe-clean nudge" disabled={locked} onClick={() => choose(false)}>
          Ispravno
        </button>
      </div>
      {result && hasBug === true ? <p className="bug-note">Bug je u liniji {bugLine}.</p> : null}
      {result && hasBug === false ? <p className="bug-note">U kodu nema buga.</p> : null}
    </article>
  )
}
