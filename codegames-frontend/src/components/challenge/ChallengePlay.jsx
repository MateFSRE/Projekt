import { useState } from 'react'
import { Link } from 'react-router-dom'
import { challengeTypeLabel, usesOptions } from './challengeTypes.js'

// Zajednički prikaz zadatka za /demo i /games/play.
export default function ChallengePlay({
  challenge,
  onSubmit,
  showRegisterPrompt = false,
  emptyMessage = 'Zadatak još nije učitan.',
}) {
  const [optionId, setOptionId] = useState('')
  const [answer, setAnswer] = useState('')
  const [result, setResult] = useState(null)
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  if (!challenge) {
    return <p>{emptyMessage}</p>
  }

  const choice = usesOptions(challenge.type)

  async function handleSubmit(event) {
    event.preventDefault()
    setError('')
    setSubmitting(true)

    try {
      const payload = choice
        ? { optionId: Number(optionId) }
        : { answer }
      const nextResult = await onSubmit(payload)
      setResult(nextResult)
    } catch (submitError) {
      setError(submitError.message)
    } finally {
      setSubmitting(false)
    }
  }

  function tryAgain() {
    setResult(null)
    setError('')
    setOptionId('')
    setAnswer('')
  }

  return (
    <article className="challenge">
      <p className="challenge-meta">
        {challenge.language} · {challengeTypeLabel(challenge.type)} · težina {challenge.difficulty}
      </p>
      <h1>{challenge.title}</h1>
      <pre className="challenge-code"><code>{challenge.codeSnippet}</code></pre>
      <p className="challenge-question">{challenge.question}</p>

      {result ? (
        <div className={result.isCorrect ? 'feedback feedback-ok' : 'feedback feedback-bad'}>
          <p>{result.isCorrect ? 'Točno.' : 'Nije točno.'}</p>
          <p>Točan odgovor: {result.correctAnswer}</p>
          <p>{result.explanation}</p>
          {showRegisterPrompt ? (
            <Link className="button" to="/register">
              Registriraj se da sačuvaš bodove i uđeš na rang listu
            </Link>
          ) : null}
          <button type="button" className="button-secondary" onClick={tryAgain}>
            Pokušaj ponovno
          </button>
        </div>
      ) : (
        <form className="form" onSubmit={handleSubmit}>
          {choice ? (
            <fieldset className="options">
              <legend>Odgovor</legend>
              {challenge.options.map((option) => (
                <label key={option.id} className="option">
                  <input
                    type="radio"
                    name="option"
                    value={option.id}
                    checked={optionId === String(option.id)}
                    onChange={(event) => setOptionId(event.target.value)}
                    required
                  />
                  <span>{option.text}</span>
                </label>
              ))}
            </fieldset>
          ) : (
            <label>
              Tvoj odgovor
              <input
                value={answer}
                onChange={(event) => setAnswer(event.target.value)}
                required
              />
            </label>
          )}
          {error ? <p className="error">{error}</p> : null}
          <button type="submit" disabled={submitting}>
            Pošalji odgovor
          </button>
        </form>
      )}
    </article>
  )
}
