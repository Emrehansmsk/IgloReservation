import { apiFetch } from './client'
import type { AuthResponse, UserInfo } from './types'

export function login(email: string, password: string) {
  return apiFetch<AuthResponse>('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify({ email, password }),
  }, null)
}

export function register(email: string, password: string, userName?: string) {
  return apiFetch<AuthResponse>('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify({ email, password, userName }),
  }, null)
}

export function me(token: string) {
  return apiFetch<UserInfo>('/api/auth/me', {}, token)
}
