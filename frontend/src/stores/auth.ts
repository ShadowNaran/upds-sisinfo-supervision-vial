import { defineStore } from 'pinia'
import { ref } from 'vue'
import type { EstadoSesion, LoginCredenciales, Perfil, Rol } from '@/types/auth'
import { api } from '@/services/api'
import { mensajeDeError } from '@/services/mensajesError'
import { borrarSesionLocal, cargarSesion, salvarSesion, sesionVigente } from '@/services/session'

const ROL_POR_DEFECTO: Record<Rol, string> = {
  Administrador: '/campo',
  SupervisorCampo: '/campo',
  PersonalMicroempresa: '/campo',
}

export const useAuthStore = defineStore('auth', () => {
  const estado = ref<EstadoSesion>('indefinido')
  const perfil = ref<Perfil | null>(null)

  const estaAutenticado = () => estado.value === 'autenticado'

  // ruta inicial segun el rol del usuario autenticado
  const rutaInicial = (): string =>
    perfil.value ? ROL_POR_DEFECTO[perfil.value.rol] : '/login'

  // restaura la sesion guardada en el dispositivo entrada sin conexion hu-01
  async function init(): Promise<void> {
    if (estado.value !== 'indefinido') return

    const sesion = await cargarSesion()
    if (sesion && sesionVigente(sesion)) {
      perfil.value = sesion.perfil
      estado.value = 'autenticado'
    } else {
      if (sesion && !sesionVigente(sesion)) {
        await borrarSesionLocal()
      }
      perfil.value = null
      estado.value = 'anonimo'
    }
  }

  // inicia sesion contra el servidor hu-01
  async function login(credenciales: LoginCredenciales): Promise<Perfil> {
    const { data } = await api.post<{
      token: string
      rol: Rol
      nombre: string
      username: string
      expiresAt: string
    }>('/api/auth/login', credenciales)

    const nuevoPerfil: Perfil = {
      token: data.token,
      rol: data.rol,
      nombre: data.nombre,
      username: data.username,
      expiresAt: data.expiresAt,
      expiresInSeconds: Math.max(0, Math.round((new Date(data.expiresAt).getTime() - Date.now()) / 1000)),
    }

    await salvarSesion(nuevoPerfil)
    perfil.value = nuevoPerfil
    estado.value = 'autenticado'
    return nuevoPerfil
  }

  // cierra la sesion y limpia los datos locales base de hu-02
  async function logout(): Promise<void> {
    await borrarSesionLocal()
    perfil.value = null
    estado.value = 'anonimo'
  }

  // traduce un error de red http en mensaje amigable para el formulario
  function traducirError(error: unknown): string {
    return mensajeDeError(error, !navigator.onLine)
  }

  return {
    estado,
    perfil,
    estaAutenticado,
    rutaInicial,
    init,
    login,
    logout,
    traducirError,
  }
})