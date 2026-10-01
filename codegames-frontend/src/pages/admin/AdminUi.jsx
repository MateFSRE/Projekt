import { createContext, useCallback, useContext, useEffect, useMemo, useRef, useState } from 'react'

const AdminUiContext = createContext(null)

export function AdminUiProvider({ children }) {
  const [toasts, setToasts] = useState([])
  const [pending, setPending] = useState(null)
  const cancelRef = useRef(null)

  useEffect(() => {
    if (!pending) {
      return undefined
    }

    cancelRef.current?.focus()

    function onKey(event) {
      if (event.key === 'Escape') {
        pending.resolve(false)
        setPending(null)
      }
    }

    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [pending])

  const toast = useCallback((message, tone = 'ok') => {
    const id = `${Date.now()}-${Math.random()}`
    setToasts((list) => [...list, { id, message, tone }])
    window.setTimeout(() => {
      setToasts((list) => list.filter((item) => item.id !== id))
    }, 4000)
  }, [])

  const confirm = useCallback((options) => new Promise((resolve) => {
    setPending({
      title: options.title,
      message: options.message,
      confirmLabel: options.confirmLabel ?? 'Potvrdi',
      resolve,
    })
  }), [])

  const value = useMemo(() => ({ toast, confirm }), [toast, confirm])

  function close(accepted) {
    pending?.resolve(accepted)
    setPending(null)
  }

  return (
    <AdminUiContext.Provider value={value}>
      {children}
      <div className="toasts" aria-live="polite">
        {toasts.map((item) => (
          <p key={item.id} className={item.tone === 'bad' ? 'toast toast-bad' : 'toast'}>
            {item.message}
          </p>
        ))}
      </div>
      {pending ? (
        <div className="dialog-backdrop" role="presentation">
          <div className="dialog" role="dialog" aria-modal="true" aria-labelledby="confirm-title" aria-describedby="confirm-message">
            <h2 id="confirm-title">{pending.title}</h2>
            <p id="confirm-message">{pending.message}</p>
            <div className="dialog-actions">
              <button ref={cancelRef} type="button" className="button-secondary button" onClick={() => close(false)}>
                Odustani
              </button>
              <button type="button" className="button" onClick={() => close(true)}>
                {pending.confirmLabel}
              </button>
            </div>
          </div>
        </div>
      ) : null}
    </AdminUiContext.Provider>
  )
}

export function useAdminUi() {
  const context = useContext(AdminUiContext)
  if (!context) {
    throw new Error('useAdminUi se koristi unutar admin stranice.')
  }

  return context
}
