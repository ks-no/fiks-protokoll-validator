const dateTimeFormatter = new Intl.DateTimeFormat('nb-NO', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
  second: '2-digit',
  timeZone: 'Europe/Oslo'
})

const dateFormatter = new Intl.DateTimeFormat('nb-NO', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  timeZone: 'Europe/Oslo'
})

function toDate(date: string | Date): Date | null {
  const d = typeof date === 'string' ? new Date(date) : date
  return isNaN(d.getTime()) ? null : d
}

export function formatDateTime(date: string | Date | undefined): string {
  if (!date) return ''
  const d = toDate(date)
  if (!d) return ''
  const ms = d.getMilliseconds().toString().padStart(3, '0')
  return `${dateTimeFormatter.format(d)}.${ms}`
}

export function formatDate(date: string | Date | undefined): string {
  if (!date) return ''
  const d = toDate(date)
  if (!d) return ''
  return dateFormatter.format(d)
}
