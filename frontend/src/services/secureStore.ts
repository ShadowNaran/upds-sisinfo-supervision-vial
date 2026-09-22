// almacenamiento local simple sin cifrado para datos de sesion
// nivel estudiante 2do año localStorage plano

// guarda un valor en localStorage como JSON
export async function guardarCifrado<T>(clave: string, valor: T): Promise<void> {
  try {
    localStorage.setItem(clave, JSON.stringify(valor))
  } catch {
    // Ignorar errores de cuota
  }
}

// lee un valor de localStorage y lo parsea como JSON
export async function leerCifrado<T>(clave: string): Promise<T | null> {
  const bruto = localStorage.getItem(clave)
  if (!bruto) return null

  try {
    return JSON.parse(bruto) as T
  } catch {
    localStorage.removeItem(clave)
    return null
  }
}

// elimina un valor de localStorage
export function eliminarCifrado(clave: string): void {
  localStorage.removeItem(clave)
}

export const CLAVE_SESION = 'aroomaf.sesion'
export const CLAVE_TOKEN = 'aroomaf.token'