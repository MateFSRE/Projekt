import { useRef, useState } from 'react'

function partsOf(template) {
  const parts = []
  const pattern = /\{\{([A-Za-z0-9]+)\}\}/g
  let last = 0
  let match = pattern.exec(template)

  while (match) {
    if (match.index > last) {
      parts.push({ type: 'text', text: template.slice(last, match.index) })
    }

    parts.push({ type: 'blank', id: match[1] })
    last = match.index + match[0].length
    match = pattern.exec(template)
  }

  if (last < template.length) {
    parts.push({ type: 'text', text: template.slice(last) })
  }

  return parts
}

export default function FillBlanksGame({ challenge, onSubmit, result, loading }) {
  const payload = challenge.payload ?? {}
  const locked = Boolean(result) || loading
  const bankRef = useRef([...(payload.wordBank ?? [])])
  const filledRef = useRef({})
  const [bank, setBank] = useState(bankRef.current)
  const [filled, setFilled] = useState({})
  const [activeId, setActiveId] = useState(null)
  const [ghost, setGhost] = useState(null)
  const parts = partsOf(String(payload.template ?? ''))
  const marks = {}
  const solutions = {}

  for (const blank of result?.details?.blanks ?? []) {
    marks[blank.id] = blank.correct
  }

  for (const blank of result?.revealedSolution?.blanks ?? []) {
    solutions[blank.id] = blank.answer
  }

  function place(blankId, word, bankIndex) {
    if (locked || filledRef.current[blankId] || bankRef.current[bankIndex] !== word) {
      return
    }

    const nextBank = bankRef.current.slice()
    nextBank.splice(bankIndex, 1)
    bankRef.current = nextBank
    filledRef.current = { ...filledRef.current, [blankId]: word }
    setBank(nextBank)
    setFilled(filledRef.current)
    setActiveId(null)
  }

  function placeInTarget(word, bankIndex) {
    const target = activeId && !filledRef.current[activeId]
      ? activeId
      : parts.find((part) => part.type === 'blank' && !filledRef.current[part.id])?.id

    if (!target) {
      return
    }

    place(target, word, bankIndex)
  }

  function clearBlank(blankId) {
    const word = filledRef.current[blankId]
    if (!word || locked) {
      return null
    }

    const nextFilled = { ...filledRef.current }
    delete nextFilled[blankId]
    filledRef.current = nextFilled
    bankRef.current = [...bankRef.current, word]
    setFilled(nextFilled)
    setBank(bankRef.current)
    return word
  }

  function onFilledPointerDown(event, blankId) {
    if (locked || event.button !== 0 || !filledRef.current[blankId]) {
      return
    }

    event.preventDefault()
    const word = filledRef.current[blankId]
    const startX = event.clientX
    const startY = event.clientY
    let moved = false

    function move(moveEvent) {
      if (Math.hypot(moveEvent.clientX - startX, moveEvent.clientY - startY) > 8) {
        moved = true
      }

      setGhost({ word, x: moveEvent.clientX, y: moveEvent.clientY })
    }

    function up(upEvent) {
      window.removeEventListener('pointermove', move)
      window.removeEventListener('pointerup', up)
      setGhost(null)

      if (!moved) {
        clearBlank(blankId)
        return
      }

      const target = document.elementFromPoint(upEvent.clientX, upEvent.clientY)
      const otherId = target?.closest('[data-blank-id]')?.dataset.blankId
      if (otherId === blankId || (otherId && filledRef.current[otherId])) {
        return
      }

      if (!otherId) {
        clearBlank(blankId)
        return
      }

      const removed = clearBlank(blankId)
      if (!removed) {
        return
      }

      const nextBank = bankRef.current.slice()
      const bankIndex = nextBank.lastIndexOf(removed)
      if (bankIndex >= 0) {
        nextBank.splice(bankIndex, 1)
        bankRef.current = nextBank
        setBank(nextBank)
      }

      filledRef.current = { ...filledRef.current, [otherId]: removed }
      setFilled(filledRef.current)
      setActiveId(null)
    }

    window.addEventListener('pointermove', move)
    window.addEventListener('pointerup', up)
  }

  function onWordPointerDown(event, word, bankIndex) {
    if (locked || event.button !== 0) {
      return
    }

    event.preventDefault()
    const startX = event.clientX
    const startY = event.clientY
    let moved = false

    function move(moveEvent) {
      if (Math.hypot(moveEvent.clientX - startX, moveEvent.clientY - startY) > 8) {
        moved = true
      }

      setGhost({ word, x: moveEvent.clientX, y: moveEvent.clientY })
    }

    function up(upEvent) {
      window.removeEventListener('pointermove', move)
      window.removeEventListener('pointerup', up)
      setGhost(null)

      if (moved) {
        const target = document.elementFromPoint(upEvent.clientX, upEvent.clientY)
        const blank = target?.closest('[data-blank-id]')
        if (blank) {
          place(blank.dataset.blankId, word, bankIndex)
        }
      } else {
        placeInTarget(word, bankIndex)
      }
    }

    window.addEventListener('pointermove', move)
    window.addEventListener('pointerup', up)
  }

  async function handleSubmit(event) {
    event.preventDefault()
    if (locked) {
      return
    }

    await onSubmit(filledRef.current)
  }

  return (
    <article className="fill-game">
      <p className="challenge-question">Dopuni rupe.</p>
      <p className="swipe-hint">Klik na riječ stavlja je u rupu. Možeš je i povući. Ako si je stavio zabunom, klikni rupu ili povuci riječ natrag u banku.</p>
      <pre className="fill-code">
        {parts.map((part, index) => {
          if (part.type === 'text') {
            return <span key={`t-${index}`} className="fill-text">{part.text}</span>
          }

          const word = filled[part.id]
          const correct = result ? marks[part.id] === true : null
          const className = [
            'blank-slot',
            'nudge',
            word ? 'blank-filled' : '',
            activeId === part.id && !result ? 'blank-active' : '',
            correct === true ? 'blank-ok' : '',
            correct === false ? 'blank-bad' : '',
          ].filter(Boolean).join(' ')

          return (
            <span key={part.id} className="blank-wrap">
              <button
                type="button"
                className={className}
                data-blank-id={part.id}
                onPointerDown={(event) => onFilledPointerDown(event, part.id)}
                onClick={() => {
                  if (locked || word) {
                    return
                  }

                  setActiveId(part.id)
                }}
              >
                {word || '___'}
              </button>
              {correct === false ? <span className="blank-fix">{solutions[part.id]}</span> : null}
            </span>
          )
        })}
      </pre>
      <div className="word-bank">
        {bank.map((word, index) => (
          <button
            key={`${word}-${index}`}
            type="button"
            className="word-chip nudge"
            disabled={locked}
            onPointerDown={(event) => onWordPointerDown(event, word, index)}
          >
            {word}
          </button>
        ))}
      </div>
      <form className="form" onSubmit={handleSubmit}>
        <button type="submit" disabled={locked}>
          Pošalji odgovor
        </button>
      </form>
      {ghost ? (
        <span className="word-ghost" style={{ left: ghost.x, top: ghost.y }}>
          {ghost.word}
        </span>
      ) : null}
    </article>
  )
}
