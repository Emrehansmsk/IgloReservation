interface DateNavProps {
  dateKey: string
  label: string
  onChange: (dateKey: string) => void
  onPrev: () => void
  onNext: () => void
  onToday: () => void
  onRefresh: () => void
}

export function DateNav({ dateKey, label, onChange, onPrev, onNext, onToday, onRefresh }: DateNavProps) {
  return (
    <nav
      aria-label="Gün seçimi"
      className="flex w-full flex-wrap items-center gap-2 rounded-xl border border-[#b9c8da] bg-white px-3 py-2 shadow-sm"
    >
      <p className="min-w-0 flex-1 font-display text-sm font-semibold tracking-tight text-[var(--iglo-navy)] sm:text-base">
        <span className="mr-1.5 text-[10px] font-semibold tracking-[0.12em] text-[var(--iglo-muted)] uppercase">
          Gün
        </span>
        {label}
      </p>

      <div className="flex flex-wrap items-center gap-1.5" role="group" aria-label="Gün gezinme">
        <button
          type="button"
          onClick={onPrev}
          className="inline-flex h-9 min-w-9 items-center justify-center rounded-lg border border-[#b8c6d8] bg-[#f5f8fb] text-sm font-semibold text-[var(--iglo-navy)] transition hover:bg-white"
          aria-label="Önceki gün"
          title="Önceki gün"
        >
          ←
        </button>
        <button
          type="button"
          onClick={onToday}
          className="inline-flex h-9 items-center justify-center rounded-lg bg-[var(--iglo-navy)] px-3 text-xs font-semibold text-white transition hover:bg-[#162c48]"
        >
          Bugün
        </button>
        <button
          type="button"
          onClick={onNext}
          className="inline-flex h-9 min-w-9 items-center justify-center rounded-lg border border-[#b8c6d8] bg-[#f5f8fb] text-sm font-semibold text-[var(--iglo-navy)] transition hover:bg-white"
          aria-label="Sonraki gün"
          title="Sonraki gün"
        >
          →
        </button>

        <input
          type="date"
          value={dateKey}
          onChange={(e) => e.target.value && onChange(e.target.value)}
          className="h-9 rounded-lg border border-[#b8c6d8] bg-[#fbfcfe] px-2 text-sm font-medium text-[var(--iglo-ink)] outline-none focus:border-[var(--iglo-navy)]"
          aria-label="Takvimden tarih seç"
        />

        <button
          type="button"
          onClick={onRefresh}
          className="inline-flex h-9 items-center justify-center gap-1.5 rounded-lg border border-[#b8c6d8] bg-[#f5f8fb] px-3 text-xs font-semibold text-[var(--iglo-navy)] transition hover:bg-white"
          title="Tabloyu yenile"
          aria-label="Tabloyu yenile"
        >
          <svg aria-hidden="true" viewBox="0 0 20 20" className="h-3.5 w-3.5 fill-current">
            <path d="M10 3a7 7 0 0 0-6.53 4.5 1 1 0 1 1-1.86-.7A9 9 0 0 1 19 10h1.5a.75.75 0 0 1 .53 1.28l-2.5 2.5a.75.75 0 0 1-1.06 0l-2.5-2.5A.75.75 0 0 1 15.5 10H17a7 7 0 0 0-7-7Z" />
            <path d="M10 17a7 7 0 0 0 6.53-4.5 1 1 0 1 1 1.86.7A9 9 0 0 1 1 10H-.5a.75.75 0 0 1-.53-1.28l2.5-2.5a.75.75 0 0 1 1.06 0l2.5 2.5A.75.75 0 0 1 4.5 10H3a7 7 0 0 0 7 7Z" />
          </svg>
          Yenile
        </button>
      </div>
    </nav>
  )
}
