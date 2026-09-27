<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { obtenerCola, reintentarItem } from '@/services/pendingSync'
import type { ItemCola } from '@/services/pendingSync'
import { sincronizarPendientes } from '@/services/sync'

const emit = defineEmits<{
  (e: 'cerrar'): void
}>()

const items = ref<ItemCola[]>([])
let intervalo = 0

async function cargar() {
  const todos = await obtenerCola()
  items.value = todos.filter(i => i.estado !== 'sincronizado').sort((a, b) => b.creadoEn - a.creadoEn)
}

onMounted(() => {
  cargar()
  intervalo = window.setInterval(cargar, 2000)
})

onUnmounted(() => {
  clearInterval(intervalo)
})

function formatoFecha(ts: number) {
  return new Date(ts).toLocaleString()
}

function labelTipo(tipo: string) {
  switch (tipo) {
    case 'planilla': return 'Planilla de Asistencia'
    case 'personal_manual': return 'Registro de Personal'
    default: return tipo
  }
}

async function reintentar(id: string) {
  await reintentarItem(id)
  await cargar()
  if (navigator.onLine) {
    void sincronizarPendientes()
  }
}
</script>

<template>
  <div class="bandeja-overlay" @click.self="emit('cerrar')">
    <div class="bandeja">
      <header class="bandeja__header">
        <h2>Bandeja de Salida</h2>
        <button class="btn-cerrar" @click="emit('cerrar')">✕</button>
      </header>
      
      <div class="bandeja__lista">
        <p v-if="items.length === 0" class="vacio">No hay elementos pendientes por sincronizar.</p>
        
        <article v-for="item in items" :key="item.id" class="item" :class="'item--' + item.estado">
          <div class="item__info">
            <strong>{{ labelTipo(item.tipo) }}</strong>
            <small>{{ formatoFecha(item.creadoEn) }}</small>
            <div class="item__estado">
              <span class="badge-estado" :class="'badge-' + item.estado">
                {{ item.estado.toUpperCase() }}
              </span>
              <span v-if="item.prioridad === 'alta'" class="badge-alta">ALTA PRIORIDAD</span>
            </div>
            <p v-if="item.error" class="item__error">{{ item.error }}</p>
          </div>
          <div class="item__acciones" v-if="item.estado === 'error'">
            <button class="btn-reintentar" @click="reintentar(item.id)">Reintentar</button>
          </div>
        </article>
      </div>
    </div>
  </div>
</template>

<style scoped>
.bandeja-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.5);
  display: flex;
  justify-content: flex-end;
  z-index: 1000;
}

.bandeja {
  width: 100%;
  max-width: 400px;
  background: var(--hogar);
  height: 100dvh;
  display: flex;
  flex-direction: column;
  box-shadow: -4px 0 15px rgba(0,0,0,0.1);
  animation: slideIn 0.3s ease;
}

@keyframes slideIn {
  from { transform: translateX(100%); }
  to { transform: translateX(0); }
}

.bandeja__header {
  padding: 1.2rem;
  background: var(--asfalto);
  color: var(--hogar);
  display: flex;
  justify-content: space-between;
  align-items: center;
}
.bandeja__header h2 { margin: 0; font-size: 1.2rem; }
.btn-cerrar { background: none; border: none; color: white; font-size: 1.5rem; cursor: pointer; }

.bandeja__lista {
  flex: 1;
  overflow-y: auto;
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.vacio {
  text-align: center;
  color: #666;
  margin-top: 2rem;
}

.item {
  border: 1px solid var(--linea);
  border-radius: var(--radio-sm);
  padding: 1rem;
  background: white;
  box-shadow: var(--sombra-sm);
}
.item--error {
  border-left: 4px solid var(--rojo-senal);
}
.item--enviando {
  border-left: 4px solid var(--amarillo-ruta);
}

.item__info strong {
  display: block;
  font-size: 1rem;
  color: var(--asfalto);
}
.item__info small {
  color: #666;
}

.item__estado {
  display: flex;
  gap: 0.5rem;
  margin-top: 0.5rem;
}

.badge-estado {
  font-size: 0.7rem;
  padding: 0.2rem 0.5rem;
  border-radius: 1rem;
  font-weight: 600;
}
.badge-pendiente { background: #eee; color: #333; }
.badge-enviando { background: var(--amarillo-ruta); color: var(--asfalto); }
.badge-error { background: var(--rojo-senal); color: white; }

.badge-alta {
  font-size: 0.7rem;
  padding: 0.2rem 0.5rem;
  border-radius: 1rem;
  font-weight: 600;
  background: var(--naranja-obra);
  color: white;
}

.item__error {
  margin-top: 0.5rem;
  font-size: 0.85rem;
  color: var(--rojo-senal);
}

.item__acciones {
  margin-top: 1rem;
  text-align: right;
}
.btn-reintentar {
  background: var(--asfalto);
  color: white;
  border: none;
  padding: 0.5rem 1rem;
  border-radius: var(--radio-sm);
  cursor: pointer;
  font-size: 0.85rem;
  font-weight: 600;
}
</style>
