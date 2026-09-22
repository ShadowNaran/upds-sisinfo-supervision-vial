import { AxiosError } from 'axios'

// traduce un error de peticion a un mensaje entendible para el usuario
// hu-01 tarea 9 401 sin conexion servidor caido errores de validacion
export function mensajeDeError(error: unknown, sinConexion = false): string {
  if (sinConexion) {
    return 'Sin conexion. Revisa tu senal e intentalo de nuevo.'
  }

  if (error instanceof AxiosError) {
    if (error.code === 'ERR_NETWORK' || error.code === 'ECONNABORTED') {
      return 'No se pudo conectar al servidor. Verifica tu conexion e intentalo de nuevo.'
    }

    const status = error.response?.status

    if (status === 401) {
      return 'Usuario o contrasena incorrectos.'
    }

    if (status === 400) {
      const datos = error.response?.data as { errors?: Record<string, string[]> } | undefined
      if (datos?.errors) {
        return Object.values(datos.errors)
          .flat()
          .join(' ')
      }
      return 'Revisa los campos marcados e intentalo de nuevo.'
    }

    if (status && status >= 500) {
      return 'El servidor no pudo procesar la solicitud. Intenta de nuevo en unos minutos.'
    }
  }

  return 'Ocurrio un error inesperado. Intentalo de nuevo.'
}