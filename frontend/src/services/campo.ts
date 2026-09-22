import { api } from '@/services/api'
import type { Personal, Planilla, Tramo } from '@/types/campo'

const CLAVE_TRAMOS = 'aroomaf.catalogo.tramos'
const clavePersonal = (idTramo: string) => `aroomaf.catalogo.personal.${idTramo}`
const clavePlanilla = (id: string) => `aroomaf.planilla.${id}`

function leer<T>(clave: string): T | null {
  try { const valor = localStorage.getItem(clave); return valor ? JSON.parse(valor) as T : null } catch { return null }
}

function guardar<T>(clave: string, valor: T): void {
  try { localStorage.setItem(clave, JSON.stringify(valor)) } catch { /* cuota local agotada */ }
}

export async function listarTramos(): Promise<Tramo[]> {
  try {
    const { data } = await api.get<Tramo[]>('/api/tramos?activo=true')
    guardar(CLAVE_TRAMOS, data)
    return data
  } catch {
    return leer<Tramo[]>(CLAVE_TRAMOS) ?? []
  }
}

export async function listarPersonal(idTramo: string): Promise<Personal[]> {
  try {
    const { data } = await api.get<Personal[]>(`/api/personal?tramoId=${idTramo}`)
    guardar(clavePersonal(idTramo), data)
    return data
  } catch {
    return leer<Personal[]>(clavePersonal(idTramo)) ?? []
  }
}

export async function crearPlanilla(idTramo: string): Promise<Planilla> {
  const { data } = await api.post<Planilla>('/api/planillas', { idTramo, fecha: new Date().toISOString().slice(0, 10) })
  return data
}

export async function actualizarPlanilla(planilla: Planilla): Promise<Planilla> {
  const { data } = await api.put<Planilla>(`/api/planillas/${planilla.id}`, {
    observaciones: planilla.observaciones,
    detalles: planilla.detalles.map((detalle) => ({ idDetalle: detalle.id, estado: detalle.estado, observacion: detalle.observacion, clasificacion: detalle.clasificacion, fotoBase64: detalle.fotoBase64 })),
  })
  return data
}

export async function firmarPlanilla(id: string, firmaBase64: string): Promise<void> {
  await api.post(`/api/planillas/${id}/firma`, { firmaBase64 })
}

export async function cerrarPlanilla(id: string): Promise<Planilla> {
  const { data } = await api.post<Planilla>(`/api/planillas/${id}/cerrar`)
  return data
}

export function crearPlanillaLocal(idTramo: string, personas: Personal[]): Planilla {
  const id = `local-${crypto.randomUUID()}`
  const planilla: Planilla = {
    id, fecha: new Date().toISOString().slice(0, 10), idTramo, tramo: 'Tramo almacenado localmente', estado: 'Borrador', tieneFirma: false,
    detalles: personas.map((persona) => ({ id: `local-${crypto.randomUUID()}`, idPersonal: persona.id, personal: persona.nombreCompleto, estado: 'NoDisponible' })),
  }
  guardar(clavePlanilla(id), planilla)
  guardar(`aroomaf.planilla.ultima.${idTramo}`, planilla)
  return planilla
}

export function guardarPlanillaLocal(planilla: Planilla): void {
  guardar(clavePlanilla(planilla.id), planilla)
  guardar(`aroomaf.planilla.ultima.${planilla.idTramo}`, planilla)
}
export function cargarPlanillaLocal(id: string): Planilla | null { return leer<Planilla>(clavePlanilla(id)) }
export function cargarUltimaPlanillaLocal(idTramo: string): Planilla | null { return leer<Planilla>(`aroomaf.planilla.ultima.${idTramo}`) }