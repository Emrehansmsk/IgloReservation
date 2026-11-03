import { useState, type FormEvent } from 'react'
import { Link, Navigate, useLocation, useNavigate } from 'react-router-dom'
import { ApiError } from '../api/types'
import { useAuth } from '../auth/AuthContext'

export function LoginPage() {
  const { login, user, loading } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const from = (location.state as { from?: string } | null)?.from ?? '/'

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  if (!loading && user) {
    return <Navigate to={from} replace />
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      await login(email.trim(), password)
      navigate(from, { replace: true })
    } catch (err) {
      if (err instanceof ApiError) {
        const msg = err.message.trim()
        const looksEnglishAuth =
          /authentication is required|credentials are invalid|invalid credentials/i.test(msg)
        setError(looksEnglishAuth ? 'Kullanıcı adı veya parolanız yanlış.' : msg || 'Giriş başarısız.')
      } else {
        setError('Giriş başarısız.')
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="mx-auto flex min-h-screen max-w-md flex-col justify-center px-4 py-10">
      <div className="rounded-2xl border border-[#d5dde8] bg-white/90 p-8 shadow-[0_20px_60px_-30px_rgba(30,58,95,0.45)] backdrop-blur">
        <p className="text-xs font-semibold tracking-[0.2em] text-[var(--iglo-muted)] uppercase">
          SGK Üsküdar
        </p>
        <h1 className="mt-2 font-serif text-3xl tracking-tight text-[var(--iglo-navy)]">
          IgloReservation
        </h1>
        <p className="mt-2 text-sm text-[var(--iglo-muted)]">Hesabınızla giriş yapın.</p>

        <form className="mt-8 space-y-4" onSubmit={(e) => void onSubmit(e)}>
          <label className="block text-sm font-medium">
            E-posta
            <input
              type="email"
              required
              autoComplete="username"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="mt-1 w-full rounded-lg border border-[#c9d4e3] bg-[#fbfcfe] px-3 py-2 outline-none focus:border-[var(--iglo-navy)]"
            />
          </label>
          <label className="block text-sm font-medium">
            Parola
            <input
              type="password"
              required
              minLength={8}
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="mt-1 w-full rounded-lg border border-[#c9d4e3] bg-[#fbfcfe] px-3 py-2 outline-none focus:border-[var(--iglo-navy)]"
            />
          </label>
          {error && (
            <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700" role="alert">
              {error}
            </p>
          )}
          <button
            type="submit"
            disabled={submitting}
            className="w-full rounded-lg bg-[var(--iglo-navy)] px-4 py-2.5 font-medium text-white transition hover:bg-[#162c48] disabled:opacity-60"
          >
            {submitting ? 'Giriş yapılıyor…' : 'Giriş yap'}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-[var(--iglo-muted)]">
          Hesabınız yok mu?{' '}
          <Link className="font-medium text-[var(--iglo-accent)] underline-offset-2 hover:underline" to="/register">
            Kayıt ol
          </Link>
        </p>
      </div>
    </div>
  )
}
