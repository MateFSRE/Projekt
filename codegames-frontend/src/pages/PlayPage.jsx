import { useCallback, useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import api from '../api/client.js'
import { useAuth } from '../auth/AuthContext.jsx'
import AwardPopup from '../components/AwardPopup.jsx'
import { challengeTypeLabel } from '../components/challenge/challengeTypes.js'
import { getGameComponent } from '../games/registry.js'
import { readGameSelection } from '../games/selection.js'

function solutionText(result) {
  const solution = result?.revealedSolution
  if (!solution || typeof solution !== 'object') {
    return ''
  }

  if (typeof solution.correctAnswer === 'string') {
    return solution.correctAnswer
  }

  if (typeof solution.text === 'string') {
    return solution.text
  }

  if (Array.isArray(solution.blanks)) {
    return solution.blanks
      .map((blank) => blank.answer)
      .filter((answer) => typeof answer === 'string' && answer.length > 0)
      .join(', ')
  }

  return ''
}

export default function PlayPage() {
  const { applyProgress } = useAuth()
  const [challenge, setChallenge] = useState(null)
  const [result, setResult] = useState(null)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')
  const [doneMessage, setDoneMessage] = useState('')
  const [awards, setAwards] = useState([])

  const loadNext = useCallback(async () => {
    setLoading(true)
    setError('')
    setResult(null)
    setAwards([])
    setDoneMessage('')
    setChallenge(null)

    const selected = readGameSelection()
    if (selected.length === 0) {
      setDoneMessage('Odaberi barem jednu vrstu igre.')
      setLoading(false)
      return
    }

    try {
      const response = await api.get('/challenges/next', { params: { types: selected.join(',') } })
      setChallenge(response.data)
    } catch (loadError) {
      if (loadError.status === 404) {
        setDoneMessage(loadError.message)
      } else {
        setError(loadError.message)
      }
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    loadNext()
  }, [loadNext])

  async function onSubmit(answer) {
    if (!challenge || result || submitting) {
      return
    }

    setError('')
    setSubmitting(true)

    try {
      const response = await api.post(`/challenges/${challenge.id}/answer`, { answer })
      setResult(response.data)
      setAwards(Array.isArray(response.data.newAwards) ? response.data.newAwards : [])
      applyProgress({ xp: response.data.newXp, level: response.data.newLevel })
    } catch (submitError) {
      setError(submitError.message)
    } finally {
      setSubmitting(false)
    }
  }

  const Game = challenge ? getGameComponent(challenge.type) : null
  const revealed = solutionText(result)

  return (
    <section className="page">
      <h1>{challenge ? challenge.title : 'Igra'}</h1>

      {loading ? <p role="status">Učitavanje...</p> : null}
      {error ? <p className="error" role="alert">{error}</p> : null}

      {!loading && doneMessage ? (
        <div className="feedback">
          <p className="empty">{doneMessage}</p>
          <Link className="button" to="/games">
            Natrag na igre
          </Link>
        </div>
      ) : null}

      {!loading && error && !challenge ? (
        <button type="button" onClick={loadNext}>
          Pokušaj ponovno
        </button>
      ) : null}

      {challenge && !Game ? (
        <p className="error">Ova vrsta zadatka još nema prikaz.</p>
      ) : null}

      {challenge && Game ? (
        <>
          <p className="challenge-meta">
            {challenge.language} · {challengeTypeLabel(challenge.type)} · težina {challenge.difficulty} · {challenge.points} bodova
          </p>
          <Game
            key={challenge.id}
            challenge={challenge}
            onSubmit={onSubmit}
            result={result}
            loading={submitting}
          />
        </>
      ) : null}

      {awards.length > 0 ? <AwardPopup awards={awards} onClose={() => setAwards([])} /> : null}

      {result ? (
        <div className={result.isCorrect ? 'feedback feedback-ok' : 'feedback feedback-bad'} role="status">
          <p>{result.isCorrect ? 'Točno.' : 'Nije točno.'}</p>
          {result.levelUp ? <p className="level-up" role="status">Level up!</p> : null}
          {revealed && !revealed.includes('\n') ? <p>Točan odgovor: {revealed}</p> : null}
          {revealed && revealed.includes('\n') ? (
            <>
              <p>Točan odgovor:</p>
              <pre className="solution-code">{revealed}</pre>
            </>
          ) : null}
          <p>{result.explanation}</p>
          <p>Osvojeno bodova: {result.pointsAwarded}</p>
          <p>Ukupno: {result.newXp} XP · razina {result.newLevel}</p>
          <button type="button" className="button" onClick={loadNext} disabled={loading}>
            Sljedeći zadatak
          </button>
        </div>
      ) : null}
    </section>
  )
}
