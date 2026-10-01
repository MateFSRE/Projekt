import { useRef, useState } from 'react'

const keywords = {
  Python: new Set(['False', 'None', 'True', 'and', 'def', 'elif', 'else', 'for', 'if', 'in', 'not', 'or', 'print', 'return', 'while']),
  JavaScript: new Set(['const', 'else', 'false', 'for', 'function', 'if', 'let', 'null', 'of', 'return', 'true', 'var', 'while']),
}

function highlight(text, language) {
  const words = keywords[language] ?? new Set()
  const pattern = /("(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|`(?:\\.|[^`\\])*`|\b\d+(?:\.\d+)?\b|\b[A-Za-z_]\w*\b|[^A-Za-z0-9_"'`]+)/g
  const nodes = []
  let match = pattern.exec(text)
  let key = 0

  while (match) {
    const token = match[0]
    let className = ''
    if (token.startsWith('"') || token.startsWith("'") || token.startsWith('`')) {
      className = 'tok-string'
    } else if (/^\d/.test(token)) {
      className = 'tok-number'
    } else if (words.has(token)) {
      className = 'tok-keyword'
    }

    nodes.push(<span key={key} className={className}>{token}</span>)
    key += 1
    match = pattern.exec(text)
  }

  return nodes
}

export default function ReorderCodeGame({ challenge, onSubmit, result, loading }) {
  const payload = challenge.payload ?? {}
  const locked = Boolean(result) || loading
  const [lines, setLines] = useState(() => payload.lines ?? [])
  const linesRef = useRef(lines)
  const [draggingId, setDraggingId] = useState(null)
  const [placed, setPlaced] = useState(null)
  const wrong = new Set(result?.details?.wrongLineIds ?? [])

  function reorder(fromIndex, toIndex) {
    const next = linesRef.current.slice()
    const [item] = next.splice(fromIndex, 1)
    next.splice(toIndex, 0, item)
    linesRef.current = next
    setLines(next)
    setPlaced({ id: item.id, down: fromIndex < toIndex, tick: Date.now() })
  }

  function onPointerDown(event, lineId) {
    if (locked || event.button !== 0) {
      return
    }

    event.preventDefault()
    setDraggingId(lineId)

    function move(moveEvent) {
      const under = document.elementFromPoint(moveEvent.clientX, moveEvent.clientY)
      const row = under?.closest('[data-line-id]')
      if (!row || row.dataset.lineId === lineId) {
        return
      }

      const fromIndex = linesRef.current.findIndex((line) => line.id === lineId)
      const targetIndex = linesRef.current.findIndex((line) => line.id === row.dataset.lineId)
      if (fromIndex < 0 || targetIndex < 0 || fromIndex === targetIndex) {
        return
      }

      const midpoint = row.getBoundingClientRect().top + row.getBoundingClientRect().height / 2
      if (fromIndex < targetIndex && moveEvent.clientY < midpoint) {
        return
      }

      if (fromIndex > targetIndex && moveEvent.clientY > midpoint) {
        return
      }

      reorder(fromIndex, targetIndex)
    }

    function up() {
      window.removeEventListener('pointermove', move)
      window.removeEventListener('pointerup', up)
      setDraggingId(null)
    }

    window.addEventListener('pointermove', move)
    window.addEventListener('pointerup', up)
  }

  async function handleSubmit(event) {
    event.preventDefault()
    if (locked) {
      return
    }

    await onSubmit(linesRef.current.map((line) => line.id))
  }

  return (
    <article className="reorder-game">
      <p className="challenge-question">{payload.goal}</p>
      <p className="swipe-hint">Povuci linije u redoslijed koji radi to što je opisano. Uvlaka ide zajedno s linijom.</p>
      <ol className="reorder-list">
        {lines.map((line) => {
          const justPlaced = placed?.id === line.id
          const className = [
            'reorder-line',
            draggingId === line.id ? 'reorder-dragging' : '',
            justPlaced ? (placed.down ? 'reorder-slotted-down' : 'reorder-slotted-up') : '',
            result && wrong.has(line.id) ? 'reorder-bad' : '',
            result && !wrong.has(line.id) ? 'reorder-ok' : '',
          ].filter(Boolean).join(' ')

          return (
            <li
              key={justPlaced ? `${line.id}-${placed.tick}` : line.id}
              className={className}
              data-line-id={line.id}
              onPointerDown={(event) => onPointerDown(event, line.id)}
            >
              <span className="reorder-handle" aria-hidden="true">::</span>
              <code className="reorder-text">
                {' '.repeat(Number(line.indent) || 0)}
                {highlight(String(line.text ?? ''), payload.language)}
              </code>
            </li>
          )
        })}
      </ol>
      <form className="form" onSubmit={handleSubmit}>
        <button type="submit" disabled={locked}>
          Provjeri
        </button>
      </form>
    </article>
  )
}
