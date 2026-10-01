import { useState } from 'react'
import CodeSnippet from './CodeSnippet.jsx'

export default function PredictOutputGame({ challenge, onSubmit, result, loading }) {
  const [value, setValue] = useState('')
  const payload = challenge.payload ?? {}
  const locked = Boolean(result) || loading

  async function handleSubmit(event) {
    event.preventDefault()
    if (locked) {
      return
    }

    await onSubmit(value)
  }

  const readAs = result?.details?.normalizedAnswer

  return (
    <article className="challenge">
      <CodeSnippet code={payload.codeSnippet ?? ''} />
      <p className="challenge-question">{payload.question}</p>
      <form className="form" onSubmit={handleSubmit}>
        <label>
          Tvoj odgovor
          <textarea
            value={value}
            onChange={(event) => setValue(event.target.value)}
            rows={3}
            required
            disabled={locked}
          />
        </label>
        <button type="submit" disabled={locked}>
          Pošalji odgovor
        </button>
      </form>
      {readAs ? <p className="challenge-meta">Pročitano kao: {readAs}</p> : null}
    </article>
  )
}
