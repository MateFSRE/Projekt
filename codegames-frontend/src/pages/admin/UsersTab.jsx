import { useEffect, useState } from 'react'
import { useForm } from 'react-hook-form'
import api from '../../api/client.js'
import { useAuth } from '../../auth/AuthContext.jsx'
import { useAdminUi } from './AdminUi.jsx'

const emptyUser = { email: '', username: '', password: '', role: 'User' }

export default function UsersTab() {
  const { user: current } = useAuth()
  const { toast, confirm } = useAdminUi()
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')
  const [result, setResult] = useState(null)
  const [loading, setLoading] = useState(true)
  const [editing, setEditing] = useState(null)
  const [serverError, setServerError] = useState('')

  const searchForm = useForm({ defaultValues: { search: '' } })
  const form = useForm({ defaultValues: emptyUser })

  useEffect(() => {
    let active = true
    setLoading(true)

    api
      .get('/admin/users', { params: { search, page, pageSize: 8 } })
      .then((response) => {
        if (active) {
          setResult(response.data)
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
  }, [page, search, toast])

  function openCreate() {
    setEditing('new')
    setServerError('')
    form.reset(emptyUser)
  }

  function openEdit(item) {
    setEditing(item.id)
    setServerError('')
    form.reset({
      email: item.email,
      username: item.username,
      password: '',
      role: item.role,
    })
  }

  async function onSearch(values) {
    setPage(1)
    setSearch(values.search.trim())
  }

  async function onSave(values) {
    setServerError('')
    try {
      if (editing === 'new') {
        await api.post('/admin/users', values)
        toast('Korisnik je kreiran.')
      } else {
        await api.put(`/admin/users/${editing}`, {
          email: values.email,
          username: values.username,
          role: values.role,
        })
        toast('Korisnik je spremljen.')
      }

      setEditing(null)
      setPage(1)
      setSearch((value) => value)
      setResult(null)
      setLoading(true)
      const response = await api.get('/admin/users', { params: { search, page: 1, pageSize: 8 } })
      setResult(response.data)
      setLoading(false)
    } catch (error) {
      setServerError(error.message)
    }
  }

  async function onDelete(item) {
    const accepted = await confirm({
      title: 'Obrisati korisnika?',
      message: `Briše se ${item.username} i svi njegovi pokušaji. Ovo se ne može vratiti.`,
      confirmLabel: 'Obriši',
    })
    if (!accepted) {
      return
    }

    try {
      const response = await api.delete(`/admin/users/${item.id}`)
      toast(response.data.message)
      setResult(null)
      setLoading(true)
      const next = await api.get('/admin/users', { params: { search, page, pageSize: 8 } })
      setResult(next.data)
      setLoading(false)
    } catch (error) {
      toast(error.message, 'bad')
    }
  }

  const items = result?.items ?? []
  const pages = result ? Math.max(1, Math.ceil(result.total / result.pageSize)) : 1

  return (
    <div>
      <form className="admin-toolbar" onSubmit={searchForm.handleSubmit(onSearch)}>
        <input
          type="search"
          placeholder="Ime ili e-mail"
          aria-label="Pretraga korisnika"
          {...searchForm.register('search')}
        />
        <button type="submit">Traži</button>
        <button type="button" className="button" onClick={openCreate}>Novi korisnik</button>
      </form>

      {loading ? <p role="status">Učitavanje...</p> : null}
      {!loading && items.length === 0 ? <p className="empty">Nema korisnika za ovu pretragu.</p> : null}

      <div className="admin-list admin-users">
        {items.map((item) => (
          <div className="admin-row" key={item.id}>
            <strong>{item.username}</strong>
            <span>{item.email}</span>
            <span>{item.role === 'Admin' ? 'Admin' : 'Korisnik'}</span>
            <span>{item.xp} XP</span>
            <span className="admin-actions">
              <button type="button" onClick={() => openEdit(item)}>Uredi</button>
              <button
                type="button"
                onClick={() => onDelete(item)}
                disabled={item.id === current?.id}
              >
                Obriši
              </button>
            </span>
          </div>
        ))}
      </div>

      <div className="admin-pager">
        <button type="button" disabled={page <= 1} onClick={() => setPage((value) => value - 1)}>Prethodna</button>
        <span>Stranica {page} / {pages}</span>
        <button type="button" disabled={page >= pages} onClick={() => setPage((value) => value + 1)}>Sljedeća</button>
      </div>

      {editing ? (
        <form className="form panel" onSubmit={form.handleSubmit(onSave)} noValidate>
          <h2>{editing === 'new' ? 'Novi korisnik' : 'Uredi korisnika'}</h2>
          <label>
            E-mail
            <input type="email" {...form.register('email', { required: 'E-mail je obavezan.' })} />
            {form.formState.errors.email ? <span className="error">{form.formState.errors.email.message}</span> : null}
          </label>
          <label>
            Korisničko ime
            <input {...form.register('username', {
              required: 'Korisničko ime je obavezno.',
              minLength: { value: 3, message: 'Najmanje 3 znaka.' },
            })}
            />
            {form.formState.errors.username ? <span className="error">{form.formState.errors.username.message}</span> : null}
          </label>
          {editing === 'new' ? (
            <label>
              Lozinka
              <input type="password" {...form.register('password', {
                required: 'Lozinka je obavezna.',
                minLength: { value: 8, message: 'Najmanje 8 znakova.' },
              })}
              />
              {form.formState.errors.password ? <span className="error">{form.formState.errors.password.message}</span> : null}
            </label>
          ) : null}
          <label>
            Uloga
            <select {...form.register('role', { required: true })}>
              <option value="User">Korisnik</option>
              <option value="Admin">Admin</option>
            </select>
          </label>
          {serverError ? <p className="error" role="alert">{serverError}</p> : null}
          <div className="admin-actions">
            <button type="submit" className="button" disabled={form.formState.isSubmitting}>Spremi</button>
            <button type="button" onClick={() => setEditing(null)}>Odustani</button>
          </div>
        </form>
      ) : null}
    </div>
  )
}
