import { useState } from 'react'

const keywords = {
  Python: new Set(['False', 'None', 'True', 'and', 'elif', 'else', 'for', 'if', 'in', 'not', 'or', 'print', 'while']),
  JavaScript: new Set(['const', 'else', 'false', 'for', 'if', 'let', 'null', 'of', 'return', 'true', 'var', 'while']),
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

function findCell(cells, stepIndex, name) {
  return (cells ?? []).find((cell) => cell.stepIndex === stepIndex && cell.name === name)
}

export default function TraceExecutionGame({ challenge, onSubmit, result, loading }) {
  const payload = challenge.payload ?? {}
  const locked = Boolean(result) || loading
  const variables = payload.variables ?? []
  const steps = payload.steps ?? []
  const askAt = new Set(payload.askAt ?? [])
  const codeLines = String(payload.code ?? '').replaceAll('\r\n', '\n').split('\n')
  const [current, setCurrent] = useState(0)
  const [values, setValues] = useState({})
  const step = steps[current]
  const asking = askAt.has(current)
  const lineNumber = step?.line

  function setValue(name, value) {
    setValues((previous) => ({
      ...previous,
      [current]: { ...previous[current], [name]: value },
    }))
  }

  async function handleSubmit(event) {
    event.preventDefault()
    if (locked) {
      return
    }

    const answer = {}
    for (const index of payload.askAt ?? []) {
      answer[index] = {}
      for (const name of variables) {
        answer[index][name] = values[index]?.[name] ?? ''
      }
    }

    await onSubmit(answer)
  }

  return (
    <article className="trace-game">
      <p className="challenge-question">Prati izvršavanje i upiši vrijednosti na traženim koracima.</p>
      <div className="trace-layout">
        <div className="trace-code-block" aria-label="Kod">
          {codeLines.map((text, index) => (
            <div
              key={index}
              className={index + 1 === lineNumber ? 'trace-line trace-line-current' : 'trace-line'}
            >
              <span className="trace-line-no">{index + 1}</span>
              <code className="trace-line-text">{highlight(text, payload.language)}</code>
            </div>
          ))}
        </div>
        <div className="trace-panel">
          <p className="trace-status">
            Korak {current + 1} od {steps.length}
            {lineNumber ? ` · linija ${lineNumber}` : ''}
            {asking ? ' · upiši vrijednosti' : ''}
          </p>
          <table className="trace-table">
            <thead>
              <tr>
                <th>Varijabla</th>
                <th>Vrijednost</th>
              </tr>
            </thead>
            <tbody>
              {variables.map((name) => {
                const mark = findCell(result?.details?.cells, current, name)
                const solution = findCell(result?.revealedSolution?.cells, current, name)
                const className = [
                  'trace-input',
                  mark?.correct === true ? 'trace-cell-ok' : '',
                  mark?.correct === false ? 'trace-cell-bad' : '',
                ].filter(Boolean).join(' ')

                return (
                  <tr key={name}>
                    <th scope="row">{name}</th>
                    <td>
                      {asking ? (
                        <input
                          className={className}
                          value={values[current]?.[name] ?? ''}
                          disabled={locked}
                          aria-label={`${name} na koraku ${current + 1}`}
                          onChange={(event) => setValue(name, event.target.value)}
                        />
                      ) : (
                        <span className="trace-quiet">{values[current]?.[name] || '—'}</span>
                      )}
                      {mark?.correct === false ? <span className="trace-fix">{solution?.value}</span> : null}
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
          {!asking ? <p className="trace-quiet">U ovom koraku nema unosa. Prijeđi dalje.</p> : null}
          <div className="trace-nav">
            <button type="button" className="grow" onClick={() => setCurrent((index) => index - 1)} disabled={current === 0}>
              Natrag
            </button>
            <button
              type="button"
              className="grow"
              onClick={() => setCurrent((index) => index + 1)}
              disabled={current >= steps.length - 1}
            >
              Sljedeći korak
            </button>
          </div>
          <ol className="trace-history">
            {steps.slice(0, current + 1).map((item, index) => (
              <li key={index}>
                <button
                  type="button"
                  className={index === current ? 'trace-history-current grow' : 'grow'}
                  onClick={() => setCurrent(index)}
                >
                  Korak {index + 1}, linija {item.line}{askAt.has(index) ? ' · unos' : ''}
                </button>
              </li>
            ))}
          </ol>
        </div>
      </div>
      <form className="form" onSubmit={handleSubmit}>
        <button type="submit" disabled={locked}>
          Pošalji odgovor
        </button>
      </form>
    </article>
  )
}
