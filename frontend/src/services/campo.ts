import { api } from '@/services/api'
import type { Planilla, Tramo, Personal } from '@/types/campo'

export interface PlanillaResumen {
  id: string
  fecha: string
  idTramo: string
  tramo: string
  estado: string
  personal: number
  presentes: number
  cerradaEn: string | null
}

export interface PersonalResumen {
  id: string
  nombreCompleto: string
  documento: string
  cargo?: string
  tramo: string
  idTramo: string
  estadoValidacion: 'Aprobado' | 'PendienteValidacion' | 'Rechazado'
}

function normalizarSeveridad(valor: unknown): 0 | 1 | 2 | undefined {
  if (valor === 0 || valor === 1 || valor === 2) return valor
  if (valor === '0' || valor === 'TransitableNormal') return 0
  if (valor === '1' || valor === 'TransitableConPrecaucion') return 1
  if (valor === '2' || valor === 'NoTransitable') return 2
  return undefined
}

function normalizarPlanilla(planilla: Planilla): Planilla {
  return {
    ...planilla,
    detalles: planilla.detalles.map((detalle) => ({
      ...detalle,
      severidad: normalizarSeveridad(detalle.severidad),
    })),
  }
}

export async function listarPlanillas(): Promise<PlanillaResumen[]> {
  try {
    const { data } = await api.get<PlanillaResumen[]>('/api/planillas')
    guardar('aroomaf.planillas_resumen', data)
    return data
  } catch {
    return leer<PlanillaResumen[]>('aroomaf.planillas_resumen') ?? []
  }
}

export async function listarTodoPersonal(): Promise<PersonalResumen[]> {
  try {
    const { data } = await api.get<PersonalResumen[]>('/api/personal')
    guardar('aroomaf.personal_resumen', data)
    return data
  } catch {
    return leer<PersonalResumen[]>('aroomaf.personal_resumen') ?? []
  }
}

export async function validarPersonal(id: string, estado: 'Aprobado' | 'Rechazado'): Promise<void> {
  await api.patch(`/api/personal/${id}/validar`, { estado })
}

export async function obtenerPlanilla(id: string): Promise<Planilla> {
  const { data } = await api.get<Planilla>(`/api/planillas/${id}`)
  return normalizarPlanilla(data)
}

const CLAVE_TRAMOS = 'aroomaf.catalogo.tramos'
const clavePersonal = (idTramo: string) => `aroomaf.catalogo.personal.${idTramo}`
const clavePlanilla = (id: string) => `aroomaf.planilla.${id}`

function leer<T>(clave: string): T | null {
  try { const valor = localStorage.getItem(clave); return valor ? JSON.parse(valor) as T : null } catch { return null }
}

function guardar<T>(clave: string, valor: T): void {
  try { localStorage.setItem(clave, JSON.stringify(valor)) } catch { /* almacenamiento lleno */ }
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

export async function crearPersonalManual(datos: { nombreCompleto: string; documento: string; cargo: string; telefono: string; idTramo: string }): Promise<Personal> {
  const { data } = await api.post<Personal>('/api/personal', datos)
  return data
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
  const payload = {
    observaciones: planilla.observaciones,
    timestampLocal: planilla.timestampLocal,
    detalles: planilla.detalles.map((detalle) => ({ idDetalle: detalle.id, estado: detalle.estado, observacion: detalle.observacion, clasificacion: detalle.clasificacion, fotoBase64: detalle.fotoBase64, latitud: detalle.latitud, longitud: detalle.longitud, kilometraje: detalle.kilometraje, severidad: normalizarSeveridad(detalle.severidad) })),
  }
  const { data } = await api.put<Planilla>(`/api/planillas/${planilla.id}`, payload)
  return normalizarPlanilla(data)
}

export async function firmarPlanilla(idPlanilla: string, firmaBase64: string): Promise<void> {
  await api.post(`/api/planillas/${idPlanilla}/firmar`, { firmaBase64 })
}

export async function registrarMitigacion(idDetalle: string, accionMitigacion: string, esFalsoPositivo: boolean = false): Promise<void> {
  await api.post(`/api/planillas/detalle/${idDetalle}/mitigacion`, { accionMitigacion, esFalsoPositivo })
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