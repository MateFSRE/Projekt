import { useEffect, useState } from 'react'
import api from '../../api/client.js'
import { useAuth } from '../../auth/AuthContext.jsx'
import { useAdminUi } from './AdminUi.jsx'

export default function ScoresTab() {
  const { user, applyProgress } = useAuth()
  const { toast, confirm } = useAdminUi()
  const [items, setItems] = useState([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let active = true
    api
      .get('/admin/users', { params: { page: 1, pageSize: 50 } })
      .then((response) => {
        if (active) {
          setItems(response.data.items)
        }
      })
      .catch((error) => {
        if (active) {
          toast(error.message, 'bad')
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
  }, [toast])

  function refreshScores(targetId) {
    setItems((list) => list.map((item) => (
      targetId === null || item.id === targetId
        ? { ...item, xp: 0, level: 1 }
        : item
    )))
    if (targetId === null || targetId === user?.id) {
      applyProgress({ xp: 0, level: 1 })
    }
  }

  async function resetOne(item) {
    const accepted = await confirm({
      title: 'Resetirati bodove?',
      message: `Bodovi, razina i streak korisnika ${item.username} vraćaju se na nulu. Njegovi pokušaji se brišu.`,
      confirmLabel: 'Resetiraj',
    })
    if (!accepted) {
      return
    }

    try {
      const response = await api.post(`/admin/leaderboard/reset/${item.id}`)
      toast(response.data.message)
      refreshScores(item.id)
    } catch (error) {
      toast(error.message, 'bad')
    }
  }

  async function resetAll() {
    const accepted = await confirm({
      title: 'Resetirati sve bodove?',
      message: 'Svi korisnici gube bodove, razinu i streak. Svi pokušaji se brišu. Ovo se ne može vratiti.',
      confirmLabel: 'Resetiraj sve',
    })
    if (!accepted) {
      return
    }

    try {
      const response = await api.post('/admin/leaderboard/reset')
      toast(response.data.message)
      refreshScores(null)
    } catch (error) {
      toast(error.message, 'bad')
    }
  }

  return (
    <div>
      <p className="trace-quiet">
        Reset briše pokušaje. Ukupni i tjedni bodovi padaju na nulu jer obje liste čitaju te pokušaje i XP.
      </p>
      <button type="button" className="button" onClick={resetAll}>Resetiraj sve</button>
      {loading ? <p role="status">Učitavanje...</p> : null}
      {!loading && items.length === 0 ? <p className="empty">Nema korisnika.</p> : null}
      <div className="admin-list">
        {items.map((item) => (
          <div className="admin-row admin-score" key={item.id}>
            <strong>{item.username}</strong>
            <span>{item.xp} XP · razina {item.level}</span>
            <button type="button" onClick={() => resetOne(item)}>Resetiraj</button>
          </div>
        ))}
      </div>
    </div>
  )
}
