import { Navigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'

export default function AdminRoute({ children }) {
  const { user, loading } = useAuth()

  if (loading) {
    return <p className="page" role="status">Učitavanje...</p>
  }

  if (!user) {
    return <Navigate to="/login" replace />
  }

  if (user.role !== 'Admin') {
    return <Navigate to="/" replace />
  }

  return children
}
