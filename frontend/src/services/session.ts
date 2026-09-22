import type { Perfil, SesionLocal } from '@/types/auth'
import { guardarCifrado, leerCifrado, eliminarCifrado, CLAVE_SESION, CLAVE_TOKEN } from './secureStore'

// tolerancia de reloj 30 s de margen antes de considerar vencida la sesion
const MARGEN_EXPIRACION_MS = 30_000

export function sesionVigente(sesion: SesionLocal | null, ahora = Date.now()): boolean {
  if (!sesion?.perfil?.expiresAt) return false
  const expira = new Date(sesion.perfil.expiresAt).getTime()
  return expira - MARGEN_EXPIRACION_MS > ahora
}

// persiste el perfil autenticado en localStorage entrada sin conexion hu-01
export async function salvarSesion(perfil: Perfil): Promise<void> {
  const sesion: SesionLocal = { perfil, guardadoEn: Date.now() }
  await guardarCifrado(CLAVE_SESION, sesion)
  await guardarCifrado(CLAVE_TOKEN, perfil.token)
}

export async function cargarSesion(): Promise<SesionLocal | null> {
  const sesion = await leerCifrado<SesionLocal>(CLAVE_SESION)
  return sesion
}

export async function borrarSesionLocal(): Promise<void> {
  eliminarCifrado(CLAVE_SESION)
  eliminarCifrado(CLAVE_TOKEN)
}