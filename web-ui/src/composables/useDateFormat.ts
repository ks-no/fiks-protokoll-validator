const norwegianDateTimeFormatter = new Intl.DateTimeFormat('nb-NO', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
  second: '2-digit'
})

const norwegianDateFormatter = new Intl.DateTimeFormat('nb-NO', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric'
})

const norwegianTimeFormatter = new Intl.DateTimeFormat('nb-NO', {
  hour: '2-digit',
  minute: '2-digit',
  second: '2-digit'
})

export function useDateFormat() {
  function formatDateTime(date: string | Date | undefined): string {
    if (!date) return ''
    const d = typeof date === 'string' ? new Date(date) : date
    if (isNaN(d.getTime())) return ''

    // Format: DD.MM.YYYY kl.HH:mm:ss.SSS
    const parts = norwegianDateTimeFormatter.formatToParts(d)
    const day = parts.find(p => p.type === 'day')?.value ?? ''
    const month = parts.find(p => p.type === 'month')?.value ?? ''
    const year = parts.find(p => p.type === 'year')?.value ?? ''
    const hour = parts.find(p => p.type === 'hour')?.value ?? ''
    const minute = parts.find(p => p.type === 'minute')?.value ?? ''
    const second = parts.find(p => p.type === 'second')?.value ?? ''
    const ms = d.getMilliseconds().toString().padStart(3, '0')

    return `${day}.${month}.${year} kl.${hour}:${minute}:${second}.${ms}`
  }

  function formatDate(date: string | Date | undefined): string {
    if (!date) return ''
    const d = typeof date === 'string' ? new Date(date) : date
    if (isNaN(d.getTime())) return ''
    return norwegianDateFormatter.format(d)
  }

  function formatTime(date: string | Date | undefined): string {
    if (!date) return ''
    const d = typeof date === 'string' ? new Date(date) : date
    if (isNaN(d.getTime())) return ''
    return norwegianTimeFormatter.format(d)
  }

  function formatRelative(date: string | Date | undefined): string {
    if (!date) return ''
    const d = typeof date === 'string' ? new Date(date) : date
    if (isNaN(d.getTime())) return ''

    const now = new Date()
    const diff = now.getTime() - d.getTime()
    const seconds = Math.floor(diff / 1000)
    const minutes = Math.floor(seconds / 60)
    const hours = Math.floor(minutes / 60)
    const days = Math.floor(hours / 24)

    if (days > 7) return formatDate(d)
    if (days > 1) return `${days} dager siden`
    if (days === 1) return 'i går'
    if (hours > 1) return `${hours} timer siden`
    if (hours === 1) return '1 time siden'
    if (minutes > 1) return `${minutes} minutter siden`
    if (minutes === 1) return '1 minutt siden'
    return 'akkurat nå'
  }

  return {
    formatDateTime,
    formatDate,
    formatTime,
    formatRelative
  }
}
