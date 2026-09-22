<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import ConnectionBadge from '@/components/ConnectionBadge.vue'
import FirmaPad from '@/components/FirmaPad.vue'
import { actualizarPlanilla, cargarUltimaPlanillaLocal, cerrarPlanilla, crearPlanilla, crearPlanillaLocal, firmarPlanilla, guardarPlanillaLocal, listarPersonal, listarTramos } from '@/services/campo'
import { guardarEnCola, generarId } from '@/services/pendingSync'
import type { EstadoAsistencia, Personal, Planilla, Tramo } from '@/types/campo'

const auth = useAuthStore()
const tramos = ref<Tramo[]>([])
const personal = ref<Personal[]>([])
const planilla = ref<Planilla | null>(null)
const tramoId = ref('')
const cargando = ref(true)
const guardando = ref(false)
const mensaje = ref('')
const error = ref('')
const mostrarFirma = ref(false)
const resumen = computed(() => {
  const detalles = planilla.value?.detalles ?? []
  return { presentes: detalles.filter((d) => d.estado === 'Presente').length, faltas: detalles.filter((d) => d.estado === 'Falta').length, pendientes: detalles.filter((d) => d.estado === 'NoDisponible').length }
})

async function cargarPersonal() {
  if (!tramoId.value) return
  personal.value = await listarPersonal(tramoId.value)
  planilla.value = null
}
async function cargarTramos() {
  try { tramos.value = await listarTramos(); tramoId.value = tramos.value[0]?.id ?? ''; await cargarPersonal() }
  catch { error.value = 'No se pudieron cargar los tramos. Verifica la conexión.' }
  finally { cargando.value = false }
}
async function iniciarPlanilla() {
  try {
    planilla.value = navigator.onLine ? await crearPlanilla(tramoId.value) : crearPlanillaLocal(tramoId.value, personal.value)
    mensaje.value = navigator.onLine ? 'Planilla creada. Marca la asistencia de cada persona.' : 'Planilla local creada sin conexión.'
  } catch { error.value = 'No se pudo crear la planilla. Puede que ya exista una para hoy.' }
}
async function guardar() {
  if (!planilla.value) return
  guardando.value = true
  try { planilla.value = await actualizarPlanilla(planilla.value); guardarPlanillaLocal(planilla.value); mensaje.value = 'Asistencia guardada correctamente.' }
  catch {
    if (!navigator.onLine) {
      await guardarEnCola({ id: generarId(), tipo: 'planilla', payload: planilla.value, estado: 'pendiente', prioridad: 'alta', creadoEn: Date.now(), reintentos: 0 })
      guardarPlanillaLocal(planilla.value)
      mensaje.value = 'Sin conexión: asistencia guardada localmente y pendiente de sincronizar.'
    } else error.value = 'No se pudo guardar la asistencia.'
  }
  finally { guardando.value = false }
}
async function guardarFirma(valor: string) {
  if (!planilla.value) return
  try { await firmarPlanilla(planilla.value.id, valor); planilla.value.tieneFirma = true; mostrarFirma.value = false; mensaje.value = 'Firma guardada correctamente.' }
  catch { error.value = 'No se pudo guardar la firma.' }
}
async function cerrar() {
  if (!planilla.value) return
  try { planilla.value = await cerrarPlanilla(planilla.value.id); mensaje.value = 'Planilla cerrada correctamente.' }
  catch { error.value = 'La planilla debe tener personal antes de cerrarse.' }
}
function marcar(id: string, estado: EstadoAsistencia) {
  const detalle = planilla.value?.detalles.find((d) => d.idPersonal === id)
  if (detalle) { detalle.estado = estado; guardarPlanillaLocal(planilla.value!) }
}
function adjuntarFoto(id: string, event: Event) {
  const archivo = (event.target as HTMLInputElement).files?.[0]
  const detalle = planilla.value?.detalles.find((d) => d.idPersonal === id)
  if (!archivo || !detalle) return
  const lector = new FileReader()
  lector.onload = () => { detalle.fotoBase64 = typeof lector.result === 'string' ? lector.result : undefined; guardarPlanillaLocal(planilla.value!) }
  lector.readAsDataURL(archivo)
}
onMounted(async () => {
  await cargarTramos()
  if (tramoId.value && !navigator.onLine) {
    const local = cargarUltimaPlanillaLocal(tramoId.value)
    if (local) planilla.value = local
  }
})
</script>

<template>
  <div class="campo">
    <header class="campo__header">
      <div><strong class="campo__marca">AROOMAF</strong><span class="campo__modulo">Modulo de campo</span></div>
      <div class="campo__derecha"><ConnectionBadge /><span class="campo__usuario">{{ auth.perfil?.nombre }}</span><button class="campo__logout" @click="auth.logout">Cerrar sesión</button></div>
    </header>
    <main class="campo__contenido">
      <section class="campo__intro"><p class="campo__eyebrow">CONTROL DIARIO</p><h1>Asistencia de cuadrilla</h1><p>Selecciona tu tramo y registra el estado de cada persona.</p></section>
      <section class="campo__toolbar">
        <label for="tramo">Tramo de trabajo</label>
        <select id="tramo" v-model="tramoId" :disabled="cargando" @change="cargarPersonal"><option v-for="tramo in tramos" :key="tramo.id" :value="tramo.id">{{ tramo.codigo }} · {{ tramo.nombre }}</option></select>
        <button class="campo__principal" :disabled="!tramoId || !!planilla" @click="iniciarPlanilla">Abrir planilla de hoy</button>
      </section>
      <p v-if="cargando" class="campo__estado">Cargando información...</p><p v-if="mensaje" class="campo__mensaje">{{ mensaje }}</p><p v-if="error" class="campo__error">{{ error }}</p>
      <section v-if="planilla" class="campo__planilla">
        <div class="campo__resumen"><span><b>{{ resumen.presentes }}</b> presentes</span><span><b>{{ resumen.faltas }}</b> faltas</span><span><b>{{ resumen.pendientes }}</b> sin marcar</span></div>
        <article v-for="detalle in planilla.detalles" :key="detalle.id" class="persona">
          <div><strong>{{ detalle.personal }}</strong><small>{{ personal.find((p) => p.id === detalle.idPersonal)?.cargo || 'Personal asignado' }}</small>
            <label>Clasificación <select v-model="detalle.clasificacion"><option value="">Sin clasificar</option><option value="Normal">Normal</option><option value="Observado">Observado</option><option value="Crítico">Crítico</option></select></label>
            <label class="persona__foto">{{ detalle.fotoBase64 ? 'Foto adjunta' : 'Adjuntar foto' }}<input type="file" accept="image/*" capture="environment" @change="adjuntarFoto(detalle.idPersonal, $event)" /></label>
          </div><div class="persona__acciones"><button :class="{ activo: detalle.estado === 'Presente' }" class="estado estado--presente" @click="marcar(detalle.idPersonal, 'Presente')">Presente</button><button :class="{ activo: detalle.estado === 'Falta' }" class="estado estado--falta" @click="marcar(detalle.idPersonal, 'Falta')">Falta</button><button :class="{ activo: detalle.estado === 'NoDisponible' }" class="estado estado--nd" @click="marcar(detalle.idPersonal, 'NoDisponible')">No disponible</button></div>
        </article>
        <button class="campo__guardar" :disabled="guardando || planilla.estado === 'Cerrada'" @click="guardar">{{ guardando ? 'Guardando...' : 'Guardar asistencia' }}</button>
        <div v-if="planilla.estado !== 'Cerrada'" class="campo__finalizar">
          <button class="campo__firma" @click="mostrarFirma = !mostrarFirma">{{ planilla.tieneFirma ? 'Actualizar firma' : 'Firmar planilla' }}</button>
          <button class="campo__cerrar" :disabled="!planilla.tieneFirma" @click="cerrar">Cerrar planilla</button>
        </div>
        <FirmaPad v-if="mostrarFirma" @firma="guardarFirma" />
        <p v-if="planilla.estado === 'Cerrada'" class="campo__mensaje">Planilla cerrada. Ya no se puede modificar.</p>
      </section>
      <p v-else-if="!cargando && !personal.length" class="campo__estado">No hay personal activo asignado a este tramo.</p>
    </main>
  </div>
</template>

<style scoped>
.campo { min-height: 100dvh; background: var(--hormigon); color: var(--asfalto); }.campo__header { display:flex; justify-content:space-between; align-items:center; gap:1rem; flex-wrap:wrap; padding:1rem 1.5rem; background:var(--asfalto); color:var(--hogar); }.campo__marca { display:block; font:700 1.4rem var(--font-titulo); color:var(--amarillo-ruta); }.campo__modulo { font-size:.8rem; text-transform:uppercase; }.campo__derecha { display:flex; align-items:center; gap:.75rem; flex-wrap:wrap; }.campo__usuario { font-size:.85rem; }.campo__logout,.campo__principal,.campo__guardar,.estado { border:0; border-radius:var(--radio-sm); cursor:pointer; font:600 .85rem var(--font-cuerpo); padding:.65rem .9rem; }.campo__logout { background:var(--rojo-senal); color:white; }.campo__contenido { width:min(100% - 2rem,58rem); margin:0 auto; padding:2rem 0 4rem; }.campo__eyebrow { margin:0; color:var(--naranja-obra); font-size:.75rem; font-weight:700; letter-spacing:.08em; }h1 { margin:.2rem 0 .4rem; font:700 clamp(1.8rem,5vw,2.8rem) var(--font-titulo); }.campo__toolbar { display:grid; grid-template-columns:1fr auto; gap:.65rem; margin:1.5rem 0; padding:1rem; background:var(--hogar); border-left:.3rem solid var(--amarillo-ruta); box-shadow:var(--sombra-sm); }.campo__toolbar label { grid-column:1 / -1; font-weight:700; font-size:.85rem; }select { width:100%; border:1px solid var(--linea); border-radius:var(--radio-sm); padding:.7rem; background:var(--hogar); font:inherit; }.campo__principal,.campo__guardar { background:var(--naranja-obra); color:white; }.campo__estado,.campo__mensaje,.campo__error { padding:.8rem 1rem; background:var(--hogar); border-radius:var(--radio-sm); }.campo__mensaje { color:var(--verde-senal); }.campo__error { color:var(--rojo-senal); }.campo__resumen { display:flex; gap:1rem; flex-wrap:wrap; margin-bottom:.75rem; }.campo__resumen b { font-size:1.2rem; }.persona { display:flex; justify-content:space-between; align-items:center; gap:1rem; margin:.65rem 0; padding:1rem; background:var(--hogar); box-shadow:var(--sombra-sm); }.persona strong,.persona small { display:block; }.persona small { color:color-mix(in srgb,var(--asfalto) 65%,transparent); }.persona__acciones { display:flex; gap:.35rem; flex-wrap:wrap; justify-content:flex-end; }.estado { background:var(--hormigon); color:var(--asfalto); border:1px solid var(--linea); }.estado.activo { color:white; }.estado--presente.activo { background:var(--verde-senal); }.estado--falta.activo { background:var(--rojo-senal); }.estado--nd.activo { background:var(--naranja-obra); }.campo__guardar { width:100%; margin-top:1rem; }@media (max-width:650px) { .campo__toolbar { grid-template-columns:1fr; }.persona { align-items:stretch; flex-direction:column; }.persona__acciones { justify-content:stretch; }.estado { flex:1; } }
</style>
