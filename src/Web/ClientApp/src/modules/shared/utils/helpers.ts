export function debounce<F extends (...args: Parameters<F>) => ReturnType<F>>(func: F, waitFor: number) {
  let timeout: ReturnType<typeof setTimeout>

  const debounced = (...args: Parameters<F>) => {
    clearTimeout(timeout)
    timeout = setTimeout(() => func(...args), waitFor)
  }

  return debounced
}

export function getFormattedDateTime({
  date,
  locale = navigator.language,
  formatOptions = {
    month: '2-digit',
    day: '2-digit',
    year: 'numeric'
  }
}: {
  date: Date | string | number // number must be milliseconds
  locale?: string
  formatOptions?: Intl.DateTimeFormatOptions
}) {
  const parsedDate = new Date(date)
  return new Intl.DateTimeFormat(locale, formatOptions).format(parsedDate)
}

export function getFormattedNumber({
  value,
  locale = navigator.language,
  options
}: {
  value: number
  locale?: string
  options?: Intl.NumberFormatOptions
}) {
  return new Intl.NumberFormat(locale, options).format(value)
}

export function getFormattedNumberWithPercent({
  value,
  locale,
  formatOptions
}: {
  value: number
  locale?: string
  formatOptions?: Intl.NumberFormatOptions
}) {
  return getFormattedNumber({
    value,
    locale,
    options: {
      style: 'percent',
      minimumFractionDigits: 0,
      maximumFractionDigits: 2,
      ...formatOptions
    }
  })
}

export function groupBy<T>(items: T[], criteria: Extract<keyof T, string> | ((item: T) => any)) {
  return items.reduce(
    (acc, cur) => {
      const key = typeof criteria === 'string' ? cur[criteria] : criteria(cur)
      if (!acc[key]) {
        acc[key] = []
      }
      acc[key].push(cur)
      return acc
    },
    {} as Record<string, T[]>
  )
}

export function uniqBy<T>(items: T[], criteria: Extract<keyof T, string> | ((item: T) => any)) {
  const mapped: Record<string, T> = {}
  items.forEach((el) => {
    const value = typeof criteria === 'string' ? el[criteria] : criteria(el)
    mapped[value] = { ...el }
  })
  return Object.values(mapped)
}

export function sortedUniqBy<T>(items: T[], criteria: Extract<keyof T, string> | ((item: T) => any)) {
  const result: T[] = []
  const stored = new Map<any, any>()
  for (let i = 0; i < items.length; i++) {
    const computed = typeof criteria === 'string' ? items[i][criteria] : criteria(items[i])
    if (i === 0 || !stored.get(computed)) {
      result.push(items[i])
      stored.set(computed, items[i])
    }
  }
  return result
}

export function isEmptyObject(object: Record<any, any>) {
  return Object.keys(object).length === 0
}

export function uniq<T>(items: T[]) {
  return [...new Set(items)]
}

export function clone<T>(data: T) {
  return JSON.parse(JSON.stringify(data))
}
