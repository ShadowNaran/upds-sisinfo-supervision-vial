import { actualizarEstadoCola, obtenerColaOrdenada } from '@/services/pendingSync'
import { actualizarPlanilla, crearPlanilla } from '@/services/campo'
import type { Planilla } from '@/types/campo'

let sincronizando = false

export async function sincronizarPendientes(): Promise<void> {
  if (sincronizando || !navigator.onLine) return
  sincronizando = true
  try {
    for (const item of await obtenerColaOrdenada()) {
      if (item.reintentos >= 5) {
        await actualizarEstadoCola(item.id, 'error', 'Máximo de reintentos superado, se requiere revisión manual.')
        continue
      }

      if (item.tipo === 'planilla') {
        await actualizarEstadoCola(item.id, 'enviando')
        try {
          const planilla = item.payload as Planilla
          if (planilla.id.startsWith('local-')) {
            const creada = await crearPlanilla(planilla.idTramo)
            planilla.id = creada.id
            planilla.detalles = creada.detalles.map((detalle, index) => {
              const local = planilla.detalles.find(d => d.personal === detalle.personal) || planilla.detalles[index]
              return { 
                ...detalle, 
                estado: local?.estado ?? detalle.estado, 
                clasificacion: local?.clasificacion, 
                fotoBase64: local?.fotoBase64,
                latitud: local?.latitud,
                longitud: local?.longitud,
                kilometraje: local?.kilometraje,
                severidad: local?.severidad
              }
            })
          }
          await actualizarPlanilla(planilla)
          await actualizarEstadoCola(item.id, 'sincronizado')
        } catch (error) {
          const esRed = !navigator.onLine || (error instanceof Error && error.message.includes('fetch'))
          await actualizarEstadoCola(item.id, esRed ? 'pendiente' : 'error', error instanceof Error ? error.message : 'Error de sincronización')
          if (esRed) break // reintenta cuando vuelva la conexion
        }
      } else if (item.tipo === 'personal_manual') {
        await actualizarEstadoCola(item.id, 'enviando')
        try {
          const { crearPersonalManual } = await import('@/services/campo')
          await crearPersonalManual(item.payload as any)
          await actualizarEstadoCola(item.id, 'sincronizado')
        } catch (error) {
          const esRed = !navigator.onLine || (error instanceof Error && error.message.includes('fetch'))
          await actualizarEstadoCola(item.id, esRed ? 'pendiente' : 'error', error instanceof Error ? error.message : 'Error de sincronización')
          if (esRed) break // reintenta cuando vuelva la conexion
        }
      }
    }
  } finally {
    sincronizando = false
  }
}