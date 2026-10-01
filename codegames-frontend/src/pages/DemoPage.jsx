import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/client.js'
import { challengeTypeLabel } from '../components/challenge/challengeTypes.js'
import { getGameComponent } from '../games/registry.js'

export default function DemoPage() {
  const [challenge, setChallenge] = useState(null)
  const [result, setResult] = useState(null)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [round, setRound] = useState(0)

  useEffect(() => {
    let active = true

    api
      .get('/demo/challenge')
      .then((response) => {
        if (active) {
          setChallenge(response.data)
        }
      })
      .catch((loadError) => {
        if (active) {
          setError(loadError.message)
        }
      })
      .finally(() => {
        if (active) {
          setLoading(false)
        }
      })

    return () => {
      active = false
    }
  }, [])

  async function onSubmit(answer) {
    if (!challenge || result || submitting) {
      return
    }

    setError('')
    setSubmitting(true)

    try {
      const response = await api.post('/demo/answer', { answer })
      setResult(response.data)
    } catch (submitError) {
      setError(submitError.message)
    } finally {
      setSubmitting(false)
    }
  }

  function tryAgain() {
    setResult(null)
    setError('')
    setRound((value) => value + 1)
  }

  const Game = challenge ? getGameComponent(challenge.type) : null

  return (
    <section className="page">
      <h1>{challenge ? challenge.title : 'Demo'}</h1>
      <p className="challenge-note">Demo se ne sprema i ne donosi bodove.</p>
      {loading ? <p role="status">Učitavanje...</p> : null}
      {error ? <p className="error" role="alert">{error}</p> : null}

      {challenge && !Game ? (
        <p className="error">Ova vrsta zadatka još nema prikaz.</p>
      ) : null}

      {challenge && Game ? (
        <>
          <p className="challenge-meta">
            {challenge.language} · {challengeTypeLabel(challenge.type)} · težina {challenge.difficulty}
          </p>
          <Game
            key={`${challenge.id}-${round}`}
            challenge={challenge}
            onSubmit={onSubmit}
            result={result}
            loading={submitting}
          />
        </>
      ) : null}

      {result ? (
        <div className={result.isCorrect ? 'feedback feedback-ok' : 'feedback feedback-bad'} role="status">
          <p>{result.isCorrect ? 'Točno.' : 'Nije točno.'}</p>
          <p>{result.explanation}</p>
          <Link className="button" to="/register">
            Registriraj se da sačuvaš bodove i uđeš na rang listu
          </Link>
          <button type="button" className="button button-secondary" onClick={tryAgain}>
            Pokušaj ponovno
          </button>
        </div>
      ) : null}
    </section>
  )
}
