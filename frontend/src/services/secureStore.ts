// sesion local sin cifrar


// guarda un valor local
export async function guardarCifrado<T>(clave: string, valor: T): Promise<void> {
  try {
    localStorage.setItem(clave, JSON.stringify(valor))
  } catch {
    // ignora errores de almacenamiento lleno
  }
}

// lee un valor local
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

// elimina un valor local
export function eliminarCifrado(clave: string): void {
  localStorage.removeItem(clave)
}

export const CLAVE_SESION = 'aroomaf.sesion'
export const CLAVE_TOKEN = 'aroomaf.token'