import { ref, type Ref } from 'vue'

interface ApiError {
  status: number
  message: string
  data?: unknown
}

interface RequestOptions {
  responseType?: 'json' | 'text' | 'blob'
}

interface UseApiReturn<T> {
  data: Ref<T | null>
  loading: Ref<boolean>
  error: Ref<ApiError | null>
  get: (endpoint: string, options?: RequestOptions) => Promise<T>
  post: <R = T>(endpoint: string, payload: unknown, options?: RequestOptions) => Promise<R>
}

export function useApi<T = unknown>(): UseApiReturn<T> {
  const baseUrl = import.meta.env.VITE_API_URL || ''
  const data = ref<T | null>(null) as Ref<T | null>
  const loading = ref(false)
  const error = ref<ApiError | null>(null)

  async function handleResponse<R>(response: Response, responseType: string = 'json'): Promise<R> {
    if (!response.ok) {
      let errorData: unknown
      try {
        errorData = await response.json()
      } catch {
        errorData = await response.text()
      }
      const apiError: ApiError = {
        status: response.status,
        message: response.statusText,
        data: errorData
      }
      error.value = apiError
      throw apiError
    }

    if (response.status === 204 || response.status === 205) {
      return null as R
    }

    if (responseType === 'text') return await response.text() as R
    if (responseType === 'blob') return await response.blob() as R

    const text = await response.text()
    if (!text) return null as R
    return JSON.parse(text) as R
  }

  async function get(endpoint: string, options?: RequestOptions): Promise<T> {
    loading.value = true
    error.value = null
    data.value = null

    try {
      const response = await fetch(`${baseUrl}${endpoint}`, {
        credentials: 'include'
      })
      const result = await handleResponse<T>(response, options?.responseType)
      data.value = result
      return result
    } catch (err) {
      data.value = null
      if ((err as ApiError).status) {
        throw err
      }
      error.value = { status: 0, message: (err as Error).message || 'Unknown error' }
      throw error.value
    } finally {
      loading.value = false
    }
  }

  async function post<R = T>(endpoint: string, payload: unknown, options?: RequestOptions): Promise<R> {
    loading.value = true
    error.value = null

    const isFormData = payload instanceof FormData
    const headers: Record<string, string> = {}
    if (!isFormData) {
      headers['Content-Type'] = 'application/json'
    }

    try {
      const response = await fetch(`${baseUrl}${endpoint}`, {
        method: 'POST',
        credentials: 'include',
        headers,
        body: isFormData ? payload : JSON.stringify(payload)
      })
      return await handleResponse<R>(response, options?.responseType)
    } catch (err) {
      if ((err as ApiError).status) {
        throw err
      }
      error.value = { status: 0, message: (err as Error).message || 'Unknown error' }
      throw error.value
    } finally {
      loading.value = false
    }
  }

  return {
    data,
    loading,
    error,
    get,
    post
  }
}
