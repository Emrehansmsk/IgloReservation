/** Turkey uses permanent UTC+3 (no DST). */
export const ISTANBUL_OFFSET = '+03:00'
export const TZ = 'Europe/Istanbul'

export interface TimeSlot {
  id: string
  label: string
  startHour: number
  endHour: number
}

export const DAY_SLOTS: TimeSlot[] = [
  { id: 'morning', label: '09:00 - 12:00', startHour: 9, endHour: 12 },
  { id: 'afternoon', label: '13:00 - 16:00', startHour: 13, endHour: 16 },
  { id: 'evening', label: '16:00 - 19:00', startHour: 16, endHour: 19 },
  { id: 'night', label: '19:00 - 22:00', startHour: 19, endHour: 22 },
]

function pad(n: number): string {
  return String(n).padStart(2, '0')
}

/** Calendar day key YYYY-MM-DD in Europe/Istanbul. */
export function toIstanbulDateKey(date: Date): string {
  return new Intl.DateTimeFormat('en-CA', {
    timeZone: TZ,
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
  }).format(date)
}

export function parseDateKey(dateKey: string): Date {
  const [y, m, d] = dateKey.split('-').map(Number)
  return new Date(Date.UTC(y, m - 1, d, 12, 0, 0))
}

export function addDaysToKey(dateKey: string, days: number): string {
  const dt = parseDateKey(dateKey)
  dt.setUTCDate(dt.getUTCDate() + days)
  return toIstanbulDateKey(dt)
}

export function formatTurkishLongDate(dateKey: string): string {
  const dt = parseDateKey(dateKey)
  const formatted = new Intl.DateTimeFormat('tr-TR', {
    timeZone: TZ,
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  }).format(dt)
  return formatted.toLocaleUpperCase('tr-TR')
}

export function slotRangeUtc(dateKey: string, slot: TimeSlot): { startAtUtc: string; endAtUtc: string } {
  const startLocal = `${dateKey}T${pad(slot.startHour)}:00:00${ISTANBUL_OFFSET}`
  const endLocal = `${dateKey}T${pad(slot.endHour)}:00:00${ISTANBUL_OFFSET}`
  return {
    startAtUtc: new Date(startLocal).toISOString(),
    endAtUtc: new Date(endLocal).toISOString(),
  }
}

export function rangesOverlap(aStart: string, aEnd: string, bStart: string, bEnd: string): boolean {
  return new Date(aStart) < new Date(bEnd) && new Date(aEnd) > new Date(bStart)
}

export function todayIstanbulKey(): string {
  return toIstanbulDateKey(new Date())
}
