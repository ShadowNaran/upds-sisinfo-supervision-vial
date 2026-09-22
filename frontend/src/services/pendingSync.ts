// cola de sincronizacion offline simple hu-04 hu-17 hu-18
// usa localStorage en lugar de IndexedDB para simplicidad

const CLAVE_COLA = 'aroomaf.cola-sync'

export type EstadoSync = 'pendiente' | 'enviando' | 'sincronizado' | 'error'

export interface ItemCola {
  id: string
  tipo: 'planilla' | 'evento' | 'firma' | 'foto'
  payload: unknown
  estado: EstadoSync
  prioridad: 'alta' | 'normal'
  creadoEn: number
  reintentos: number
  error?: string
}

// obtiene la cola completa de localStorage
function obtenerColaStorage(): ItemCola[] {
  const bruto = localStorage.getItem(CLAVE_COLA)
  if (!bruto) return []
  try {
    return JSON.parse(bruto) as ItemCola[]
  } catch {
    return []
  }
}

// guarda la cola completa en localStorage
function guardarColaStorage(cola: ItemCola[]): void {
  try {
    localStorage.setItem(CLAVE_COLA, JSON.stringify(cola))
  } catch {
    // Ignorar errores de cuota
  }
}

// guarda o actualiza un item en la cola
export async function guardarEnCola(item: ItemCola): Promise<void> {
  const cola = obtenerColaStorage()
  const idx = cola.findIndex(i => i.id === item.id)
  if (idx >= 0) {
    cola[idx] = item
  } else {
    cola.push(item)
  }
  guardarColaStorage(cola)
}

// obtiene items por estado
export async function obtenerCola(estado?: EstadoSync): Promise<ItemCola[]> {
  const cola = obtenerColaStorage()
  return estado ? cola.filter(i => i.estado === estado) : cola
}

// obtiene items ordenados por prioridad alta primero y antiguedad
export async function obtenerColaOrdenada(): Promise<ItemCola[]> {
  const items = await obtenerCola()
  return items
    .filter((i) => i.estado !== 'sincronizado')
    .sort((a, b) => {
      if (a.prioridad !== b.prioridad) return a.prioridad === 'alta' ? -1 : 1
      return a.creadoEn - b.creadoEn
    })
}

// actualiza el estado de un item
export async function actualizarEstadoCola(id: string, estado: EstadoSync, error?: string): Promise<void> {
  const cola = obtenerColaStorage()
  const item = cola.find(i => i.id === id)
  if (item) {
    item.estado = estado
    if (error) item.error = error
    if (estado === 'enviando') item.reintentos++
    guardarColaStorage(cola)
  }
}

// elimina items sincronizados antiguos limpieza
export async function limpiarSincronizados(antesDe = Date.now() - 7 * 86_400_000): Promise<number> {
  const cola = obtenerColaStorage()
  const inicial = cola.length
  const filtrada = cola.filter(item => !(item.estado === 'sincronizado' && item.creadoEn < antesDe))
  guardarColaStorage(filtrada)
  return inicial - filtrada.length
}

// cuenta items pendientes para badge hu-05
export async function contarPendientes(): Promise<number> {
  const pendientes = await obtenerCola('pendiente')
  const errores = await obtenerCola('error')
  return pendientes.length + errores.length
}

// genera id unico simple
export function generarId(): string {
  return `${Date.now()}-${Math.random().toString(36).slice(2, 9)}`
}

// clave localStorage legacy compatibilidad
export const CLAVE_COLA_LEGACY = CLAVE_COLA