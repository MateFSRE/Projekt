import { createContext, useContext, useEffect, useMemo, useState } from 'react'
import api from '../api/client.js'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let active = true

    api
      .get('/auth/me')
      .then((response) => {
        if (active) {
          setUser(response.data)
        }
      })
      .catch(() => {
        if (active) {
          setUser(null)
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

  const value = useMemo(
    () => ({
      user,
      loading,
      async login(payload) {
        const response = await api.post('/auth/login', payload)
        setUser(response.data)
        return response.data
      },
      async register(payload) {
        const response = await api.post('/auth/register', payload)
        setUser(response.data)
        return response.data
      },
      async logout() {
        await api.post('/auth/logout')
        setUser(null)
      },
      applyProgress(progress) {
        setUser((current) => {
          if (!current) {
            return current
          }

          return { ...current, xp: progress.xp, level: progress.level }
        })
      },
    }),
    [user, loading],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth se koristi unutar AuthProvider.')
  }

  return context
}
