import { useCallback, useEffect, useMemo, useState } from 'react'
import { createReservation, deleteReservation, listReservations, updateReservation } from '../api/reservationsApi'
import { createUnit, deleteUnit, listUnits, updateUnit } from '../api/unitsApi'
import { ApiError, type IgloUnit, type Reservation, type ReservationStatus } from '../api/types'
import { useAuth } from '../auth/AuthContext'
import { DateNav } from '../components/DateNav'
import { IgloUnitsPanel } from '../components/IgloUnitsPanel'
import { ReservationGrid } from '../components/ReservationGrid'
import { ReservationModal, type ReservationModalState } from '../components/ReservationModal'
import {
  addDaysToKey,
  formatTurkishLongDate,
  slotRangeUtc,
  todayIstanbulKey,
  type TimeSlot,
} from '../lib/slots'

export function BoardPage() {
  const { user, isAdmin, logout } = useAuth()
  const [dateKey, setDateKey] = useState(todayIstanbulKey)
  const [units, setUnits] = useState<IgloUnit[]>([])
  const [reservations, setReservations] = useState<Reservation[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [modal, setModal] = useState<ReservationModalState | null>(null)
  const [modalBusy, setModalBusy] = useState(false)
  const [modalError, setModalError] = useState<string | null>(null)
  const [unitsBusy, setUnitsBusy] = useState(false)
  const [unitsError, setUnitsError] = useState<string | null>(null)

  const dateLabel = useMemo(() => formatTurkishLongDate(dateKey), [dateKey])

  const refresh = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const [unitsPage, reservationsPage] = await Promise.all([
        listUnits(1, 100),
        listReservations(1, 100),
      ])
      setUnits(unitsPage.items.filter((u) => u.isActive))
      setReservations(reservationsPage.items)
    } catch (err) {
      const message = err instanceof ApiError ? err.message : 'Veriler yüklenemedi.'
      setError(message)
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    void refresh()
  }, [refresh])

  function openCreate(unit: IgloUnit, slot: TimeSlot) {
    setModalError(null)
    setModal({ mode: 'create', unit, slot })
  }

  function openEdit(unit: IgloUnit, slot: TimeSlot, reservation: Reservation) {
    setModalError(null)
    setModal({ mode: 'edit', unit, slot, reservation })
  }

  function openView(unit: IgloUnit, slot: TimeSlot, reservation: Reservation) {
    setModalError(null)
    setModal({ mode: 'view', unit, slot, reservation })
  }

  async function handleQuickDelete(reservation: Reservation) {
    if (!window.confirm('Bu rezervasyonu silmek istiyor musunuz?')) {
      return
    }
    setError(null)
    try {
      await deleteReservation(reservation.id)
      await refresh()
    } catch (err) {
      setError(err instanceof ApiError ? err.message : 'Silme başarısız.')
    }
  }

  async function handleSave(input: { description: string; status: ReservationStatus }) {
    if (!modal || !user) return
    setModalBusy(true)
    setModalError(null)
    try {
      const range = slotRangeUtc(dateKey, modal.slot)
      if (modal.mode === 'create') {
        await createReservation({
          userId: user.id,
          igloUnitId: modal.unit.id,
          startAtUtc: range.startAtUtc,
          endAtUtc: range.endAtUtc,
          status: input.status,
          description: input.description.trim() || null,
        })
      } else if (modal.mode === 'edit' && modal.reservation) {
        await updateReservation(modal.reservation.id, {
          igloUnitId: modal.unit.id,
          startAtUtc: range.startAtUtc,
          endAtUtc: range.endAtUtc,
          status: input.status,
          description: input.description.trim() || null,
        })
      }
      setModal(null)
      await refresh()
    } catch (err) {
      setModalError(err instanceof ApiError ? err.message : 'Kayıt başarısız.')
    } finally {
      setModalBusy(false)
    }
  }

  async function handleDelete() {
    if (!modal?.reservation) return
    setModalBusy(true)
    setModalError(null)
    try {
      await deleteReservation(modal.reservation.id)
      setModal(null)
      await refresh()
    } catch (err) {
      setModalError(err instanceof ApiError ? err.message : 'Silme başarısız.')
    } finally {
      setModalBusy(false)
    }
  }

  async function handleCreateUnit(name: string) {
    setUnitsBusy(true)
    setUnitsError(null)
    try {
      await createUnit(name, true)
      await refresh()
    } catch (err) {
      setUnitsError(err instanceof ApiError ? err.message : 'Birim eklenemedi.')
    } finally {
      setUnitsBusy(false)
    }
  }

  async function handleRenameUnit(unit: IgloUnit, name: string) {
    if (!name) return
    setUnitsBusy(true)
    setUnitsError(null)
    try {
      await updateUnit(unit.id, name, unit.isActive)
      await refresh()
    } catch (err) {
      setUnitsError(err instanceof ApiError ? err.message : 'Birim güncellenemedi.')
    } finally {
      setUnitsBusy(false)
    }
  }

  async function handleDeleteUnit(unit: IgloUnit) {
    setUnitsBusy(true)
    setUnitsError(null)
    try {
      await deleteUnit(unit.id)
      await refresh()
    } catch (err) {
      setUnitsError(err instanceof ApiError ? err.message : 'Birim silinemedi.')
    } finally {
      setUnitsBusy(false)
    }
  }

  return (
    <div className="mx-auto min-h-screen w-full max-w-[1920px] px-2 py-2 sm:px-3 sm:py-3">
      <header className="mb-2 flex flex-wrap items-center justify-between gap-2 rounded-xl border border-[#c5d2e3] bg-white/90 px-3 py-2 shadow-sm">
        <div className="min-w-0">
          <p className="truncate text-[10px] font-semibold tracking-[0.14em] text-[var(--iglo-accent)] uppercase">
            SGK Üsküdar Sosyal Tesisi ve Eğitim Merkezi
          </p>
          <h1 className="font-display text-xl font-bold tracking-tight text-[var(--iglo-navy)] sm:text-2xl">
            İglo Rezervasyon Tablosu
          </h1>
        </div>

        <div className="flex flex-wrap items-center gap-2">
          <div className="flex items-center gap-2 rounded-lg border border-[#d0dbe8] bg-[#f5f8fb] px-2.5 py-1.5 text-sm">
            <span className="font-semibold text-[var(--iglo-ink)]">
              {user?.userName || user?.email}
            </span>
            <span className="rounded bg-[var(--iglo-navy)] px-1.5 py-0.5 text-[10px] font-bold tracking-wide text-white">
              {isAdmin ? 'Admin' : 'User'}
            </span>
          </div>

          <button
            type="button"
            onClick={logout}
            className="inline-flex h-9 items-center justify-center gap-1.5 rounded-lg border-2 border-[#9a3b3b] bg-[#fff5f5] px-3 text-xs font-bold text-[#8f2f2f] transition hover:bg-[#fde8e8]"
            aria-label="Oturumu kapat"
          >
            <svg aria-hidden="true" viewBox="0 0 20 20" className="h-3.5 w-3.5 fill-current">
              <path d="M3 3.5A1.5 1.5 0 0 1 4.5 2H10a1 1 0 1 1 0 2H5v12h5a1 1 0 1 1 0 2H4.5A1.5 1.5 0 0 1 3 16.5v-13Z" />
              <path d="M13.3 6.3a1 1 0 0 1 1.4 0l3 3a1 1 0 0 1 0 1.4l-3 3a1 1 0 1 1-1.4-1.4L14.58 11H8a1 1 0 1 1 0-2h6.58l-1.28-1.3a1 1 0 0 1 0-1.4Z" />
            </svg>
            Çıkış Yap
          </button>
        </div>
      </header>

      <div className="mb-2">
        <DateNav
          dateKey={dateKey}
          label={dateLabel}
          onChange={setDateKey}
          onPrev={() => setDateKey((d) => addDaysToKey(d, -1))}
          onNext={() => setDateKey((d) => addDaysToKey(d, 1))}
          onToday={() => setDateKey(todayIstanbulKey())}
          onRefresh={() => void refresh()}
        />
      </div>

      {isAdmin && (
        <IgloUnitsPanel
          units={units}
          busy={unitsBusy}
          error={unitsError}
          onCreate={handleCreateUnit}
          onRename={handleRenameUnit}
          onDelete={handleDeleteUnit}
        />
      )}

      {error && (
        <p className="mb-2 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700" role="alert">
          {error}
        </p>
      )}

      {loading ? (
        <p className="py-10 text-center text-[var(--iglo-muted)]">Tablo yükleniyor…</p>
      ) : (
        <ReservationGrid
          dateKey={dateKey}
          units={units}
          reservations={reservations}
          isAdmin={isAdmin}
          onAdd={openCreate}
          onEdit={openEdit}
          onDelete={(r) => void handleQuickDelete(r)}
          onView={openView}
        />
      )}

      <ReservationModal
        open={!!modal}
        state={modal}
        isAdmin={isAdmin}
        busy={modalBusy}
        error={modalError}
        onClose={() => setModal(null)}
        onSave={handleSave}
        onDelete={isAdmin ? handleDelete : undefined}
      />
    </div>
  )
}
