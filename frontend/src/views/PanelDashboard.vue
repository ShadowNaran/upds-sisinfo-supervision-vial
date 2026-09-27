<script setup lang="ts">
import { ref, onMounted, computed, watch } from 'vue'
import { useRouter } from 'vue-router'
import { listarPlanillas, listarTodoPersonal, listarTramos } from '@/services/campo'
import type { PlanillaResumen, PersonalResumen } from '@/services/campo'
import { useAuthStore } from '@/stores/auth'
import GestionPersonal from './GestionPersonal.vue'
import AdminPanel from './AdminPanel.vue'

const auth = useAuthStore()
const router = useRouter()
const planillas = ref<PlanillaResumen[]>([])
const personalTodo = ref<PersonalResumen[]>([])
const cargando = ref(true)
const error = ref('')
const pestanaActiva = ref<'reportes' | 'personal' | 'admin'>('reportes')
const tramosSistema = ref<any[]>([])

// filtros
const filtroFechaInicio = ref('')
const filtroFechaFin = ref('')
const filtroTramo = ref('')

async function cargarDashboard() {
  cargando.value = true
  try {
    const [p, per, trs] = await Promise.all([listarPlanillas(), listarTodoPersonal(), listarTramos()])
    planillas.value = p
    personalTodo.value = per
    tramosSistema.value = trs
  } catch (err) {
    error.value = 'No se pudieron cargar los datos.'
  } finally {
    cargando.value = false
  }
}

onMounted(() => {
  cargarDashboard()
})

watch(pestanaActiva, (nueva) => {
  if (nueva === 'reportes') {
    cargarDashboard()
  }
})

const pendientesValidacion = computed(() => personalTodo.value.filter(p => p.estadoValidacion === 'PendienteValidacion').length)

const planillasFiltradas = computed(() => {
  return planillas.value.filter(p => {
    let matchFecha = true
    if (filtroFechaInicio.value && filtroFechaFin.value) {
      matchFecha = p.fecha >= filtroFechaInicio.value && p.fecha <= filtroFechaFin.value
    } else if (filtroFechaInicio.value) {
      matchFecha = p.fecha >= filtroFechaInicio.value
    } else if (filtroFechaFin.value) {
      matchFecha = p.fecha <= filtroFechaFin.value
    }
    
    const matchTramo = filtroTramo.value ? p.tramo.toLowerCase().includes(filtroTramo.value.toLowerCase()) : true
    return matchFecha && matchTramo
  })
})

const tramosUnicos = computed(() => {
  return tramosSistema.value.map(t => t.nombre)
})

const indicadores = computed(() => {
  const hoy = new Date().toISOString().split('T')[0]
  const planillasHoy = planillas.value.filter(p => p.fecha.startsWith(hoy))
  return {
    totalReportesHoy: planillasHoy.length,
    totalPersonalHoy: planillasHoy.reduce((sum, p) => sum + p.personal, 0)
  }
})


const asistenciaGlobal = computed(() => {
  const conPersonal = planillasFiltradas.value.filter(p => p.personal > 0)
  if (!conPersonal.length) return 0
  const suma = conPersonal.reduce((s, p) => s + p.presentes / p.personal * 100, 0)
  return Math.round(suma / conPersonal.length)
})

const tramosCriticos = computed(() =>
  planillasFiltradas.value.filter(p => p.personal > 0 && p.presentes / p.personal < 0.7).length
)

function irAlReporte(id: string) {
  router.push(`/panel/reporte/${id}`)
}
</script>

<template>
  <div class="page-shell">
    <!-- encabezado -->
    <div class="page-heading">
      <div>
        <p class="eyebrow">CENTRO DE OPERACIONES</p>
        <h1>Dashboard Central</h1>
        <p>Monitoreo de tramos y personal en tiempo real</p>
      </div>
      <div class="updated">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/></svg>
        <span>ÚLTIMA ACTUALIZACIÓN<b>{{ new Date().toLocaleDateString('es-BO', { day:'2-digit', month:'short' }) }} · {{ new Date().toLocaleTimeString('es-BO', { hour:'2-digit', minute:'2-digit' }) }}</b></span>
      </div>
    </div>

    <!-- navegacion -->
    <div class="tabs">
      <button :class="{ active: pestanaActiva === 'reportes' }" @click="pestanaActiva = 'reportes'">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="5" y="4" width="14" height="18" rx="2"/><path d="M9 4V2h6v2M9 10h6m-6 4h6m-6 4h4"/></svg>
        REPORTES DIARIOS
      </button>
      <button :class="{ active: pestanaActiva === 'personal' }" @click="pestanaActiva = 'personal'">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M18 8a3 3 0 0 1 0 6m4 7v-2a4 4 0 0 0-3-3.87"/></svg>
        PERSONAL DE CAMPO
        <span v-if="pendientesValidacion > 0">{{ pendientesValidacion }}</span>
      </button>
      <button v-if="['Administrador', 'SupervisorCampo'].includes(auth.perfil?.rol || '')" :class="{ active: pestanaActiva === 'admin' as any }" @click="pestanaActiva = 'admin' as any">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 16V8a2 2 0 0 0-1-1.73l-7-4a2 2 0 0 0-2 0l-7 4A2 2 0 0 0 3 8v8a2 2 0 0 0 1 1.73l7 4a2 2 0 0 0 2 0l7-4A2 2 0 0 0 21 16z"/><polyline points="3.27 6.96 12 12.01 20.73 6.96"/><line x1="12" y1="22.08" x2="12" y2="12"/></svg>
        ADMINISTRACIÓN
      </button>
    </div>

    <!-- administracion -->
    <AdminPanel v-if="pestanaActiva === 'admin'" />

    <!-- personal -->
    <GestionPersonal v-if="pestanaActiva === 'personal'" />

    <!-- reportes -->
    <template v-if="pestanaActiva === 'reportes'">

      <!-- indicadores -->
      <section class="kpi-grid">
        <article class="kpi">
          <div class="kpi-icon">
            <svg width="23" height="23" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="5" y="4" width="14" height="18" rx="2"/><path d="M9 4V2h6v2M9 10h6m-6 4h6m-6 4h4"/></svg>
          </div>
          <div><p>REPORTES HOY</p><strong>{{ indicadores.totalReportesHoy }}</strong><small>Planillas registradas</small></div>
        </article>
        <article class="kpi yellow">
          <div class="kpi-icon">
            <svg width="23" height="23" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M18 8a3 3 0 0 1 0 6m4 7v-2a4 4 0 0 0-3-3.87"/></svg>
          </div>
          <div><p>PERSONAL REGISTRADO</p><strong>{{ indicadores.totalPersonalHoy }}</strong><small>En tramos activos hoy</small></div>
        </article>
        <article class="kpi green">
          <div class="kpi-icon">
            <svg width="23" height="23" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m5 12 4 4L19 6"/></svg>
          </div>
          <div><p>ASISTENCIA GLOBAL</p><strong>{{ asistenciaGlobal }}%</strong><small>Promedio entre tramos</small></div>
        </article>
        <article class="kpi red">
          <div class="kpi-icon">
            <svg width="23" height="23" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 3 2 21h20z"/><path d="M12 9v5m0 3h.01"/></svg>
          </div>
          <div><p>TRAMOS CRÍTICOS</p><strong>{{ tramosCriticos }}</strong><small>Con asistencia &lt; 70%</small></div>
        </article>
      </section>

      <!-- filtros -->
      <section class="panel-box filters">
        <div class="filter-title">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="11" cy="11" r="7"/><path d="m20 20-4-4"/></svg>
          <div><b>FILTRAR REPORTES</b><span>Refina los resultados por fecha y tramo</span></div>
        </div>
        <label>DESDE
          <div class="input-like">
            <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M8 2v4m8-4v4M3 10h18"/><rect x="3" y="4" width="18" height="18" rx="2"/></svg>
            <input type="date" v-model="filtroFechaInicio" style="border:0;outline:0;font-size:12px;color:#343a3e;" />
          </div>
        </label>
        <label>HASTA
          <div class="input-like">
            <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M8 2v4m8-4v4M3 10h18"/><rect x="3" y="4" width="18" height="18" rx="2"/></svg>
            <input type="date" v-model="filtroFechaFin" style="border:0;outline:0;font-size:12px;color:#343a3e;" />
          </div>
        </label>
        <label class="stretch">TRAMO
          <select v-model="filtroTramo" aria-label="Tramo">
            <option value="">Todos los tramos</option>
            <option v-for="t in tramosUnicos" :key="t" :value="t">{{ t }}</option>
          </select>
        </label>
        <button class="clear-filter" @click="filtroFechaInicio = ''; filtroFechaFin = ''; filtroTramo = ''">LIMPIAR</button>
      </section>

      <!-- reportes recientes -->
      <div class="section-title">
        <div><h2>REPORTES RECIENTES</h2><p>Mostrando {{ planillasFiltradas.length }} de {{ planillas.length }} reportes</p></div>
      </div>

      <div v-if="cargando" style="text-align:center;padding:3rem;color:#666;">Cargando reportes...</div>
      <div v-else-if="error" style="text-align:center;padding:3rem;color:var(--red);">{{ error }}</div>
      <div v-else-if="planillasFiltradas.length === 0" style="text-align:center;padding:3rem;color:#666;background:white;border-radius:10px;border:1px dashed var(--linea);">No se encontraron reportes con estos filtros.</div>

      <div v-else class="report-grid">
        <article
          v-for="p in planillasFiltradas"
          :key="p.id"
          class="report-card"
          :class="{ critical: p.presentes / p.personal < 0.7 && p.personal > 0 }"
          @click="irAlReporte(p.id)"
          style="cursor:pointer;"
        >
          <div class="report-head">
            <div class="date-box">
              <strong>{{ p.fecha.split('-')[2] }}</strong>
              <span>{{ ['ENE','FEB','MAR','ABR','MAY','JUN','JUL','AGO','SEP','OCT','NOV','DIC'][parseInt(p.fecha.split('-')[1])-1] }}</span>
            </div>
            <div>
              <p>{{ new Date(p.fecha).toLocaleDateString('es-BO', { weekday: 'long' }).toUpperCase() }}</p>
              <h3>{{ p.fecha }}</h3>
            </div>
            <span class="status-badge" :class="p.estado.toLowerCase() === 'cerrada' ? 'cerrada' : 'borrador'">{{ p.estado.toUpperCase() }}</span>
          </div>
          <div class="route-name">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="6" cy="19" r="2"/><circle cx="18" cy="5" r="2"/><path d="M8 19h2a2 2 0 0 0 2-2V7a2 2 0 0 1 2-2h2"/></svg>
            <span>{{ p.tramo }}</span>
          </div>
          <div class="attendance-line">
            <div>
              <span>ASISTENCIA</span>
              <b :class="p.personal > 0 && p.presentes/p.personal < 0.7 ? 'red-text' : ''">{{ p.personal > 0 ? Math.round(p.presentes/p.personal*100) : 0 }}%</b>
            </div>
            <div class="progress-track">
              <span :class="p.personal > 0 && p.presentes/p.personal < 0.7 ? 'danger-bar' : ''" :style="{ width: (p.personal > 0 ? Math.round(p.presentes/p.personal*100) : 0) + '%' }" />
            </div>
          </div>
          <div class="report-stats">
            <span>
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/></svg>
              {{ p.presentes }} / {{ p.personal }} presentes
            </span>
            <span v-if="p.cerradaEn">Cerrada: {{ new Date(p.cerradaEn).toLocaleTimeString() }}</span>
          </div>
          <button class="card-link">
            Ver reporte completo
            <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m9 18 6-6-6-6"/></svg>
          </button>
        </article>
      </div>

    </template>
  </div>
</template>

<style scoped>
/* estilos globales */
</style>
