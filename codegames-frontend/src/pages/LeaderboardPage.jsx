import { useEffect, useState } from 'react'
import api from '../api/client.js'
import { useAuth } from '../auth/AuthContext.jsx'

const periods = [
  { id: 'total', label: 'Ukupno' },
  { id: 'weekly', label: 'Ovaj tjedan' },
]

export default function LeaderboardPage() {
  const { user } = useAuth()
  const [period, setPeriod] = useState('total')
  const [board, setBoard] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let active = true
    setLoading(true)
    setError('')

    api
      .get('/leaderboard', { params: { period } })
      .then((response) => {
        if (active) {
          setBoard(response.data)
        }
      })
      .catch((loadError) => {
        if (active) {
          setBoard(null)
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
  }, [period])

  const entries = board?.entries ?? []
  const outside = board?.currentUser ?? null

  return (
    <section className="page">
      <h1>Rang lista</h1>
      <div className="period-switch" role="tablist" aria-label="Razdoblje">
        {periods.map((item) => (
          <button
            key={item.id}
            type="button"
            role="tab"
            aria-selected={period === item.id}
            className={period === item.id ? 'period-tab period-tab-active' : 'period-tab'}
            onClick={() => setPeriod(item.id)}
          >
            {item.label}
          </button>
        ))}
      </div>
      <p className="trace-quiet">
        {period === 'weekly'
          ? 'Poredak po bodovima osvojenim od ponedjeljka. Dani su po UTC vremenu.'
          : 'Poredak po ukupnim bodovima.'}
      </p>
      {loading ? <p role="status">Učitavanje...</p> : null}
      {error ? <p className="error" role="alert">{error}</p> : null}
      {!loading && !error && entries.length === 0 ? (
        <p className="empty">Još nema bodova u ovom razdoblju.</p>
      ) : null}
      {!loading && entries.length > 0 ? (
        <div className="board" role="table">
          <div className="board-row board-head" role="row">
            <span role="columnheader">Rang</span>
            <span role="columnheader">Igrač</span>
            <span role="columnheader">Razina</span>
            <span role="columnheader">Bodovi</span>
          </div>
          {entries.map((entry) => (
            <BoardRow key={`${entry.rank}-${entry.username}`} entry={entry} username={user?.username} />
          ))}
        </div>
      ) : null}
      {!loading && outside ? (
        <div className="board-outside">
          <p>Nisi u prvih 50. Tvoj rang je {outside.rank}.</p>
          <div className="board" role="table">
            <BoardRow entry={outside} username={user?.username} />
          </div>
        </div>
      ) : null}
      {!user ? <p className="trace-quiet">Prijavi se da vidiš vlastiti rang.</p> : null}
    </section>
  )
}

function BoardRow({ entry, username }) {
  const mine = username && entry.username === username
  return (
    <div className={mine ? 'board-row board-me' : 'board-row'} role="row">
      <span role="cell">{entry.rank}</span>
      <span role="cell">{entry.username}</span>
      <span role="cell">{entry.level}</span>
      <span role="cell">{entry.points}</span>
    </div>
  )
}
