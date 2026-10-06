import { useEffect, useState } from 'react'
import {
  ArrowDownUp,
  Bot,
  Bug,
  Check,
  Footprints,
  ListChecks,
  Terminal,
  TextCursorInput,
  Wrench,
} from 'lucide-react'
import api from '../api/client.js'
import { useAuth } from '../auth/AuthContext.jsx'
import { challengeTypeLabel } from '../components/challenge/challengeTypes.js'
import { getGameComponent } from '../games/registry.js'

const icons = {
  PredictOutput: Terminal,
  MultipleChoice: ListChecks,
  FixBug: Wrench,
  SwipeBug: Bug,
  FillBlanks: TextCursorInput,
  ReorderCode: ArrowDownUp,
  TraceExecution: Footprints,
  RobotMaze: Bot,
}

function formatCountdown(total) {
  const safe = Math.max(0, total)
  const hours = String(Math.floor(safe / 3600)).padStart(2, '0')
  const minutes = String(Math.floor((safe % 3600) / 60)).padStart(2, '0')
  const seconds = String(safe % 60).padStart(2, '0')
  return `${hours}:${minutes}:${seconds}`
}

export default function DailyPage() {
  const { applyProgress } = useAuth()
  const [board, setBoard] = useState(null)
  const [seconds, setSeconds] = useState(0)
  const [activeId, setActiveId] = useState(null)
  const [result, setResult] = useState(null)
  const [loading, setLoading] = useState(true)
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  async function load() {
    setLoading(true)
    setError('')
    try {
      const response = await api.get('/daily')
      setBoard(response.data)
      setSeconds(response.data.secondsUntilUtcMidnight ?? 0)
    } catch (loadError) {
      setError(loadError.message)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [])

  useEffect(() => {
    const timer = setInterval(() => {
      setSeconds((value) => (value > 0 ? value - 1 : 0))
    }, 1000)
    return () => clearInterval(timer)
  }, [])

  function openChallenge(slot) {
    setActiveId(slot.challenge.id)
    setResult(null)
    setError('')
  }

  async function onSubmit(answer) {
    if (!activeId || submitting) {
      return
    }

    setSubmitting(true)
    setError('')
    try {
      const response = await api.post(`/daily/${activeId}/answer`, { answer })
      setResult(response.data)
      applyProgress({ xp: response.data.newXp, level: response.data.newLevel })
      setBoard((current) => {
        if (!current) {
          return current
        }

        return {
          ...current,
          completedCount: response.data.completedCount,
          challenger: {
            ...current.challenger,
            tier: response.data.challengerTier,
            totalCompleted: response.data.totalDailyCompleted,
          },
          challenges: current.challenges.map((slot) => (
            slot.challenge.id === activeId
              ? { ...slot, completed: response.data.dailyCompleted || slot.completed }
              : slot
          )),
        }
      })
    } catch (submitError) {
      setError(submitError.message)
    } finally {
      setSubmitting(false)
    }
  }

  const slots = board?.challenges ?? []
  const active = slots.find((slot) => slot.challenge.id === activeId) ?? null
  const Game = active ? getGameComponent(active.challenge.type) : null
  const done = board ? board.completedCount >= slots.length && slots.length > 0 : false
  const tierLabel = result?.challengerTierUp
    ? roman(result.challengerTier)
    : ''

  return (
    <section className="page">
      <h1>Dnevni izazovi</h1>
      <p className="daily-countdown" role="status">Novi izazovi za {formatCountdown(seconds)}</p>

      {loading ? <p role="status">Učitavanje...</p> : null}
      {error ? <p className="error" role="alert">{error}</p> : null}
      {!loading && error && !board ? (
        <button type="button" onClick={load}>Pokušaj ponovno</button>
      ) : null}

      {board ? (
        <p className="daily-progress">{board.completedCount}/{slots.length} završeno</p>
      ) : null}
      {done ? <p className="feedback feedback-ok">Sva tri dnevna izazova su gotova.</p> : null}

      <div className="daily-grid">
        {slots.map((slot) => {
          const Icon = icons[slot.challenge.type] ?? Terminal
          return (
            <button
              key={slot.challenge.id}
              type="button"
              className={slot.completed ? 'daily-card is-done' : 'daily-card'}
              onClick={() => openChallenge(slot)}
            >
              <Icon size={22} aria-hidden="true" />
              <strong>{slot.challenge.title}</strong>
              <span>{challengeTypeLabel(slot.challenge.type)}</span>
              <span>Težina {slot.challenge.difficulty} · {slot.challenge.points} bodova</span>
              <span className={slot.completed ? 'attempt-ok' : 'trace-quiet'}>
                {slot.completed ? <><Check size={16} aria-hidden="true" /> Završeno</> : 'Čeka'}
              </span>
            </button>
          )
        })}
      </div>

      {active && Game ? (
        <div className="panel">
          <h2>{active.challenge.title}</h2>
          <Game
            key={`${active.challenge.id}-${result ? 'done' : 'open'}`}
            challenge={active.challenge}
            onSubmit={onSubmit}
            result={result}
            loading={submitting}
          />
          {result ? (
            <div className={result.isCorrect ? 'feedback feedback-ok' : 'feedback feedback-bad'} role="status">
              <p>{result.isCorrect ? 'Točno.' : 'Nije točno.'}</p>
              {result.levelUp ? <p className="level-up">Level up!</p> : null}
              {result.challengerTierUp ? (
                <p className="level-up">Challenger {tierLabel}!</p>
              ) : null}
              {result.allClearBonus > 0 ? <p>Bonus za sva tri: +{result.allClearBonus} XP</p> : null}
              <p>{result.explanation}</p>
              <p>Osvojeno bodova: {result.pointsAwarded}</p>
            </div>
          ) : null}
        </div>
      ) : null}
    </section>
  )
}

function roman(tier) {
  if (!tier || tier < 1) {
    return ''
  }

  const parts = [
    [1000, 'M'], [900, 'CM'], [500, 'D'], [400, 'CD'],
    [100, 'C'], [90, 'XC'], [50, 'L'], [40, 'XL'],
    [10, 'X'], [9, 'IX'], [5, 'V'], [4, 'IV'], [1, 'I'],
  ]
  let remaining = tier
  let text = ''
  for (const [value, symbol] of parts) {
    while (remaining >= value) {
      text += symbol
      remaining -= value
    }
  }
  return text
}
