import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'

export default function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [serverError, setServerError] = useState('')
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm()

  async function onSubmit(values) {
    setServerError('')
    try {
      await login(values)
      navigate(location.state?.from || '/')
    } catch (error) {
      setServerError(error.message)
    }
  }

  return (
    <section className="page">
      <h1>Prijava</h1>
      <form className="form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <label htmlFor="login-email">
          E-mail
          <input
            id="login-email"
            type="email"
            autoComplete="email"
            aria-invalid={errors.email ? 'true' : 'false'}
            aria-describedby={errors.email ? 'login-email-error' : undefined}
            {...register('email', { required: 'E-mail je obavezan.' })}
          />
          {errors.email ? <span id="login-email-error" className="error" role="alert">{errors.email.message}</span> : null}
        </label>
        <label htmlFor="login-password">
          Lozinka
          <input
            id="login-password"
            type="password"
            autoComplete="current-password"
            aria-invalid={errors.password ? 'true' : 'false'}
            aria-describedby={errors.password ? 'login-password-error' : undefined}
            {...register('password', { required: 'Lozinka je obavezna.' })}
          />
          {errors.password ? <span id="login-password-error" className="error" role="alert">{errors.password.message}</span> : null}
        </label>
        {serverError ? <p className="error" role="alert">{serverError}</p> : null}
        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Prijava...' : 'Prijavi se'}
        </button>
      </form>
      <p>
        Nemaš račun? <Link to="/register">Registriraj se</Link>
      </p>
    </section>
  )
}
