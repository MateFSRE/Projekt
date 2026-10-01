import { useState } from 'react'
import CodeSnippet from './CodeSnippet.jsx'

export default function MultipleChoiceGame({ challenge, onSubmit, result, loading }) {
  const [optionId, setOptionId] = useState('')
  const payload = challenge.payload ?? {}
  const options = payload.options ?? []
  const locked = Boolean(result) || loading
  const correctId = result?.revealedSolution?.optionId

  async function handleSubmit(event) {
    event.preventDefault()
    if (locked) {
      return
    }

    await onSubmit({ optionId: Number(optionId) })
  }

  return (
    <article className="challenge">
      <CodeSnippet code={payload.codeSnippet ?? ''} />
      <p className="challenge-question">{payload.question}</p>
      <form className="form" onSubmit={handleSubmit}>
        <fieldset className="options">
          <legend>Odgovor</legend>
          {options.map((option) => {
            const selected = optionId === String(option.id)
            const revealed = result && option.id === correctId
            const className = [
              'option',
              'grow',
              revealed ? 'option-correct' : '',
              result && selected && !revealed ? 'option-wrong' : '',
            ]
              .filter(Boolean)
              .join(' ')

            return (
              <label key={option.id} className={className}>
                <input
                  type="radio"
                  name="option"
                  value={option.id}
                  checked={selected}
                  onChange={(event) => setOptionId(event.target.value)}
                  required
                  disabled={locked}
                />
                <span>{option.text}</span>
              </label>
            )
          })}
        </fieldset>
        <button type="submit" disabled={locked || optionId === ''}>
          Pošalji odgovor
        </button>
      </form>
    </article>
  )
}
