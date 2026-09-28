<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { api } from '@/services/api'
import { listarTramos, crearPersonalManual, listarTodoPersonal } from '@/services/campo'
import type { Tramo } from '@/types/campo'
import { useAuthStore } from '@/stores/auth'

const tramos = ref<Tramo[]>([])
const personal = ref<any[]>([])
const cargando = ref(true)
const mensaje = ref('')
const error = ref('')

const formTramo = ref({ id: '', codigo: '', nombre: '', descripcion: '', kmInicio: 0, kmFin: 0 })
const formPersonal = ref({ id: '', nombreCompleto: '', documento: '', cargo: '', idTramo: '', telefono: '' })

const editandoTramo = computed(() => !!formTramo.value.id)
const editandoPersonal = computed(() => !!formPersonal.value.id)

const ciValido = computed(() => /^[0-9]{8}$/.test(formPersonal.value.documento))
const authStore = useAuthStore()
const esAdmin = computed(() => authStore.perfil?.rol === 'Administrador')

async function cargarDatos() {
  cargando.value = true
  try {
    tramos.value = await listarTramos()
    personal.value = await listarTodoPersonal()
    if (tramos.value.length > 0 && !editandoPersonal.value) formPersonal.value.idTramo = tramos.value[0].id
  } catch {
    error.value = 'Error al cargar datos'
  } finally {
    cargando.value = false
  }
}

function cargarParaEditarTramo(t: any) {
  formTramo.value = { id: t.id, codigo: t.codigo, nombre: t.nombre, descripcion: t.descripcion || '', kmInicio: t.kmInicio ?? 0, kmFin: t.kmFin ?? 0 }
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

function cancelarEdicionTramo() {
  formTramo.value = { id: '', codigo: '', nombre: '', descripcion: '', kmInicio: 0, kmFin: 0 }
}

async function guardarTramo() {
  if (!formTramo.value.codigo || !formTramo.value.nombre || formTramo.value.kmInicio > formTramo.value.kmFin) return
  if (!navigator.onLine) {
    error.value = 'Crear o editar tramos requiere conexión a internet.'
    window.scrollTo({ top: 0, behavior: 'smooth' })
    return
  }
  try {
    error.value = ''
    if (editandoTramo.value) {
      await api.put(`/api/tramos/${formTramo.value.id}`, formTramo.value)
      mensaje.value = 'Tramo actualizado exitosamente'
    } else {
      await api.post('/api/tramos', formTramo.value)
      mensaje.value = 'Tramo creado exitosamente'
    }
    cancelarEdicionTramo()
    await cargarDatos()
    setTimeout(() => mensaje.value = '', 3000)
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Error al guardar el tramo'
  }
}

async function desactivarTramo(id: string) {
  if (!navigator.onLine) {
    error.value = 'Desactivar tramos requiere conexión a internet.'
    window.scrollTo({ top: 0, behavior: 'smooth' })
    return
  }
  if (!confirm('¿Estás seguro de desactivar este tramo?')) return
  try {
    await api.delete(`/api/tramos/${id}`)
    mensaje.value = 'Tramo desactivado'
    await cargarDatos()
    setTimeout(() => mensaje.value = '', 3000)
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Error al desactivar el tramo'
  }
}

function cargarParaEditarPersonal(p: any) {
  formPersonal.value = { id: p.id, nombreCompleto: p.nombreCompleto, documento: p.documento, cargo: p.cargo || '', idTramo: p.idTramo, telefono: p.telefono || '' }
  window.scrollTo({ top: 0, behavior: 'smooth' })
}

function cancelarEdicionPersonal() {
  formPersonal.value = { id: '', nombreCompleto: '', documento: '', cargo: '', idTramo: tramos.value[0]?.id || '', telefono: '' }
}

async function guardarPersonal() {
  if (!formPersonal.value.nombreCompleto.trim() || !ciValido.value || !formPersonal.value.idTramo) return
  if (!navigator.onLine) {
    error.value = 'Registrar personal desde el panel requiere conexión a internet.'
    window.scrollTo({ top: 0, behavior: 'smooth' })
    return
  }
  try {
    error.value = ''
    if (editandoPersonal.value) {
      await api.put(`/api/personal/${formPersonal.value.id}`, formPersonal.value)
      mensaje.value = 'Trabajador actualizado exitosamente'
    } else {
      await crearPersonalManual(formPersonal.value)
      mensaje.value = 'Trabajador registrado exitosamente'
    }
    cancelarEdicionPersonal()
    await cargarDatos()
    setTimeout(() => mensaje.value = '', 3000)
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Error al guardar el trabajador'
  }
}

async function desactivarPersonal(id: string) {
  if (!navigator.onLine) {
    error.value = 'Desactivar trabajadores requiere conexión a internet.'
    window.scrollTo({ top: 0, behavior: 'smooth' })
    return
  }
  if (!confirm('¿Estás seguro de desactivar a este trabajador?')) return
  try {
    await api.delete(`/api/personal/${id}`)
    mensaje.value = 'Trabajador desactivado'
    await cargarDatos()
    setTimeout(() => mensaje.value = '', 3000)
  } catch (err: any) {
    error.value = err.response?.data?.message || 'Error al desactivar al trabajador'
  }
}

onMounted(() => {
  cargarDatos()
})
</script>

<template>
  <div>
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

    <div style="display:grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap:24px;">
      
      <!-- formulario de tramo -->
      <section class="panel-box filters" style="display:flex; flex-direction:column; gap:16px;">
        <div class="filter-title">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1z"/><line x1="4" y1="22" x2="4" y2="15"/></svg>
          <div><b style="text-transform:uppercase;">{{ editandoTramo ? 'EDITAR TRAMO' : 'NUEVO TRAMO' }}</b><span>Administra las rutas disponibles</span></div>
        </div>
        
        <label>CÓDIGO (Ej: SC-04)
          <input class="input-like" v-model="formTramo.codigo" placeholder="Ej. SC-04" style="width:100%;" :disabled="editandoTramo" />
        </label>
        <label>NOMBRE DEL TRAMO
          <input class="input-like" v-model="formTramo.nombre" placeholder="Ej. El Torno - La Angostura" style="width:100%;" />
        </label>
        <label>DESCRIPCIÓN
          <input class="input-like" v-model="formTramo.descripcion" placeholder="Opcional" style="width:100%;" />
        </label>
        <div style="display:grid; grid-template-columns:1fr 1fr; gap:12px;">
          <label>KM INICIAL
            <input class="input-like" v-model.number="formTramo.kmInicio" type="number" min="0" step="1" style="width:100%;" />
          </label>
          <label>KM FINAL
            <input class="input-like" v-model.number="formTramo.kmFin" type="number" min="0" step="1" style="width:100%;" />
          </label>
        </div>
        <small v-if="formTramo.kmInicio > formTramo.kmFin" style="color:var(--red);">El kilómetro inicial no puede ser mayor al final.</small>
        
        <div style="display:flex; gap:12px; margin-top:auto;">
          <button class="primary-button" style="flex:1;" @click="guardarTramo" :disabled="!formTramo.codigo || !formTramo.nombre || formTramo.kmInicio > formTramo.kmFin">
            {{ editandoTramo ? 'Actualizar' : 'Guardar' }}
          </button>
          <button v-if="editandoTramo" class="secondary-button" style="flex:1;" @click="cancelarEdicionTramo">
            Cancelar
          </button>
        </div>
      </section>

      <!-- formulario de personal -->
      <section class="panel-box filters" style="display:flex; flex-direction:column; gap:16px;">
        <div class="filter-title">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M19 8v6m-3-3h6"/></svg>
          <div><b style="text-transform:uppercase;">{{ editandoPersonal ? 'EDITAR TRABAJADOR' : 'NUEVO TRABAJADOR' }}</b><span>Registrar o actualizar personal</span></div>
        </div>
        
        <label>TRAMO ASIGNADO
          <select v-model="formPersonal.idTramo" class="input-like" style="width:100%; padding: 8px;">
            <option v-for="t in tramos" :key="t.id" :value="t.id">{{ t.codigo }} - {{ t.nombre }}</option>
          </select>
        </label>
        <label>NOMBRE COMPLETO
          <input class="input-like" v-model="formPersonal.nombreCompleto" placeholder="Ej. Juan Pérez" style="width:100%;" />
        </label>
        <label>DOCUMENTO DE IDENTIDAD (8 DÍGITOS)
          <input class="input-like" v-model="formPersonal.documento" placeholder="Ej. 12345678" style="width:100%;" />
          <small v-if="formPersonal.documento && !ciValido" style="color:var(--red); font-size:10px;">Debe tener exactamente 8 dígitos numéricos</small>
        </label>
        <label>CARGO (OPCIONAL)
          <input class="input-like" v-model="formPersonal.cargo" placeholder="Ej. Peón" style="width:100%;" />
        </label>
        
        <div style="display:flex; gap:12px; margin-top:auto;">
          <button class="primary-button" style="flex:1;" @click="guardarPersonal" :disabled="!formPersonal.nombreCompleto.trim() || !ciValido || !formPersonal.idTramo">
            {{ editandoPersonal ? 'Actualizar' : 'Registrar' }}
          </button>
          <button v-if="editandoPersonal" class="secondary-button" style="flex:1;" @click="cancelarEdicionPersonal">
            Cancelar
          </button>
        </div>
      </section>
      
    </div>

    <div style="margin-top: 32px; display:grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap:24px;">
      
      <!-- tramos -->
      <section class="panel-box filters">
        <div class="filter-title" style="margin-bottom: 16px;">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><line x1="8" y1="6" x2="21" y2="6"/><line x1="8" y1="12" x2="21" y2="12"/><line x1="8" y1="18" x2="21" y2="18"/><line x1="3" y1="6" x2="3.01" y2="6"/><line x1="3" y1="12" x2="3.01" y2="12"/><line x1="3" y1="18" x2="3.01" y2="18"/></svg>
          <div><b>TRAMOS ACTIVOS</b><span>({{ tramos.length }})</span></div>
        </div>
        <div style="display:flex; flex-direction:column; gap:8px; max-height:400px; overflow-y:auto; padding-right:8px;">
          <div v-for="t in tramos" :key="t.id" style="padding:12px; border:1px solid #eee; border-radius:6px; display:flex; justify-content:space-between; align-items:center;">
            <div>
              <strong style="display:block; font-size:13px;">{{ t.codigo }}</strong>
              <span style="font-size:12px; color:#666;">{{ t.nombre }} · Km {{ t.kmInicio }} - {{ t.kmFin }}</span>
            </div>
            <div style="display:flex; gap:8px;">
              <button @click="cargarParaEditarTramo(t)" style="background:none; border:none; color:var(--blue); cursor:pointer;">Editar</button>
              <button v-if="esAdmin" @click="desactivarTramo(t.id)" style="background:none; border:none; color:var(--red); cursor:pointer;">Eliminar</button>
            </div>
          </div>
          <div v-if="tramos.length === 0" style="font-size:12px; color:#999;">No hay tramos activos.</div>
        </div>
      </section>

      <!-- personal -->
      <section class="panel-box filters">
        <div class="filter-title" style="margin-bottom: 16px;">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M23 21v-2a4 4 0 0 0-3-3.87"/><path d="M16 3.13a4 4 0 0 1 0 7.75"/></svg>
          <div><b>TRABAJADORES ACTIVOS</b><span>({{ personal.length }})</span></div>
        </div>
        <div style="display:flex; flex-direction:column; gap:8px; max-height:400px; overflow-y:auto; padding-right:8px;">
          <div v-for="p in personal" :key="p.id" style="padding:12px; border:1px solid #eee; border-radius:6px; display:flex; justify-content:space-between; align-items:center;">
            <div>
              <strong style="display:block; font-size:13px;">{{ p.nombreCompleto }}</strong>
              <span style="font-size:12px; color:#666;">CI: {{ p.documento }} - {{ p.tramo }}</span>
            </div>
            <div style="display:flex; gap:8px;">
              <button @click="cargarParaEditarPersonal(p)" style="background:none; border:none; color:var(--blue); cursor:pointer;">Editar</button>
              <button v-if="esAdmin" @click="desactivarPersonal(p.id)" style="background:none; border:none; color:var(--red); cursor:pointer;">Eliminar</button>
            </div>
          </div>
          <div v-if="personal.length === 0" style="font-size:12px; color:#999;">No hay personal activo.</div>
        </div>
      </section>

    </div>
  </div>
</template>
