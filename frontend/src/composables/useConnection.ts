import { onMounted, onUnmounted, ref, readonly, computed } from 'vue'
import { contarPendientes } from '@/services/pendingSync'
import { sincronizarPendientes } from '@/services/sync'

type EstadoConexion = 'online' | 'offline' | 'online-con-pendientes'

const enLinea = ref(typeof navigator !== 'undefined' ? navigator.onLine : true)
const pendientes = ref(0)

function actualizarEnLinea(): void {
  enLinea.value = typeof navigator !== 'undefined' ? navigator.onLine : true
  if (enLinea.value) void sincronizarPendientes()
}

async function actualizarPendientes(): Promise<void> {
  try {
    pendientes.value = await contarPendientes()
  } catch {
    pendientes.value = 0
  }
}

const listeners = new Set<() => void>()

function suscribir(): void {
  if (listeners.size === 0) {
    window.addEventListener('online', actualizarEnLinea)
    window.addEventListener('offline', actualizarEnLinea)
    // actualiza pendientes cada 2 segundos
    setInterval(actualizarPendientes, 2000)
    actualizarPendientes()
  }
  listeners.add(actualizarEnLinea)
}

function desuscribir(): void {
  listeners.delete(actualizarEnLinea)
  if (listeners.size === 0) {
    window.removeEventListener('online', actualizarEnLinea)
    window.removeEventListener('offline', actualizarEnLinea)
  }
}

const estado = computed<EstadoConexion>(() => {
  if (!enLinea.value) return 'offline'
  return pendientes.value > 0 ? 'online-con-pendientes' : 'online'
})

const etiqueta = computed(() => {
  switch (estado.value) {
    case 'online': return 'En línea'
    case 'online-con-pendientes': return `En línea (${pendientes.value} pendiente${pendientes.value !== 1 ? 's' : ''})`
    case 'offline': return 'Sin conexión'
  }
})

// estado de conexion y pendientes
export function useConnection() {
  onMounted(() => {
    suscribir()
    actualizarPendientes()
  })
  onUnmounted(desuscribir)

  return {
    enLinea: readonly(enLinea),
    pendientes: readonly(pendientes),
    estado: readonly(estado),
    etiqueta: readonly(etiqueta),
    actualizarPendientes,
  }
}

// estado de conexion sin contador
export function useOnline() {
  onMounted(suscribir)
  onUnmounted(desuscribir)
  return { enLinea: readonly(enLinea) }
}