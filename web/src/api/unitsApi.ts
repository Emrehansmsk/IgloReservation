import { apiFetch } from './client'
import type { IgloUnit, PagedResult } from './types'

export function listUnits(page = 1, pageSize = 100) {
  const q = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  return apiFetch<PagedResult<IgloUnit>>(`/api/iglo-units?${q}`)
}

export function createUnit(name: string, isActive = true) {
  return apiFetch<IgloUnit>('/api/iglo-units', {
    method: 'POST',
    body: JSON.stringify({ name, isActive }),
  })
}

export function updateUnit(id: string, name: string, isActive: boolean) {
  return apiFetch<IgloUnit>(`/api/iglo-units/${id}`, {
    method: 'PUT',
    body: JSON.stringify({ name, isActive }),
  })
}

export function deleteUnit(id: string) {
  return apiFetch<void>(`/api/iglo-units/${id}`, { method: 'DELETE' })
}
