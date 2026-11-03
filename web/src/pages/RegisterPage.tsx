import { useState, type FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router-dom'
import { ApiError } from '../api/types'
import { useAuth } from '../auth/AuthContext'

export function RegisterPage() {
  const { register, user, loading } = useAuth()
  const navigate = useNavigate()
  const [email, setEmail] = useState('')
  const [userName, setUserName] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  if (!loading && user) {
    return <Navigate to="/" replace />
  }

  async function onSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      await register(email.trim(), password, userName.trim() || undefined)
      navigate('/', { replace: true })
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Kayıt başarısız.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="mx-auto flex min-h-screen max-w-md flex-col justify-center px-4 py-10">
      <div className="rounded-2xl border border-[#d5dde8] bg-white/90 p-8 shadow-[0_20px_60px_-30px_rgba(30,58,95,0.45)] backdrop-blur">
        <h1 className="font-serif text-3xl tracking-tight text-[var(--iglo-navy)]">Kayıt ol</h1>
        <p className="mt-2 text-sm text-[var(--iglo-muted)]">
          Yeni hesaplar <strong>User</strong> rolüyle oluşturulur.
        </p>

        <form className="mt-8 space-y-4" onSubmit={(e) => void onSubmit(e)}>
          <label className="block text-sm font-medium">
            E-posta
            <input
              type="email"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="mt-1 w-full rounded-lg border border-[#c9d4e3] bg-[#fbfcfe] px-3 py-2 outline-none focus:border-[var(--iglo-navy)]"
            />
          </label>
          <label className="block text-sm font-medium">
            Kullanıcı adı (opsiyonel)
            <input
              type="text"
              value={userName}
              onChange={(e) => setUserName(e.target.value)}
              className="mt-1 w-full rounded-lg border border-[#c9d4e3] bg-[#fbfcfe] px-3 py-2 outline-none focus:border-[var(--iglo-navy)]"
            />
          </label>
          <label className="block text-sm font-medium">
            Parola
            <input
              type="password"
              required
              minLength={8}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="mt-1 w-full rounded-lg border border-[#c9d4e3] bg-[#fbfcfe] px-3 py-2 outline-none focus:border-[var(--iglo-navy)]"
            />
            <span className="mt-1 block text-xs text-[var(--iglo-muted)]">
              En az 8 karakter; büyük harf ve özel karakter önerilir.
            </span>
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
            {submitting ? 'Kaydediliyor…' : 'Kayıt ol'}
          </button>
        </form>

        <p className="mt-6 text-center text-sm text-[var(--iglo-muted)]">
          Zaten hesabınız var mı?{' '}
          <Link className="font-medium text-[var(--iglo-accent)] underline-offset-2 hover:underline" to="/login">
            Giriş yap
          </Link>
        </p>
      </div>
    </div>
  )
}
