import { useState } from 'react'
import ChallengesTab from './admin/ChallengesTab.jsx'
import ScoresTab from './admin/ScoresTab.jsx'
import { AdminUiProvider } from './admin/AdminUi.jsx'
import UsersTab from './admin/UsersTab.jsx'

const tabs = [
  { id: 'users', label: 'Korisnici' },
  { id: 'challenges', label: 'Zadaci' },
  { id: 'scores', label: 'Rang lista' },
]

export default function AdminPage() {
  const [tab, setTab] = useState('users')

  return (
    <AdminUiProvider>
      <section className="page page-wide">
        <h1>Admin</h1>
        <div className="period-switch" role="tablist" aria-label="Admin odjeljci">
          {tabs.map((item) => (
            <button
              key={item.id}
              id={`${item.id}-tab`}
              type="button"
              role="tab"
              aria-selected={tab === item.id}
              aria-controls={`${item.id}-panel`}
              className={tab === item.id ? 'period-tab period-tab-active' : 'period-tab'}
              onClick={() => setTab(item.id)}
            >
              {item.label}
            </button>
          ))}
        </div>
        <div id={`${tab}-panel`} role="tabpanel" aria-labelledby={`${tab}-tab`}>
          {tab === 'users' ? <UsersTab /> : null}
          {tab === 'challenges' ? <ChallengesTab /> : null}
          {tab === 'scores' ? <ScoresTab /> : null}
        </div>
      </section>
    </AdminUiProvider>
  )
}
