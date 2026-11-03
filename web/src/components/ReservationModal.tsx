import { useEffect, useState, type FormEvent } from 'react'
import type { IgloUnit, Reservation, ReservationStatus } from '../api/types'
import { STATUS_OPTIONS } from '../lib/statusLabels'
import type { TimeSlot } from '../lib/slots'

export interface ReservationModalState {
  mode: 'create' | 'edit' | 'view'
  unit: IgloUnit
  slot: TimeSlot
  reservation?: Reservation
}

interface ReservationModalProps {
  open: boolean
  state: ReservationModalState | null
  isAdmin: boolean
  busy: boolean
  error: string | null
  onClose: () => void
  onSave: (input: {
    description: string
    status: ReservationStatus
  }) => Promise<void>
  onDelete?: () => Promise<void>
}

export function ReservationModal({
  open,
  state,
  isAdmin,
  busy,
  error,
  onClose,
  onSave,
  onDelete,
}: ReservationModalProps) {
  const [description, setDescription] = useState('')
  const [status, setStatus] = useState<ReservationStatus>('Pending')

  useEffect(() => {
    if (!state) return
    setDescription(state.reservation?.description ?? '')
    setStatus(state.reservation?.status ?? 'Pending')
  }, [state])

  if (!open || !state) {
    return null
  }

  const readOnly = !isAdmin || state.mode === 'view'

  async function submit(e: FormEvent) {
    e.preventDefault()
    if (readOnly) return
    await onSave({ description, status })
  }

  return (
    <div className="fixed inset-0 z-50 flex items-end justify-center bg-black/40 p-4 sm:items-center" role="dialog" aria-modal="true">
      <div className="max-h-[90vh] w-full max-w-lg overflow-auto rounded-2xl bg-white shadow-2xl">
        <div className="border-b border-[#dce3ec] px-5 py-4">
          <h2 className="text-lg font-semibold text-[var(--iglo-navy)]">
            {state.mode === 'create' ? 'Yeni rezervasyon' : state.mode === 'edit' ? 'Rezervasyonu düzenle' : 'Rezervasyon'}
          </h2>
          <p className="mt-1 text-sm text-[var(--iglo-muted)]">
            {state.unit.name} · {state.slot.label}
          </p>
        </div>

        <form className="space-y-4 px-5 py-4" onSubmit={submit}>
          <label className="block text-sm font-medium">
            Açıklama
            <textarea
              rows={5}
              maxLength={1000}
              readOnly={readOnly}
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Örn. Adem Barış 2 kişi 500 tl kapora …"
              className="mt-1 w-full rounded-lg border border-[#c9d4e3] bg-[#fbfcfe] px-3 py-3 text-base font-semibold leading-relaxed outline-none focus:border-[var(--iglo-navy)] read-only:bg-[#f3f5f8]"
            />
          </label>

          <label className="block text-sm font-medium">
            Durum
            <select
              disabled={readOnly}
              value={status}
              onChange={(e) => setStatus(e.target.value as ReservationStatus)}
              className="mt-1 w-full rounded-lg border border-[#c9d4e3] bg-[#fbfcfe] px-3 py-2 text-sm disabled:bg-[#f3f5f8]"
            >
              {STATUS_OPTIONS.map((s) => (
                <option key={s.value} value={s.value}>{s.label}</option>
              ))}
            </select>
          </label>

          {error && (
            <p className="rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700" role="alert">{error}</p>
          )}

          <div className="flex flex-wrap justify-end gap-2 pt-2">
            <button
              type="button"
              onClick={onClose}
              className="rounded-lg border border-[#c9d4e3] px-4 py-2 text-sm font-medium hover:bg-[#f3f6fa]"
            >
              Kapat
            </button>
            {isAdmin && state.mode === 'edit' && onDelete && (
              <button
                type="button"
                disabled={busy}
                onClick={() => void onDelete()}
                className="rounded-lg bg-red-700 px-4 py-2 text-sm font-medium text-white hover:bg-red-800 disabled:opacity-60"
              >
                Sil
              </button>
            )}
            {!readOnly && (
              <button
                type="submit"
                disabled={busy}
                className="rounded-lg bg-[var(--iglo-navy)] px-4 py-2 text-sm font-medium text-white hover:bg-[#162c48] disabled:opacity-60"
              >
                {busy ? 'Kaydediliyor…' : 'Kaydet'}
              </button>
            )}
          </div>
        </form>
      </div>
    </div>
  )
}
