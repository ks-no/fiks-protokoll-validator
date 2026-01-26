import { ref, watch, type Ref } from 'vue'

export function useLocalStorage<T>(key: string, defaultValue: T): Ref<T> {
  const storedValue = localStorage.getItem(key)
  const initial = storedValue ? (JSON.parse(storedValue) as T) : defaultValue

  const value = ref<T>(initial) as Ref<T>

  watch(
    value,
    (newValue) => {
      if (newValue === null || newValue === undefined) {
        localStorage.removeItem(key)
      } else {
        localStorage.setItem(key, JSON.stringify(newValue))
      }
    },
    { deep: true }
  )

  return value
}

export function useLocalStorageString(key: string, defaultValue: string = ''): Ref<string> {
  const storedValue = localStorage.getItem(key)
  const initial = storedValue ?? defaultValue

  const value = ref<string>(initial)

  watch(value, (newValue) => {
    if (newValue === null || newValue === undefined || newValue === '') {
      localStorage.removeItem(key)
    } else {
      localStorage.setItem(key, newValue)
    }
  })

  return value
}
