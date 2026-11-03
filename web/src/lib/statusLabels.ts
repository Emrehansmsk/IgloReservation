import type { ReservationStatus } from '../api/types'

/** UI labels (TR). API values stay English. */
export const STATUS_OPTIONS: { value: ReservationStatus; label: string }[] = [
  { value: 'Pending', label: 'Beklemede' },
  { value: 'Confirmed', label: 'Onaylandı' },
  { value: 'Cancelled', label: 'İptal' },
  { value: 'Completed', label: 'Tamamlandı' },
]

export function statusLabel(status: ReservationStatus | string | undefined): string {
  switch (status) {
    case 'Pending':
      return 'Beklemede'
    case 'Confirmed':
      return 'Onaylandı'
    case 'Cancelled':
      return 'İptal'
    case 'Completed':
      return 'Tamamlandı'
    default:
      return status ?? ''
  }
}

/** High-contrast badge classes for grid / modal status chips. */
export function statusBadgeClass(status: ReservationStatus | string | undefined): string {
  switch (status) {
    case 'Pending':
      return 'bg-amber-100 text-amber-900 ring-1 ring-inset ring-amber-400'
    case 'Confirmed':
      return 'bg-emerald-100 text-emerald-900 ring-1 ring-inset ring-emerald-500'
    case 'Cancelled':
      return 'bg-red-100 text-red-900 ring-1 ring-inset ring-red-400'
    case 'Completed':
      return 'bg-slate-200 text-slate-800 ring-1 ring-inset ring-slate-400'
    default:
      return 'bg-slate-100 text-slate-700 ring-1 ring-inset ring-slate-300'
  }
}
