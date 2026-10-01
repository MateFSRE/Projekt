import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { Link, useNavigate } from 'react-router-dom'
import { useAuth } from '../auth/AuthContext.jsx'

export default function RegisterPage() {
  const { register: registerUser } = useAuth()
  const navigate = useNavigate()
  const [serverError, setServerError] = useState('')
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm()

  async function onSubmit(values) {
    setServerError('')
    try {
      await registerUser(values)
      navigate('/')
    } catch (error) {
      setServerError(error.message)
    }
  }

  return (
    <section className="page">
      <h1>Registracija</h1>
      <form className="form" onSubmit={handleSubmit(onSubmit)} noValidate>
        <label htmlFor="register-email">
          E-mail
          <input
            id="register-email"
            type="email"
            autoComplete="email"
            aria-invalid={errors.email ? 'true' : 'false'}
            aria-describedby={errors.email ? 'register-email-error' : undefined}
            {...register('email', { required: 'E-mail je obavezan.' })}
          />
          {errors.email ? <span id="register-email-error" className="error" role="alert">{errors.email.message}</span> : null}
        </label>
        <label htmlFor="register-username">
          Korisničko ime
          <input
            id="register-username"
            autoComplete="username"
            aria-invalid={errors.username ? 'true' : 'false'}
            aria-describedby={errors.username ? 'register-username-error' : undefined}
            {...register('username', {
              required: 'Korisničko ime je obavezno.',
              minLength: { value: 3, message: 'Korisničko ime mora imati najmanje 3 znaka.' },
            })}
          />
          {errors.username ? <span id="register-username-error" className="error" role="alert">{errors.username.message}</span> : null}
        </label>
        <label htmlFor="register-password">
          Lozinka
          <input
            id="register-password"
            type="password"
            autoComplete="new-password"
            aria-invalid={errors.password ? 'true' : 'false'}
            aria-describedby={errors.password ? 'register-password-error' : undefined}
            {...register('password', {
              required: 'Lozinka je obavezna.',
              minLength: { value: 8, message: 'Lozinka mora imati najmanje 8 znakova.' },
            })}
          />
          {errors.password ? <span id="register-password-error" className="error" role="alert">{errors.password.message}</span> : null}
        </label>
        {serverError ? <p className="error" role="alert">{serverError}</p> : null}
        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? 'Spremanje...' : 'Registriraj se'}
        </button>
      </form>
      <p>
        Već imaš račun? <Link to="/login">Prijavi se</Link>
      </p>
    </section>
  )
}
