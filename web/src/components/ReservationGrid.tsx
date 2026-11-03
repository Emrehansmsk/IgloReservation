import type { IgloUnit, Reservation } from '../api/types'
import { DAY_SLOTS, rangesOverlap, slotRangeUtc, type TimeSlot } from '../lib/slots'
import { statusBadgeClass, statusLabel } from '../lib/statusLabels'

interface ReservationGridProps {
  dateKey: string
  units: IgloUnit[]
  reservations: Reservation[]
  isAdmin: boolean
  onAdd: (unit: IgloUnit, slot: TimeSlot) => void
  onEdit: (unit: IgloUnit, slot: TimeSlot, reservation: Reservation) => void
  onDelete: (reservation: Reservation) => void
  onView: (unit: IgloUnit, slot: TimeSlot, reservation: Reservation) => void
}

function findForCell(
  reservations: Reservation[],
  unitId: string,
  dateKey: string,
  slot: TimeSlot,
): Reservation | undefined {
  const range = slotRangeUtc(dateKey, slot)
  const matches = reservations.filter(
    (r) =>
      r.igloUnitId === unitId &&
      rangesOverlap(r.startAtUtc, r.endAtUtc, range.startAtUtc, range.endAtUtc) &&
      r.status !== 'Cancelled',
  )
  return matches[0]
}

function statusTone(status: Reservation['status'] | undefined): string {
  switch (status) {
    case 'Confirmed':
      return 'border-l-[5px] border-l-emerald-600 bg-emerald-50/40'
    case 'Pending':
      return 'border-l-[5px] border-l-amber-500 bg-amber-50/50'
    case 'Completed':
      return 'border-l-[5px] border-l-slate-400'
    case 'Cancelled':
      return 'border-l-[5px] border-l-red-500'
    default:
      return ''
  }
}

function StatusBadge({ status }: { status: Reservation['status'] }) {
  return (
    <span
      className={`inline-flex w-fit max-w-full truncate items-center rounded px-1.5 py-0.5 text-[10px] font-bold tracking-wide ${statusBadgeClass(status)}`}
    >
      {statusLabel(status)}
    </span>
  )
}

function CellActions({
  isAdmin,
  reservation,
  onAdd,
  onEdit,
  onDelete,
  onView,
}: {
  isAdmin: boolean
  reservation?: Reservation
  onAdd: () => void
  onEdit: () => void
  onDelete: () => void
  onView: () => void
}) {
  if (!isAdmin) {
    if (!reservation) {
      return <span className="text-[10px] text-[#8a98a8]">—</span>
    }
    return (
      <button
        type="button"
        onClick={onView}
        className="inline-flex h-7 items-center rounded-full border border-[#b8c6d8] bg-white/90 px-2 text-[11px] font-semibold text-[var(--iglo-navy)] shadow-sm"
      >
        Görüntüle
      </button>
    )
  }

  if (!reservation) {
    return (
      <button
        type="button"
        onClick={onAdd}
        title="Rezervasyon ekle"
        className="inline-flex h-7 w-7 items-center justify-center rounded-full bg-[var(--iglo-navy)] text-base font-bold leading-none text-white shadow-sm transition hover:scale-105 hover:bg-[#162c48]"
      >
        +
      </button>
    )
  }

  return (
    <div className="flex flex-wrap gap-1">
      <button
        type="button"
        onClick={onEdit}
        className="inline-flex h-7 items-center rounded-full bg-emerald-700 px-2 text-[11px] font-semibold text-white shadow-sm transition hover:bg-emerald-800"
      >
        Güncelle
      </button>
      <button
        type="button"
        onClick={onDelete}
        className="inline-flex h-7 items-center rounded-full bg-red-700 px-2 text-[11px] font-semibold text-white shadow-sm transition hover:bg-red-800"
      >
        Sil
      </button>
    </div>
  )
}

export function ReservationGrid({
  dateKey,
  units,
  reservations,
  isAdmin,
  onAdd,
  onEdit,
  onDelete,
  onView,
}: ReservationGridProps) {
  if (units.length === 0) {
    return (
      <p className="rounded-xl border border-dashed border-[#b8c6d8] bg-white/70 px-4 py-8 text-center text-sm text-[var(--iglo-muted)]">
        Henüz iglo birimi yok. Üstteki panelden Admin olarak birim ekleyin.
      </p>
    )
  }

  return (
    <>
      <div className="hidden overflow-x-auto rounded-xl border border-[#9eb0c7] shadow-sm md:block">
        <table className="w-full table-fixed border-collapse text-left text-sm">
          <thead>
            <tr className="bg-[var(--iglo-header)] text-white">
              <th className="sticky left-0 z-10 w-[6.25rem] border border-[#6f849c] bg-[var(--iglo-header)] px-1.5 py-1.5 text-xs font-semibold">
                Saat
              </th>
              {units.map((u) => (
                <th
                  key={u.id}
                  className="border border-[#6f849c] px-1 py-1.5 text-center text-[11px] font-semibold leading-tight"
                  title={u.name}
                >
                  <span className="line-clamp-2 break-words">{u.name}</span>
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {DAY_SLOTS.map((slot, rowIndex) => (
              <tr key={slot.id} style={{ background: rowIndex % 2 === 0 ? 'var(--iglo-row-a)' : 'var(--iglo-row-b)' }}>
                <th className="sticky left-0 z-10 border border-[#9eb0c7] bg-inherit px-1.5 py-1.5 text-[11px] font-bold tracking-wide text-[var(--iglo-navy)] whitespace-nowrap">
                  {slot.label}
                </th>
                {units.map((unit) => {
                  const reservation = findForCell(reservations, unit.id, dateKey, slot)
                  return (
                    <td key={`${slot.id}-${unit.id}`} className={`border border-[#9eb0c7] p-1.5 align-top ${statusTone(reservation?.status)}`}>
                      <div className={`flex min-h-[4.75rem] flex-col gap-1.5 ${reservation ? '' : 'items-center justify-center'}`}>
                        {reservation ? (
                          <>
                            <p className="line-clamp-3 flex-1 text-[12px] font-semibold leading-snug text-[var(--iglo-ink)]">
                              {reservation.description?.trim() || statusLabel(reservation.status)}
                            </p>
                            <StatusBadge status={reservation.status} />
                          </>
                        ) : null}
                        <CellActions
                          isAdmin={isAdmin}
                          reservation={reservation}
                          onAdd={() => onAdd(unit, slot)}
                          onEdit={() => reservation && onEdit(unit, slot, reservation)}
                          onDelete={() => reservation && onDelete(reservation)}
                          onView={() => reservation && onView(unit, slot, reservation)}
                        />
                      </div>
                    </td>
                  )
                })}
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      <div className="space-y-4 md:hidden">
        {DAY_SLOTS.map((slot, rowIndex) => (
          <section
            key={slot.id}
            className="overflow-hidden rounded-xl border border-[#9eb0c7]"
            style={{ background: rowIndex % 2 === 0 ? 'var(--iglo-row-a)' : 'var(--iglo-row-b)' }}
          >
            <h3 className="bg-[var(--iglo-header)] px-3 py-2 text-sm font-semibold text-white">
              {slot.label}
            </h3>
            <ul className="divide-y divide-[#9eb0c7]">
              {units.map((unit) => {
                const reservation = findForCell(reservations, unit.id, dateKey, slot)
                return (
                  <li key={`${slot.id}-${unit.id}`} className={`px-3 py-3 ${statusTone(reservation?.status)}`}>
                    <div className="flex flex-col gap-2">
                      <span className="text-xs font-bold text-[var(--iglo-navy)]">{unit.name}</span>
                      <span className="text-[16px] font-semibold leading-relaxed text-[var(--iglo-ink)]">
                        {reservation?.description?.trim()
                          || (reservation ? statusLabel(reservation.status) : 'Boş')}
                      </span>
                      {reservation ? <StatusBadge status={reservation.status} /> : null}
                      <CellActions
                        isAdmin={isAdmin}
                        reservation={reservation}
                        onAdd={() => onAdd(unit, slot)}
                        onEdit={() => reservation && onEdit(unit, slot, reservation)}
                        onDelete={() => reservation && onDelete(reservation)}
                        onView={() => reservation && onView(unit, slot, reservation)}
                      />
                    </div>
                  </li>
                )
              })}
            </ul>
          </section>
        ))}
      </div>
    </>
  )
}
