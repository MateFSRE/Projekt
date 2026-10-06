import { useEffect, useState } from 'react'
import {
  ArrowDownUp,
  Bot,
  Bug,
  Footprints,
  ListChecks,
  Terminal,
  TextCursorInput,
  Wrench,
} from 'lucide-react'
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import api from '../api/client.js'
import { useAuth } from '../auth/AuthContext.jsx'
import AwardMark from '../components/AwardMark.jsx'
import { challengeTypeLabel } from '../components/challenge/challengeTypes.js'

const gameIcons = {
  PredictOutput: Terminal,
  MultipleChoice: ListChecks,
  FixBug: Wrench,
  SwipeBug: Bug,
  FillBlanks: TextCursorInput,
  ReorderCode: ArrowDownUp,
  TraceExecution: Footprints,
  RobotMaze: Bot,
}

const tabs = [
  { id: 'overview', label: 'Pregled' },
  { id: 'stats', label: 'Statistika' },
  { id: 'awards', label: 'Nagrade' },
]

function dayLabel(value) {
  const parts = String(value).split('-')
  if (parts.length !== 3) {
    return value
  }

  return `${Number(parts[2])}.${Number(parts[1])}.`
}

function when(value) {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return value
  }

  return date.toLocaleString('hr-HR', { dateStyle: 'short', timeStyle: 'short' })
}

function joinedLabel(value) {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return ''
  }

  return date.toLocaleDateString('hr-HR', { day: 'numeric', month: 'long', year: 'numeric' })
}

function playsLabel(count) {
  const mod10 = count % 10
  const mod100 = count % 100
  if (mod10 === 1 && mod100 !== 11) {
    return `${count} put`
  }

  return `${count} puta`
}

function avatarColor(name) {
  const palette = ['#2f6fed', '#1f7a3a', '#9b2c2c', '#6b4c9a', '#0f6e6e', '#c47d00']
  const code = name.codePointAt(0) ?? 0
  return palette[code % palette.length]
}

export default function ProfilePage() {
  const { user } = useAuth()
  const [stats, setStats] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [tab, setTab] = useState('overview')

  useEffect(() => {
    let active = true

    api
      .get('/profile/stats')
      .then((response) => {
        if (active) {
          setStats(response.data)
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

  const username = stats?.username || user?.username || 'Profil'
  const initial = username.trim().charAt(0).toUpperCase() || '?'
  const chart = (stats?.dailyPoints ?? []).map((item) => ({
    label: dayLabel(item.date),
    points: item.points,
  }))
  const levelAwards = (stats?.awards ?? []).filter((award) => award.kind === 'Level')
  const streakAwards = (stats?.awards ?? []).filter((award) => award.kind === 'Streak')
  const FavoriteIcon = stats?.favoriteType ? gameIcons[stats.favoriteType] : null

  function onTabKey(event) {
    const index = tabs.findIndex((item) => item.id === tab)
    if (event.key !== 'ArrowRight' && event.key !== 'ArrowLeft') {
      return
    }

    event.preventDefault()
    const step = event.key === 'ArrowRight' ? 1 : -1
    const next = tabs[(index + step + tabs.length) % tabs.length]
    setTab(next.id)
    document.getElementById(`profile-tab-${next.id}`)?.focus()
  }

  return (
    <section className="page page-wide">
      <article className="profile-card">
        <div className="profile-hero">
          <div
            className="profile-avatar"
            style={{ background: avatarColor(username) }}
            aria-hidden="true"
          >
            {initial}
          </div>
          <div>
            <h1 className="profile-name">{username}</h1>
            {stats ? (
              <p className="profile-meta">
                Član od {joinedLabel(stats.createdAt)}
                {' · '}
                razina {stats.level}
                {' · '}
                {stats.totalPoints} XP
              </p>
            ) : null}
          </div>
        </div>
        <div className="profile-tabs" role="tablist" aria-label="Prikaz profila" onKeyDown={onTabKey}>
          {tabs.map((item) => (
            <button
              key={item.id}
              id={`profile-tab-${item.id}`}
              type="button"
              className="profile-tab"
              role="tab"
              aria-selected={tab === item.id}
              aria-controls={`profile-panel-${item.id}`}
              tabIndex={tab === item.id ? 0 : -1}
              onClick={() => setTab(item.id)}
            >
              {item.label}
            </button>
          ))}
        </div>
      </article>

      {loading ? <p role="status">Učitavanje...</p> : null}
      {error ? <p className="error" role="alert">{error}</p> : null}

      {stats && tab === 'overview' ? (
        <div id="profile-panel-overview" role="tabpanel" aria-labelledby="profile-tab-overview" className="stats-grid overview-grid">
          <article className="stat-card">
            <p className="stat-label">Bodovi</p>
            <p className="stat-value">{stats.totalPoints}</p>
          </article>
          <article className="stat-card favorite-card">
            <p className="stat-label">Najdraža igra</p>
            {stats.favoriteType && FavoriteIcon ? (
              <div className="favorite-row">
                <span className="favorite-icon" aria-hidden="true">
                  <FavoriteIcon size={28} />
                </span>
                <div>
                  <p className="stat-value">{challengeTypeLabel(stats.favoriteType)}</p>
                  <p className="trace-quiet">Odigrano {playsLabel(stats.favoritePlays)}</p>
                </div>
              </div>
            ) : (
              <p className="empty">Još nema odigrane igre.</p>
            )}
          </article>
        </div>
      ) : null}

      {stats && tab === 'stats' ? (
        <div id="profile-panel-stats" role="tabpanel" aria-labelledby="profile-tab-stats">
          <div className="stats-grid">
            <article className="stat-card">
              <p className="stat-label">Razina</p>
              <p className="stat-value">{stats.level}</p>
              <div
                className="progress-track"
                role="progressbar"
                aria-valuemin={0}
                aria-valuemax={100}
                aria-valuenow={stats.progressPercent}
                aria-label="Napredak do sljedeće razine"
              >
                <div className="progress-fill" style={{ width: `${stats.progressPercent}%` }} />
              </div>
              <p className="trace-quiet">{stats.progressPercent}% do sljedeće razine</p>
            </article>
            <article className="stat-card">
              <p className="stat-label">Ukupni bodovi</p>
              <p className="stat-value">{stats.totalPoints}</p>
            </article>
            <article className="stat-card">
              <p className="stat-label">Točni odgovori</p>
              <p className="stat-value">{stats.correctPercent}%</p>
            </article>
            <article className="stat-card">
              <p className="stat-label">Streak</p>
              <p className="stat-value">{stats.currentStreak}</p>
              <p className="trace-quiet">Najbolji: {stats.bestStreak}</p>
            </article>
            <article className="stat-card">
              <p className="stat-label">Riješeni zadaci</p>
              <p className="stat-value">{stats.solvedCount}</p>
            </article>
          </div>

          <section className="stat-block">
            <h2>Po vrsti igre</h2>
            {stats.solvedByType.length === 0 ? <p className="empty">Još nema točno riješenih zadataka.</p> : null}
            {stats.solvedByType.length > 0 ? (
              <ul className="type-stats">
                {stats.solvedByType.map((item) => (
                  <li key={item.type}>
                    <span>{challengeTypeLabel(item.type)}</span>
                    <strong>{item.solved}</strong>
                  </li>
                ))}
              </ul>
            ) : null}
          </section>

          <section className="stat-block">
            <h2>Bodovi po danu</h2>
            <p className="trace-quiet">Zadnjih 30 dana, po UTC datumu.</p>
            <div className="chart-box" role="img" aria-label="Graf bodova po danu, zadnjih 30 dana">
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={chart}>
                  <CartesianGrid stroke="#e6edf5" vertical={false} />
                  <XAxis dataKey="label" tick={{ fontSize: 12 }} minTickGap={24} />
                  <YAxis allowDecimals={false} width={32} tick={{ fontSize: 12 }} />
                  <Tooltip />
                  <Bar dataKey="points" name="Bodovi" fill="#2f6fed" radius={[3, 3, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            </div>
          </section>

          <section className="stat-block">
            <h2>Zadnji pokušaji</h2>
            {stats.recentAttempts.length === 0 ? <p className="empty">Još nema pokušaja.</p> : null}
            {stats.recentAttempts.length > 0 ? (
              <ul className="attempt-list">
                {stats.recentAttempts.map((attempt) => (
                  <li key={attempt.id}>
                    <span className={attempt.isCorrect ? 'attempt-ok' : 'attempt-bad'}>
                      {attempt.isCorrect ? 'Točno' : 'Netočno'}
                    </span>
                    <span>
                      {attempt.challengeTitle}
                      <small>{challengeTypeLabel(attempt.type)} · {when(attempt.createdAt)}</small>
                    </span>
                    <strong>{attempt.pointsAwarded}</strong>
                  </li>
                ))}
              </ul>
            ) : null}
          </section>
        </div>
      ) : null}

      {stats && tab === 'awards' ? (
        <div id="profile-panel-awards" role="tabpanel" aria-labelledby="profile-tab-awards">
          <AwardGroup title="Razine" awards={levelAwards} />
          <AwardGroup title="Niz dana" awards={streakAwards} />
          <ChallengerCard challenger={stats.challenger} />
        </div>
      ) : null}
    </section>
  )
}

function ChallengerCard({ challenger }) {
  if (!challenger) {
    return null
  }

  const step = challenger.step || 10
  const progress = challenger.progress ?? 0
  const width = Math.min(100, Math.round((progress / step) * 100))
  const earned = (challenger.tier ?? 0) > 0

  return (
    <section className="stat-block">
      <h2>Challenger</h2>
      <article className={earned ? 'award-card is-earned' : 'award-card is-locked'}>
        <div>
          <p className="award-title">
            {earned ? `Challenger ${challenger.tierLabel}` : 'Challenger'}
          </p>
          <p className="trace-quiet">
            Riješeno dnevnih zadataka: {challenger.totalCompleted ?? 0}
          </p>
          <div
            className="challenger-track"
            role="progressbar"
            aria-valuemin={0}
            aria-valuemax={step}
            aria-valuenow={progress}
            aria-label="Progres do sljedećeg Challenger tiera"
          >
            <span style={{ width: `${width}%` }} />
          </div>
          <p className="trace-quiet">{progress}/{step} do sljedećih 10</p>
        </div>
      </article>
    </section>
  )
}

function AwardGroup({ title, awards }) {
  return (
    <section className="stat-block">
      <h2>{title}</h2>
      <div className="award-grid">
        {awards.map((award) => (
          <article
            key={`${award.kind}-${award.threshold}`}
            className={award.earned ? 'award-card is-earned' : 'award-card is-locked'}
          >
            <span className="award-icon">
              <AwardMark kind={award.kind} threshold={award.threshold} />
            </span>
            <div>
              <p className="award-title">{award.title}</p>
              <p className="trace-quiet">{award.earned ? award.description : 'Još nije osvojeno'}</p>
            </div>
          </article>
        ))}
      </div>
    </section>
  )
}
