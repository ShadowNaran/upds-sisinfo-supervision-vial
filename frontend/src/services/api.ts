import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

export const API_URL =
  import.meta.env.VITE_API_URL ?? 'http://localhost:5094'

export const api = axios.create({
  baseURL: API_URL,
  timeout: 15_000,
})

api.interceptors.request.use((config) => {
  const auth = useAuthStore()
  const token = auth.perfil?.token
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        const auth = useAuthStore()
        await auth.logout()
        window.location.href = '/login'
      } catch {
        window.location.href = '/login'
      }
    }

    return Promise.reject(error)
  },
)