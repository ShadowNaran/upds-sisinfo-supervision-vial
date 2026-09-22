import { actualizarEstadoCola, obtenerColaOrdenada } from '@/services/pendingSync'
import { actualizarPlanilla, crearPlanilla } from '@/services/campo'
import type { Planilla } from '@/types/campo'

let sincronizando = false

export async function sincronizarPendientes(): Promise<void> {
  if (sincronizando || !navigator.onLine) return
  sincronizando = true
  try {
    for (const item of await obtenerColaOrdenada()) {
      if (item.tipo !== 'planilla') continue
      await actualizarEstadoCola(item.id, 'enviando')
      try {
        const planilla = item.payload as Planilla
        if (planilla.id.startsWith('local-')) {
          const creada = await crearPlanilla(planilla.idTramo)
          planilla.id = creada.id
          planilla.detalles = creada.detalles.map((detalle, index) => ({ ...detalle, estado: planilla.detalles[index]?.estado ?? detalle.estado, clasificacion: planilla.detalles[index]?.clasificacion, fotoBase64: planilla.detalles[index]?.fotoBase64 }))
        }
        await actualizarPlanilla(planilla)
        await actualizarEstadoCola(item.id, 'sincronizado')
      } catch (error) {
        await actualizarEstadoCola(item.id, 'error', error instanceof Error ? error.message : 'No se pudo sincronizar')
      }
    }
  } finally {
    sincronizando = false
  }
}