// cola local para sincronizar datos pendientes

const CLAVE_COLA = 'aroomaf.cola-sync'

export type EstadoSync = 'pendiente' | 'enviando' | 'sincronizado' | 'error'

export interface ItemCola {
  id: string
  tipo: 'planilla' | 'evento' | 'firma' | 'foto' | 'personal_manual'
  payload: unknown
  estado: EstadoSync
  prioridad: 'alta' | 'normal'
  creadoEn: number
  reintentos: number
  error?: string
}

// lee la cola local
function obtenerColaStorage(): ItemCola[] {
  const bruto = localStorage.getItem(CLAVE_COLA)
  if (!bruto) return []
  try {
    return JSON.parse(bruto) as ItemCola[]
  } catch {
    return []
  }
}

// guarda la cola local
function guardarColaStorage(cola: ItemCola[]): void {
  try {
    localStorage.setItem(CLAVE_COLA, JSON.stringify(cola))
  } catch {
    // ignora errores de almacenamiento lleno
  }
}

// agrega o actualiza un elemento
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

// filtra elementos por estado
export async function obtenerCola(estado?: EstadoSync): Promise<ItemCola[]> {
  const cola = obtenerColaStorage()
  return estado ? cola.filter(i => i.estado === estado) : cola
}

// ordena por prioridad y antiguedad
export async function obtenerColaOrdenada(): Promise<ItemCola[]> {
  const items = await obtenerCola()
  return items
    .filter((i) => i.estado !== 'sincronizado')
    .sort((a, b) => {
      if (a.prioridad !== b.prioridad) return a.prioridad === 'alta' ? -1 : 1
      return a.creadoEn - b.creadoEn
    })
}

// actualiza el estado de un elemento
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

export async function reintentarItem(id: string): Promise<void> {
  const cola = obtenerColaStorage()
  const item = cola.find(i => i.id === id)
  if (item) {
    item.estado = 'pendiente'
    item.reintentos = 0
    item.error = undefined
    guardarColaStorage(cola)
  }
}

// cuenta elementos pendientes y fallidos
export async function contarPendientes(): Promise<number> {
  const pendientes = await obtenerCola('pendiente')
  const errores = await obtenerCola('error')
  return pendientes.length + errores.length
}

// genera un id unico
export function generarId(): string {
  return `${Date.now()}-${Math.random().toString(36).slice(2, 9)}`
}