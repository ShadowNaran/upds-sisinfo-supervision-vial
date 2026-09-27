<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { listarTodoPersonal, validarPersonal } from '@/services/campo'
import type { PersonalResumen } from '@/services/campo'

const personal = ref<PersonalResumen[]>([])
const cargando = ref(true)
const error = ref('')
const procesando = ref<string | null>(null)
const mensaje = ref('')
const filtroEstado = ref<'todos' | 'PendienteValidacion' | 'Aprobado' | 'Rechazado'>('PendienteValidacion')

onMounted(async () => {
  await cargar()
})

async function cargar() {
  try {
    cargando.value = true
    error.value = ''
    personal.value = await listarTodoPersonal()
  } catch {
    error.value = 'No se pudo cargar el personal. Verifica la conexion.'
  } finally {
    cargando.value = false
  }
}

async function accion(id: string, estado: 'Aprobado' | 'Rechazado') {
  procesando.value = id
  mensaje.value = ''
  try {
    await validarPersonal(id, estado)
    const item = personal.value.find(p => p.id === id)
    if (item) item.estadoValidacion = estado
    mensaje.value = estado === 'Aprobado' ? 'Personal aprobado.' : 'Personal rechazado.'
    setTimeout(() => { mensaje.value = '' }, 3000)
  } catch {
    error.value = 'No se pudo actualizar el estado del personal.'
  } finally {
    procesando.value = null
  }
}

const personalFiltrado = computed(() => {
  if (filtroEstado.value === 'todos') return personal.value
  return personal.value.filter(p => p.estadoValidacion === filtroEstado.value)
})

const contadores = computed(() => ({
  pendientes: personal.value.filter(p => p.estadoValidacion === 'PendienteValidacion').length,
  aprobados: personal.value.filter(p => p.estadoValidacion === 'Aprobado').length,
  rechazados: personal.value.filter(p => p.estadoValidacion === 'Rechazado').length,
}))

</script>

<template>
  <div class="page-shell">
    <div class="page-heading">
      <div>
        <p class="eyebrow">RECURSOS HUMANOS</p>
        <h1>Gestión de Personal</h1>
        <p>Valida o rechaza trabajadores registrados manualmente por supervisores.</p>
      </div>
      <div>
        <button class="secondary-button" @click="cargar" :disabled="cargando">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21.5 2v6h-6M2.5 22v-6h6M2 11.5a10 10 0 0 1 18.8-4.3M22 12.5a10 10 0 0 1-18.8 4.2"/></svg>
          ACTUALIZAR
        </button>
      </div>
    </div>

    <div class="filters-container">
      <button :class="['filter-pill', filtroEstado === 'PendienteValidacion' ? 'dark-pill' : 'outline-pill']" @click="filtroEstado = 'PendienteValidacion'">
        Pendientes {{ contadores.pendientes }}
      </button>
      <button :class="['filter-pill', filtroEstado === 'Aprobado' ? 'dark-pill' : 'outline-pill']" @click="filtroEstado = 'Aprobado'">
        Aprobados {{ contadores.aprobados }}
      </button>
      <button :class="['filter-pill', filtroEstado === 'Rechazado' ? 'dark-pill' : 'outline-pill']" @click="filtroEstado = 'Rechazado'">
        Rechazados {{ contadores.rechazados }}
      </button>
      <button :class="['filter-pill', filtroEstado === 'todos' ? 'dark-pill' : 'outline-pill']" @click="filtroEstado = 'todos'">
        Todos {{ personal.length }}
      </button>
    </div>

    <div v-if="mensaje" class="toast" style="margin-bottom:20px;">
      <div class="toast-icon" style="background:#e5f6ed; color:var(--green);">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
      </div>
      <div><b style="color:var(--green)">Notificación</b><p>{{ mensaje }}</p></div>
    </div>
    
    <div v-if="error" class="toast" style="margin-bottom:20px;">
      <div class="toast-icon" style="background:#ffebe9; color:var(--red);">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
      </div>
      <div><b style="color:var(--red)">Error</b><p>{{ error }}</p></div>
    </div>

    <div v-if="cargando" style="padding: 20px; text-align: center; color: #666;">Cargando personal...</div>
    <div v-else-if="personalFiltrado.length === 0" style="padding: 30px; text-align: center; background: white; border: 1px dashed var(--linea); border-radius: 10px; color: #666;">
      No hay personal con ese estado.
    </div>
    
    <div v-else class="person-list">
      <article v-for="p in personalFiltrado" :key="p.id" class="person-card-ui">
        <div class="person-left">
          <div class="avatar-circle">
            {{ p.nombreCompleto.split(' ').map((n: string) => n[0]).slice(0, 2).join('').toUpperCase() }}
          </div>
          <div class="person-info">
            <h3>{{ p.nombreCompleto }}</h3>
            <p>{{ p.cargo ? p.cargo + ' · ' : '' }}CI {{ p.documento }}</p>
          </div>
        </div>

        <div class="tramo-info">
          <span class="label">TRAMO</span>
          <strong>{{ p.tramo }}</strong>
        </div>
        
        <div class="card-actions">
          <span v-if="p.estadoValidacion === 'PendienteValidacion'" class="ui-badge yellow-badge">PENDIENTE</span>
          <span v-if="p.estadoValidacion === 'Aprobado'" class="ui-badge green-badge">APROBADO</span>
          <span v-if="p.estadoValidacion === 'Rechazado'" class="ui-badge red-badge">RECHAZADO</span>

          <template v-if="p.estadoValidacion === 'PendienteValidacion'">
            <button class="action-btn btn-approve" :disabled="procesando === p.id" @click="accion(p.id, 'Aprobado')">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M20 6L9 17l-5-5"/></svg>
              {{ procesando === p.id ? '...' : 'Aprobar' }}
            </button>
            <button class="action-btn btn-reject" :disabled="procesando === p.id" @click="accion(p.id, 'Rechazado')">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M18 6L6 18M6 6l12 12"/></svg>
              {{ procesando === p.id ? '...' : 'Rechazar' }}
            </button>
          </template>
        </div>
      </article>
    </div>
  </div>
</template>

<style scoped>
.filters-container {
  display: flex;
  gap: 12px;
  margin-bottom: 24px;
}

.filter-pill {
  padding: 8px 16px;
  border-radius: 6px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  border: 1px solid transparent;
}

.dark-pill {
  background: #25282c;
  color: white;
}

.outline-pill {
  background: white;
  color: #555;
  border-color: #ddd;
}
.outline-pill:hover {
  border-color: #aaa;
}

.person-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.person-card-ui {
  background: white;
  border-radius: 8px;
  padding: 16px 24px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  box-shadow: 0 1px 3px rgba(0,0,0,0.05);
  border: 1px solid #f0f0f0;
}

.person-left {
  display: flex;
  align-items: center;
  gap: 16px;
  flex: 1;
}

.avatar-circle {
  width: 44px;
  height: 44px;
  background: #eee;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 14px;
  color: #333;
}

.person-info h3 {
  margin: 0 0 4px 0;
  font-size: 16px;
  font-weight: 600;
  color: #222;
}

.person-info p {
  margin: 0;
  font-size: 13px;
  color: #777;
}

.tramo-info {
  display: flex;
  flex-direction: column;
  flex: 1;
}

.tramo-info .label {
  font-size: 10px;
  color: #999;
  letter-spacing: 0.5px;
  margin-bottom: 2px;
}

.tramo-info strong {
  font-size: 14px;
  color: #333;
  font-weight: 600;
}

.card-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.ui-badge {
  padding: 6px 12px;
  border-radius: 12px;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.5px;
}

.yellow-badge {
  background: #fdf3cd;
  color: #a88200;
}

.green-badge {
  background: #e3f5e9;
  color: #219653;
}

.red-badge {
  background: #ffebe9;
  color: #d92d20;
}

.action-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  border-radius: 4px;
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  background: white;
  transition: all 0.2s;
}

.btn-approve {
  border: 1px solid #75c994;
  color: #219653;
}
.btn-approve:hover {
  background: #f0fbf4;
}

.btn-reject {
  border: 1px solid #f2a8a4;
  color: #d92d20;
}
.btn-reject:hover {
  background: #fdf2f2;
}
</style>
