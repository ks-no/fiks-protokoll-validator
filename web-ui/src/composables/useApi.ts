import { ref, type Ref } from 'vue'
import axios, { type AxiosError, type AxiosRequestConfig } from 'axios'

interface ApiError {
  status: number
  message: string
  data?: unknown
}

interface UseApiReturn<T> {
  data: Ref<T | null>
  loading: Ref<boolean>
  error: Ref<ApiError | null>
  get: (endpoint: string, config?: AxiosRequestConfig) => Promise<T>
  post: <R = T>(endpoint: string, payload: unknown, config?: AxiosRequestConfig) => Promise<R>
}

export function useApi<T = unknown>(): UseApiReturn<T> {
  const baseUrl = import.meta.env.VITE_API_URL || ''
  const data = ref<T | null>(null) as Ref<T | null>
  const loading = ref(false)
  const error = ref<ApiError | null>(null)

  async function get(endpoint: string, config?: AxiosRequestConfig): Promise<T> {
    loading.value = true
    error.value = null

    try {
      const response = await axios.get<T>(`${baseUrl}${endpoint}`, {
        withCredentials: true,
        ...config
      })
      data.value = response.data
      return response.data
    } catch (err) {
      const axiosError = err as AxiosError
      error.value = {
        status: axiosError.response?.status ?? 0,
        message: axiosError.message,
        data: axiosError.response?.data
      }
      throw error.value
    } finally {
      loading.value = false
    }
  }

  async function post<R = T>(endpoint: string, payload: unknown, config?: AxiosRequestConfig): Promise<R> {
    loading.value = true
    error.value = null

    try {
      const response = await axios.post<R>(`${baseUrl}${endpoint}`, payload, {
        withCredentials: true,
        ...config
      })
      return response.data
    } catch (err) {
      const axiosError = err as AxiosError
      error.value = {
        status: axiosError.response?.status ?? 0,
        message: axiosError.message,
        data: axiosError.response?.data
      }
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
