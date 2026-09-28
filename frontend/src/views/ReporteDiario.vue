<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { obtenerPlanilla, registrarMitigacion } from '@/services/campo'
import type { Planilla } from '@/types/campo'

const route = useRoute()
const router = useRouter()
const id = route.params.id as string

const planilla = ref<Planilla | null>(null)
const cargando = ref(true)
const error = ref('')

// estado del formulario de mitigacion
const mitigandoId = ref<string | null>(null)
const accionTexto = ref('')
const marcandoFalsoPositivo = ref(false)
const guardandoMitigacion = ref(false)

onMounted(async () => {
  try {
    planilla.value = await obtenerPlanilla(id)
  } catch (err) {
    error.value = 'Error al cargar el reporte diario.'
  } finally {
    cargando.value = false
  }
})

// totales del reporte
const totalPersonal = computed(() => planilla.value?.detalles.length || 0)
const presentes = computed(() => planilla.value?.detalles.filter(d => d.estado === 'Presente').length || 0)
const faltas = computed(() => planilla.value?.detalles.filter(d => d.estado === 'Falta').length || 0)

const eventos = computed(() => {
  if (!planilla.value) return []
  return planilla.value.detalles.filter(d => 
    d.observacion || d.clasificacion || d.severidad !== undefined || d.fotoBase64 || d.estado === 'Falta'
  )
})

async function guardarMitigacion(idDetalle: string, falsoPositivo: boolean = false) {
  if (!accionTexto.value.trim()) return
  if (!navigator.onLine) {
    alert('Esta acción requiere conexión a internet.')
    return
  }
  
  try {
    guardandoMitigacion.value = true
    await registrarMitigacion(idDetalle, accionTexto.value, falsoPositivo)
    
    // actualiza el reporte local
    if (planilla.value) {
      const det = planilla.value.detalles.find(d => d.id === idDetalle)
      if (det) {
        det.accionMitigacion = accionTexto.value
        det.horaMitigacion = new Date().toISOString()
        det.esFalsoPositivo = falsoPositivo
      }
    }
    
    // limpia el formulario
    mitigandoId.value = null
    accionTexto.value = ''
    marcandoFalsoPositivo.value = false
  } catch (err) {
    alert('Error al registrar la acción')
  } finally {
    guardandoMitigacion.value = false
  }
}

function imprimir() {
  window.print()
}
</script>

<template>
  <div class="page-shell">
    <div v-if="cargando" style="padding: 20px; text-align: center; color: #666;">Generando reporte...</div>
    <div v-else-if="error" style="padding: 20px; text-align: center; color: var(--red);">{{ error }}</div>
    
    <template v-else-if="planilla">
      <div style="display:flex; justify-content:space-between; align-items:center;">
        <button class="back-link" @click="router.push('/panel')">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m15 18-6-6 6-6"/></svg>
          VOLVER AL LISTADO
        </button>
        <button class="secondary-button" @click="imprimir" style="padding:4px 12px; height:32px; font-size:11px;">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
          DESCARGAR PDF
        </button>
      </div>
      
      <div class="report-title">
        <div>
          <h1>{{ planilla.tramo }}</h1>
          <p>
            <strong>{{ new Date(planilla.fecha).toLocaleDateString('es-BO', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) }}</strong>
            <span />
            Reporte generado automáticamente
          </p>
        </div>
        <span class="status-badge" :class="planilla.estado.toLowerCase() === 'cerrada' ? 'cerrada' : 'borrador'">{{ planilla.estado.toUpperCase() }}</span>
      </div>

      <div class="detail-kpis">
        <div><span>TOTAL ASIGNADOS</span><b>{{ totalPersonal }}</b></div>
        <div><span>PERSONAL PRESENTE</span><b style="color:var(--green)">{{ presentes }}</b></div>
        <div><span>FALTAS REGISTRADAS</span><b :style="faltas > 0 ? 'color:var(--red)' : ''">{{ faltas }}</b></div>
      </div>

      <div class="section-title" style="margin-top:40px;">
        <div><h2>BITÁCORA DE EVENTOS Y EVIDENCIAS</h2><p>Registro detallado de estado de ruta y justificaciones.</p></div>
      </div>

      <div v-if="eventos.length === 0" style="padding: 30px; text-align: center; background: white; border: 1px dashed var(--linea); border-radius: 10px; color: #666;">
        No se registraron incidentes ni fotografías durante la jornada.
      </div>

      <div v-else style="display:flex; flex-direction:column; gap:16px;">
        <article v-for="evt in eventos" :key="evt.id" class="event-card" :class="{ 'critical': evt.estado === 'Falta' || evt.severidad === 2 }">
          <div class="event-copy">
            <div class="event-top">
              <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="#e9500e" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 8v8m-4-4h8"/></svg>
              <div>
                <span v-if="evt.severidad !== undefined" :style="evt.severidad === 0 ? 'color:var(--green)' : evt.severidad === 1 ? 'color:#b18200' : 'color:var(--red)'">
                  {{ evt.severidad === 0 ? 'TRANSITA' : evt.severidad === 1 ? 'PRECAUCIÓN' : 'NO TRANSITA' }}
                </span>
                <h2 v-if="evt.clasificacion">{{ evt.clasificacion.toUpperCase() }} - {{ evt.personal }}</h2>
                <h2 v-else>{{ evt.personal }}</h2>
              </div>
              <b v-if="evt.estado === 'Falta'">FALTA INJUSTIFICADA</b>
            </div>
            
            <div class="event-meta">
              <span v-if="evt.kilometraje">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="10" r="3"/><path d="M12 21.7C17.3 17 20 13 20 10a8 8 0 1 0-16 0c0 3 2.7 7 8 11.7z"/></svg>
                Km: {{ evt.kilometraje.toFixed(3) }}
              </span>
              <span v-if="evt.latitud && evt.longitud">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
                Coordenadas registradas
              </span>
            </div>

            <p class="observation" v-if="evt.observacion">{{ evt.observacion }}</p>
            <p class="observation" v-else-if="evt.estado === 'Falta'">El trabajador no se presentó al punto de encuentro y no hay justificación registrada.</p>

            <!-- mitigacion de eventos -->
            <div v-if="evt.severidad === 2 || evt.severidad === 1 || evt.estado === 'Falta'">
              <div v-if="evt.accionMitigacion" class="mitigacion" :class="{ 'done': !evt.esFalsoPositivo }">
                <div style="flex:0 0 auto;">{{ evt.esFalsoPositivo ? '❌' : '🛡️' }}</div>
                <div>
                  <b>{{ evt.esFalsoPositivo ? 'FALSO POSITIVO / DESESTIMADO' : 'ACCIÓN DE MITIGACIÓN TOMADA' }}</b>
                  <p>{{ evt.accionMitigacion }}</p>
                </div>
              </div>
              
              <div v-else style="margin-top:18px;">
                <div v-if="mitigandoId !== evt.id" style="display:flex; gap:10px;">
                  <button class="secondary-button" @click="mitigandoId = evt.id; accionTexto = ''; marcandoFalsoPositivo = false" style="flex:1;">Registrar mitigación</button>
                  <button class="secondary-button" @click="mitigandoId = evt.id; accionTexto = ''; marcandoFalsoPositivo = true" style="flex:1; border-style:dashed;">Marcar falso positivo</button>
                </div>
                <div v-else style="display:flex; gap:10px;">
                  <input type="text" v-model="accionTexto" class="input-like" style="flex:1;" :placeholder="marcandoFalsoPositivo ? 'Justificación (Ej: Duplicado)' : 'Ej: Se envió maquinaria...'" :disabled="guardandoMitigacion" />
                  <button class="primary-button" @click="guardarMitigacion(evt.id, marcandoFalsoPositivo)" :disabled="guardandoMitigacion || !accionTexto.trim()">{{ guardandoMitigacion ? '...' : 'Guardar' }}</button>
                  <button class="clear-filter" @click="mitigandoId = null" :disabled="guardandoMitigacion">Cancelar</button>
                </div>
              </div>
            </div>

          </div>
          
          <!-- evidencia fotografica -->
          <div class="evidence-photo" v-if="evt.fotoBase64">
            <span>
              <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14.5 4h-5L7 7H4a2 2 0 0 0-2 2v9a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2h-3l-2.5-3z"/><circle cx="12" cy="13" r="3"/></svg>
              EVIDENCIA CAPTURADA
            </span>
            <img :src="evt.fotoBase64" style="width:100%; height:100%; object-fit:cover;" />
            <small>{{ new Date(planilla.fecha).toLocaleDateString() }} · {{ evt.latitud != null && evt.longitud != null ? `${evt.latitud.toFixed(5)}, ${evt.longitud.toFixed(5)}` : 'Ubicación no disponible' }}</small>
          </div>
          <div class="evidence-photo" v-else style="background:var(--asfalto); display:grid; place-content:center; color:#535a60; font-size:11px; font-family:'Archivo';">
            Sin evidencia fotográfica
          </div>
        </article>
      </div>

      <!-- firma -->
      <div v-if="planilla.tieneFirma" style="margin-top:40px; padding:20px; background:white; border:1px solid var(--linea); border-radius:10px; display:flex; flex-direction:column; align-items:center; gap:16px;">
        <span class="status-badge cerrada" style="margin:0;">PLANILLA FIRMADA Y CERTIFICADA</span>
        <img v-if="planilla.firmaBase64" :src="planilla.firmaBase64" alt="Firma del responsable" style="max-width:300px; max-height:150px;" />
      </div>

    </template>
  </div>
</template>

<style scoped>
/* estilos globales */
</style>
