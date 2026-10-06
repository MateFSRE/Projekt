import { CalendarDays, Gamepad2, House, LogOut, Shield, Trophy, User } from 'lucide-react'
import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'

const links = [
  { to: '/', label: 'Naslovna', icon: House, roles: ['guest', 'User', 'Admin'] },
  { to: '/demo', label: 'Demo', icon: Gamepad2, roles: ['guest', 'User', 'Admin'] },
  { to: '/games', label: 'Igre', icon: Gamepad2, roles: ['User', 'Admin'] },
  { to: '/daily', label: 'Dnevni izazovi', icon: CalendarDays, roles: ['User', 'Admin'] },
  { to: '/leaderboard', label: 'Rang lista', icon: Trophy, roles: ['guest', 'User', 'Admin'] },
  { to: '/profile', label: 'Profil', icon: User, roles: ['User', 'Admin'] },
  { to: '/admin', label: 'Admin', icon: Shield, roles: ['Admin'] },
]

export default function Layout() {
  const { user, logout } = useAuth()
  const navigate = useNavigate()
  const role = user?.role ?? 'guest'
  const visibleLinks = links.filter((link) => link.roles.includes(role))

  async function onLogout() {
    await logout()
    navigate('/')
  }

  return (
    <div className="layout">
      <a className="skip-link" href="#sadrzaj">Preskoči na sadržaj</a>
      <header className="header">
        <div className="brand-block">
          <p className="brand">CodeGames</p>
          {user ? (
            <p className="player-stats">
              {user.xp} XP · razina {user.level}
            </p>
          ) : null}
        </div>
        <nav className="nav" aria-label="Glavna navigacija">
          {visibleLinks.map((link) => {
            const Icon = link.icon
            return (
              <NavLink
                key={link.to}
                to={link.to}
                end={link.to === '/'}
                className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              >
                <Icon size={18} aria-hidden="true" />
                {link.label}
              </NavLink>
            )
          })}
          {user ? (
            <button type="button" className="nav-link nav-button" onClick={onLogout}>
              <LogOut size={18} aria-hidden="true" />
              Odjava
            </button>
          ) : null}
        </nav>
      </header>
      <main id="sadrzaj">
        <Outlet />
      </main>
    </div>
  )
}
