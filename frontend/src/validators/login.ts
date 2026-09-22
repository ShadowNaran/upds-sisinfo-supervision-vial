import type { LoginCredenciales } from '@/types/auth'

export interface ErroresLogin {
  username?: string
  password?: string
}

// validacion de campos en el cliente hu-01 tarea 3
// reglas espejo del backend obligatorio longitud minima
export function validarLogin({
  username,
  password,
}: LoginCredenciales): ErroresLogin {
  const errores: ErroresLogin = {}

  const usuarioLimpio = username.trim()

  if (!usuarioLimpio) {
    errores.username = 'El usuario es obligatorio.'
  } else if (usuarioLimpio.length < 3) {
    errores.username = 'El usuario debe tener al menos 3 caracteres.'
  }

  if (!password) {
    errores.password = 'La contrasena es obligatoria.'
  } else if (password.length < 6) {
    errores.password = 'La contrasena debe tener al menos 6 caracteres.'
  }

  return errores
}

export function tieneErrores(errores: ErroresLogin): boolean {
  return Object.keys(errores).length > 0
}