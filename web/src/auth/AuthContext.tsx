import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react'
import { me as fetchMe, login as apiLogin, register as apiRegister } from '../api/authApi'
import { getStoredToken, setStoredToken } from '../api/client'
import type { UserInfo } from '../api/types'

interface AuthContextValue {
  user: UserInfo | null
  token: string | null
  loading: boolean
  isAdmin: boolean
  login: (email: string, password: string) => Promise<void>
  register: (email: string, password: string, userName?: string) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserInfo | null>(null)
  const [token, setToken] = useState<string | null>(() => getStoredToken())
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    async function bootstrap() {
      const stored = getStoredToken()
      if (!stored) {
        if (!cancelled) {
          setLoading(false)
        }
        return
      }
      try {
        const profile = await fetchMe(stored)
        if (!cancelled) {
          setUser(profile)
          setToken(stored)
        }
      } catch {
        setStoredToken(null)
        if (!cancelled) {
          setUser(null)
          setToken(null)
        }
      } finally {
        if (!cancelled) {
          setLoading(false)
        }
      }
    }
    void bootstrap()
    return () => {
      cancelled = true
    }
  }, [])

  const login = useCallback(async (email: string, password: string) => {
    const res = await apiLogin(email, password)
    setStoredToken(res.accessToken)
    setToken(res.accessToken)
    setUser(res.user)
  }, [])

  const register = useCallback(async (email: string, password: string, userName?: string) => {
    const res = await apiRegister(email, password, userName)
    setStoredToken(res.accessToken)
    setToken(res.accessToken)
    setUser(res.user)
  }, [])

  const logout = useCallback(() => {
    setStoredToken(null)
    setToken(null)
    setUser(null)
  }, [])

  const value = useMemo<AuthContextValue>(() => ({
    user,
    token,
    loading,
    isAdmin: !!user?.roles.includes('Admin'),
    login,
    register,
    logout,
  }), [user, token, loading, login, register, logout])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return ctx
}
