import { apiFetch } from './client'
import type {
  CreateReservationPayload,
  PagedResult,
  Reservation,
  UpdateReservationPayload,
} from './types'

export function listReservations(page = 1, pageSize = 100) {
  const q = new URLSearchParams({ page: String(page), pageSize: String(pageSize) })
  return apiFetch<PagedResult<Reservation>>(`/api/reservations?${q}`)
}

export function createReservation(payload: CreateReservationPayload) {
  return apiFetch<Reservation>('/api/reservations', {
    method: 'POST',
    body: JSON.stringify(payload),
  })
}

export function updateReservation(id: string, payload: UpdateReservationPayload) {
  return apiFetch<Reservation>(`/api/reservations/${id}`, {
    method: 'PUT',
    body: JSON.stringify(payload),
  })
}

export function deleteReservation(id: string) {
  return apiFetch<void>(`/api/reservations/${id}`, { method: 'DELETE' })
}
