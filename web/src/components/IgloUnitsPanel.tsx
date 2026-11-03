import { useState, type FormEvent } from 'react'
import type { IgloUnit } from '../api/types'

interface IgloUnitsPanelProps {
  units: IgloUnit[]
  busy: boolean
  error: string | null
  onCreate: (name: string) => Promise<void>
  onRename: (unit: IgloUnit, name: string) => Promise<void>
  onDelete: (unit: IgloUnit) => Promise<void>
}

export function IgloUnitsPanel({
  units,
  busy,
  error,
  onCreate,
  onRename,
  onDelete,
}: IgloUnitsPanelProps) {
  const [open, setOpen] = useState(false)
  const [name, setName] = useState('')
  const [editingId, setEditingId] = useState<string | null>(null)
  const [editName, setEditName] = useState('')

  async function submitCreate(e: FormEvent) {
    e.preventDefault()
    const trimmed = name.trim()
    if (!trimmed) return
    await onCreate(trimmed)
    setName('')
  }

  return (
    <section className="mb-2 rounded-xl border border-[#c9d4e3] bg-white/90 shadow-sm">
      <button
        type="button"
        onClick={() => setOpen((v) => !v)}
        className="flex w-full items-center justify-between gap-2 px-3 py-2 text-left"
        aria-expanded={open}
      >
        <span className="text-xs font-bold tracking-wide text-[var(--iglo-navy)] uppercase">
          Iglo birimleri (Admin)
        </span>
        <span className="text-xs text-[var(--iglo-muted)]">
          {units.length} birim · {open ? 'Gizle' : 'Yönet'}
        </span>
      </button>

      {open && (
        <div className="border-t border-[#e2e8f0] px-3 pb-3 pt-2">
          <form className="flex flex-wrap gap-2" onSubmit={(e) => void submitCreate(e)}>
            <input
              type="text"
              required
              maxLength={200}
              value={name}
              onChange={(e) => setName(e.target.value)}
              placeholder="Örn. 9. İGLO veya TERAS-3"
              className="min-w-[12rem] flex-1 rounded-lg border border-[#c9d4e3] bg-[#fbfcfe] px-3 py-1.5 text-sm outline-none focus:border-[var(--iglo-navy)]"
            />
            <button
              type="submit"
              disabled={busy}
              className="rounded-lg bg-[var(--iglo-navy)] px-3 py-1.5 text-sm font-medium text-white hover:bg-[#162c48] disabled:opacity-60"
            >
              Iglo ekle
            </button>
          </form>

          {error && (
            <p className="mt-2 rounded-lg bg-red-50 px-3 py-2 text-sm text-red-700" role="alert">
              {error}
            </p>
          )}

          <ul className="mt-2 max-h-40 divide-y divide-[#e2e8f0] overflow-y-auto rounded-lg border border-[#e2e8f0]">
            {units.length === 0 && (
              <li className="px-3 py-2 text-sm text-[var(--iglo-muted)]">Henüz birim yok.</li>
            )}
            {units.map((unit) => (
              <li key={unit.id} className="flex flex-wrap items-center gap-2 px-3 py-1.5">
                {editingId === unit.id ? (
                  <>
                    <input
                      value={editName}
                      onChange={(e) => setEditName(e.target.value)}
                      className="min-w-[10rem] flex-1 rounded border border-[#c9d4e3] px-2 py-1 text-sm"
                    />
                    <button
                      type="button"
                      disabled={busy}
                      onClick={() => void onRename(unit, editName.trim()).then(() => setEditingId(null))}
                      className="rounded bg-emerald-700 px-2.5 py-1 text-xs font-semibold text-white disabled:opacity-60"
                    >
                      Kaydet
                    </button>
                    <button
                      type="button"
                      onClick={() => setEditingId(null)}
                      className="rounded border border-[#c9d4e3] px-2.5 py-1 text-xs font-semibold"
                    >
                      İptal
                    </button>
                  </>
                ) : (
                  <>
                    <span className="flex-1 text-sm font-medium text-[var(--iglo-ink)]">{unit.name}</span>
                    <button
                      type="button"
                      disabled={busy}
                      onClick={() => {
                        setEditingId(unit.id)
                        setEditName(unit.name)
                      }}
                      className="rounded border border-[#c9d4e3] bg-[#f5f8fb] px-2.5 py-1 text-xs font-semibold hover:bg-[#e8eef6] disabled:opacity-60"
                    >
                      Güncelle
                    </button>
                    <button
                      type="button"
                      disabled={busy}
                      onClick={() => {
                        if (window.confirm(`“${unit.name}” birimini silmek istiyor musunuz?`)) {
                          void onDelete(unit)
                        }
                      }}
                      className="rounded bg-red-700 px-2.5 py-1 text-xs font-semibold text-white hover:bg-red-800 disabled:opacity-60"
                    >
                      Sil
                    </button>
                  </>
                )}
              </li>
            ))}
          </ul>
        </div>
      )}
    </section>
  )
}
