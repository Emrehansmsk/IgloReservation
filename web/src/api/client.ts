import { ApiError } from './types'

const API_BASE = (import.meta.env.VITE_API_BASE_URL as string | undefined)?.replace(/\/$/, '')
  ?? 'http://localhost:5066'

const TOKEN_KEY = 'iglo.accessToken'

export function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function setStoredToken(token: string | null): void {
  if (token) {
    localStorage.setItem(TOKEN_KEY, token)
  } else {
    localStorage.removeItem(TOKEN_KEY)
  }
}

export async function apiFetch<T>(
  path: string,
  options: RequestInit = {},
  token?: string | null,
): Promise<T> {
  const headers = new Headers(options.headers)
  if (!headers.has('Content-Type') && options.body) {
    headers.set('Content-Type', 'application/json')
  }

  const auth = token === undefined ? getStoredToken() : token
  if (auth) {
    headers.set('Authorization', `Bearer ${auth}`)
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers,
  })

  if (response.status === 204) {
    return undefined as T
  }

  const text = await response.text()
  let data: unknown = null
  if (text) {
    try {
      data = JSON.parse(text)
    } catch {
      data = text
    }
  }

  if (!response.ok) {
    const title =
      typeof data === 'object' && data && 'title' in data
        ? String((data as { title?: string }).title)
        : response.statusText
    const detail =
      typeof data === 'object' && data && 'detail' in data
        ? String((data as { detail?: string }).detail)
        : undefined
    throw new ApiError(response.status, detail ?? title ?? 'Request failed', data)
  }

  return data as T
}
