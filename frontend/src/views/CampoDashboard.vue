<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import FirmaPad from '@/components/FirmaPad.vue'
import { actualizarPlanilla, cargarUltimaPlanillaLocal, cerrarPlanilla, crearPlanilla, crearPlanillaLocal, firmarPlanilla, guardarPlanillaLocal, listarPersonal, listarTramos, crearPersonalManual } from '@/services/campo'
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
const mostrarFormNuevo = ref(false)
const guardandoNuevo = ref(false)
const nuevoPersonal = ref({ nombreCompleto: '', documento: '', cargo: '', telefono: '' })

const resumen = computed(() => {
  const detalles = planilla.value?.detalles ?? []
  return { presentes: detalles.filter((d) => d.estado === 'Presente').length, faltas: detalles.filter((d) => d.estado === 'Falta').length, pendientes: detalles.filter((d) => d.estado === 'NoDisponible').length }
})

const esPersonal = computed(() => auth.perfil?.rol === 'PersonalMicroempresa')
const ciValido = computed(() => /^[0-9]{8}$/.test(nuevoPersonal.value.documento))
const isOnline = computed(() => navigator.onLine)

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
  const faltaFoto = planilla.value.detalles.some(d => d.estado === 'Presente' && !d.fotoBase64)
  if (faltaFoto) {
    error.value = 'Debes adjuntar foto como evidencia para todo el personal marcado como Presente.'
    return
  }
  const sinClasificar = planilla.value.detalles.some(d => d.severidad === undefined)
  if (sinClasificar) {
    error.value = 'Debes clasificar la severidad (transitabilidad) en todos los registros antes de guardar.'
    return
  }
  
  error.value = ''
  guardando.value = true
  planilla.value.timestampLocal = Date.now()
  try { planilla.value = await actualizarPlanilla(planilla.value); guardarPlanillaLocal(planilla.value); mensaje.value = 'Asistencia guardada correctamente.' }
  catch {
    if (!navigator.onLine) {
      const esCritico = planilla.value.detalles.some(d => d.severidad === 2)
      await guardarEnCola({ id: generarId(), tipo: 'planilla', payload: planilla.value, estado: 'pendiente', prioridad: esCritico ? 'alta' : 'normal', creadoEn: Date.now(), reintentos: 0 })
      guardarPlanillaLocal(planilla.value)
      mensaje.value = 'Sin conexión: registro guardado localmente y en cola.'
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
function obtenerUbicacion(): Promise<{ lat: number; lng: number } | null> {
  return new Promise((resolve) => {
    if (!('geolocation' in navigator)) {
      console.error('Error GPS: el navegador no ofrece geolocalización.')
      error.value = 'El GPS requiere HTTPS o usar localhost.'
      return resolve(null)
    }
    if (!window.isSecureContext && location.hostname !== 'localhost') {
      console.error('Error GPS: la geolocalización requiere HTTPS o localhost.')
      error.value = 'El GPS requiere HTTPS o usar localhost.'
      return resolve(null)
    }
    navigator.geolocation.getCurrentPosition(
      (pos) => resolve({ lat: pos.coords.latitude, lng: pos.coords.longitude }),
      (reason) => {
        console.error('Error GPS:', reason.message, reason.code)
        resolve(null)
      },
      { enableHighAccuracy: false, timeout: 15000, maximumAge: 10000 }
    )
  })
}

function estamparImagen(archivo: File, lat?: number, lng?: number, km?: number): Promise<string> {
  return new Promise((resolve, reject) => {
    const lector = new FileReader()
    lector.onload = (e) => {
      const img = new Image()
      img.onload = () => {
        const canvas = document.createElement('canvas')
        const ctx = canvas.getContext('2d')
        if (!ctx) return resolve(e.target?.result as string)
        
        canvas.width = img.width
        canvas.height = img.height
        ctx.drawImage(img, 0, 0)
        
        const fecha = new Date().toLocaleString()
        const coordsStr = lat != null && lng != null ? `Lat: ${lat.toFixed(5)}, Lng: ${lng.toFixed(5)}` : 'Ubicación no disponible'
        const kmStr = km !== undefined ? ` | Km: ${km.toFixed(2)}` : ''
        const texto = `${fecha} | ${coordsStr}${kmStr}`
        
        const fontSize = Math.max(14, Math.floor(img.width * 0.035))
        ctx.font = `bold ${fontSize}px sans-serif`
        const padding = fontSize * 0.5
        
        ctx.fillStyle = 'rgba(0, 0, 0, 0.6)'
        ctx.fillRect(0, img.height - fontSize - padding * 2, canvas.width, fontSize + padding * 2)
        ctx.fillStyle = '#ffffff'
        ctx.fillText(texto, padding, img.height - padding)
        
        resolve(canvas.toDataURL('image/jpeg', 0.8))
      }
      img.onerror = reject
      img.src = e.target?.result as string
    }
    lector.onerror = reject
    lector.readAsDataURL(archivo)
  })
}

function calcularKilometraje(lat: number, lng: number, tramo: Tramo): { km: number, desviacionKm: number } | null {
  if (tramo.latitudInicio == null || tramo.longitudInicio == null || tramo.latitudFin == null || tramo.longitudFin == null) return null
  
  const haversine = (lat1: number, lon1: number, lat2: number, lon2: number) => {
    const R = 6371;
    const dLat = (lat2 - lat1) * Math.PI / 180;
    const dLon = (lon2 - lon1) * Math.PI / 180;
    const a = Math.sin(dLat/2) * Math.sin(dLat/2) + Math.cos(lat1 * Math.PI / 180) * Math.cos(lat2 * Math.PI / 180) * Math.sin(dLon/2) * Math.sin(dLon/2);
    return R * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
  }

  const x = lat, y = lng;
  const x1 = tramo.latitudInicio, y1 = tramo.longitudInicio;
  const x2 = tramo.latitudFin, y2 = tramo.longitudFin;

  const A = x - x1, B = y - y1, C = x2 - x1, D = y2 - y1;
  const dot = A * C + B * D;
  const lenSq = C * C + D * D;
  let param = -1;
  if (lenSq !== 0) param = dot / lenSq;

  let xx = x1, yy = y1;
  if (param > 1) { xx = x2; yy = y2; }
  else if (param > 0) { xx = x1 + param * C; yy = y1 + param * D; }

  const desviacionKm = haversine(x, y, xx, yy);
  const distProyectada = haversine(x1, y1, xx, yy);
  const distTotal = haversine(x1, y1, x2, y2);
  
  const kStart = tramo.kmInicio ?? 0;
  const kEnd = tramo.kmFin ?? 0;
  let km = kStart;
  if (distTotal > 0) km = kStart + (distProyectada / distTotal) * (kEnd - kStart);

  return { km, desviacionKm };
}

async function adjuntarFoto(id: string, event: Event) {
  const archivo = (event.target as HTMLInputElement).files?.[0]
  const detalle = planilla.value?.detalles.find((d) => d.idPersonal === id)
  if (!archivo || !detalle) return
  
  mensaje.value = 'Procesando foto y obteniendo ubicación...'
  const coords = await obtenerUbicacion()
  const tramoActual = tramos.value.find(t => t.id === tramoId.value)
  let kmActual: number | undefined = undefined;

  if (coords) {
    detalle.latitud = coords.lat
    detalle.longitud = coords.lng
    if (tramoActual) {
      const calculo = calcularKilometraje(coords.lat, coords.lng, tramoActual)
      if (calculo) {
        if (calculo.desviacionKm > 5) {
          error.value = 'Advertencia: La ubicación está a más de 5km de la ruta del tramo. Se registrará la incidencia.'
        }
        kmActual = calculo.km
        detalle.kilometraje = kmActual
      }
    }
  } else {
    error.value = 'Permiso de ubicación denegado o sin señal. Se guardará sin coordenadas.'
  }

  try {
    detalle.fotoBase64 = await estamparImagen(archivo, coords?.lat, coords?.lng, kmActual)
    guardarPlanillaLocal(planilla.value!)
    mensaje.value = 'Foto estampada y guardada correctamente.'
  } catch {
    error.value = 'Error al procesar la imagen.'
  }
}

async function agregarPersonalManual() {
  if (!nuevoPersonal.value.nombreCompleto || !nuevoPersonal.value.documento) return
  guardandoNuevo.value = true
  try {
    const payload = { ...nuevoPersonal.value, idTramo: tramoId.value }
    let personaCreada: Personal
    if (navigator.onLine) {
      personaCreada = await crearPersonalManual(payload)
    } else {
      const idLocal = `local-${crypto.randomUUID()}`
      personaCreada = { id: idLocal, ...payload, estadoValidacion: 'PendienteValidacion' }
      await guardarEnCola({ id: generarId(), tipo: 'personal_manual', payload: personaCreada, estado: 'pendiente', prioridad: 'normal', creadoEn: Date.now(), reintentos: 0 })
    }
    
    personal.value.push(personaCreada)
    if (planilla.value) {
      planilla.value.detalles.push({ id: `local-${crypto.randomUUID()}`, idPersonal: personaCreada.id, personal: personaCreada.nombreCompleto, estado: 'Presente' })
      guardarPlanillaLocal(planilla.value)
    }
    
    mensaje.value = 'Persona añadida localmente y marcada presente.'
    mostrarFormNuevo.value = false
    nuevoPersonal.value = { nombreCompleto: '', documento: '', cargo: '', telefono: '' }
  } catch {
    error.value = 'No se pudo agregar el trabajador manualmente.'
  } finally {
    guardandoNuevo.value = false
  }
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
  <div class="page-shell">
    <!-- encabezado -->
    <div class="page-heading">
      <div>
        <p class="eyebrow">OPERACIÓN EN TERRENO</p>
        <h1>Planilla de campo</h1>
        <p>{{ new Date().toLocaleDateString('es-BO', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }) }}</p>
      </div>
      <div>
        <button class="primary-button" :disabled="!tramoId || !!planilla" @click="iniciarPlanilla">
          ABRIR PLANILLA
        </button>
      </div>
    </div>

    <!-- tramo y acciones -->
    <section class="field-toolbar">
      <label>
        TRAMO ASIGNADO
        <select v-model="tramoId" :disabled="cargando" @change="cargarPersonal">
          <option v-for="tramo in tramos" :key="tramo.id" :value="tramo.id">{{ tramo.codigo }} · {{ tramo.nombre }}</option>
        </select>
      </label>
      
      <div class="field-sync" v-if="tramoId">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 22s8-4 8-11V5l-8-3-8 3v6c0 7 8 11 8 11zm-3-10 2 2 4-4"/></svg>
        <div>
          <b>{{ isOnline ? 'TRABAJANDO EN LÍNEA' : 'TRABAJANDO OFFLINE' }}</b>
          <small>{{ isOnline ? 'Conectado a central' : 'Los datos se sincronizarán después' }}</small>
        </div>
      </div>

      <button class="secondary-button" v-if="tramoId && !esPersonal" @click="mostrarFormNuevo = !mostrarFormNuevo">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"/><circle cx="9" cy="7" r="4"/><path d="M19 8v6m-3-3h6"/></svg>
        Añadir no listado
      </button>
    </section>

    <!-- nuevo trabajador -->
    <div v-if="mostrarFormNuevo" class="panel-box filters" style="margin-top:14px; display:block;">
      <div class="filter-title" style="margin-bottom:14px;">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><path d="M12 8v8m-4-4h8"/></svg>
        <div>
          <b>AÑADIR TRABAJADOR MANUALMENTE</b>
          <span>Quedará pendiente de validación en oficina.</span>
        </div>
      </div>
      <div style="display:flex; gap:14px; flex-wrap:wrap;">
        <label style="flex:1;">NOMBRE COMPLETO
          <input class="input-like" v-model="nuevoPersonal.nombreCompleto" placeholder="Ej. Juan Pérez" required style="width:100%; border:none; outline:none;" />
        </label>
        <label style="flex:1;">DOCUMENTO (CI)
          <input class="input-like" v-model="nuevoPersonal.documento" placeholder="Ej. 12345678" required style="width:100%; border:none; outline:none;" />
          <small v-if="nuevoPersonal.documento && !ciValido" style="color:var(--red); font-size:10px;">Debe tener exactamente 8 dígitos numéricos</small>
        </label>
        <label style="flex:1;">CARGO (OPCIONAL)
          <input class="input-like" v-model="nuevoPersonal.cargo" placeholder="Ej. Peón" style="width:100%; border:none; outline:none;" />
        </label>
      </div>
      <div style="display:flex; gap:8px; justify-content:flex-end; margin-top:14px;">
        <button class="secondary-button" @click="mostrarFormNuevo = false">Cancelar</button>
        <button class="primary-button" @click="agregarPersonalManual" :disabled="guardandoNuevo || !nuevoPersonal.nombreCompleto.trim() || !ciValido">
          {{ guardandoNuevo ? 'Guardando...' : 'Guardar y marcar presente' }}
        </button>
      </div>
    </div>

    <div v-if="cargando" style="padding: 20px; text-align: center; color: #666;">Cargando información...</div>
    <div v-else-if="error" style="padding: 20px; text-align: center; color: var(--red);">{{ error }}</div>

    <!-- planilla -->
    <template v-if="planilla">
      <!-- resumen -->
      <section class="field-summary">
        <div>
          <span class="summary-dot green" />
          <strong>{{ resumen.presentes }}</strong>
          <small>PRESENTES</small>
        </div>
        <div>
          <span class="summary-dot red" />
          <strong>{{ resumen.faltas }}</strong>
          <small>FALTAS</small>
        </div>
        <div>
          <span class="summary-dot gray" />
          <strong>{{ resumen.pendientes }}</strong>
          <small>SIN MARCAR</small>
        </div>
        <div class="summary-progress">
          <span><b>{{ resumen.presentes + resumen.faltas }}</b> MARCADOS</span>
          <span><b>{{ planilla.detalles.length }}</b> TOTAL</span>
          <div>
            <i :style="`width: ${((resumen.presentes + resumen.faltas) / (planilla.detalles.length || 1)) * 100}%`"></i>
          </div>
        </div>
      </section>

      <!-- personal -->
      <section class="field-list">
        <article class="person-card" v-for="detalle in planilla.detalles" :key="detalle.id">
          
          <div class="person-id">
            <span>{{ detalle.personal.split(' ').map((n: string) => n[0]).slice(0, 2).join('') }}</span>
            <div>
              <h3>
                {{ detalle.personal }}
                <span v-if="personal.find(p => p.id === detalle.idPersonal)?.estadoValidacion === 'PendienteValidacion'" 
                      style="font-size:8px; background:#fff2bf; color:#785d05; padding:2px 6px; border-radius:99px; margin-left:6px; font-weight:700;">
                  PENDIENTE VALIDAR
                </span>
              </h3>
              <p>{{ personal.find((p) => p.id === detalle.idPersonal)?.cargo || 'Personal asignado' }}</p>
            </div>
          </div>
          <!-- severidad -->
          <div class="field-control">
            <label>NIVEL DE SEVERIDAD (TRANSITABILIDAD)</label>
            <div class="segmented">
              <button :disabled="esPersonal" :class="{'selected-green': detalle.severidad === 0}" @click="detalle.severidad = 0; guardarPlanillaLocal(planilla!)">Transita</button>
              <button :disabled="esPersonal" :class="{'selected-yellow': detalle.severidad === 1}" @click="detalle.severidad = 1; guardarPlanillaLocal(planilla!)">Precaución</button>
              <button :disabled="esPersonal" :class="{'selected-red': detalle.severidad === 2}" @click="detalle.severidad = 2; guardarPlanillaLocal(planilla!)">No transita</button>
            </div>
          </div>

          <!-- clasificacion y kilometraje -->
          <div class="field-control" v-if="detalle.estado === 'Presente'">
            <div class="attendance-detail-fields">
              <div class="work-type-control">
                <label>TIPO DE TRABAJO</label>
                <select class="input-like" v-model="detalle.clasificacion" @change="guardarPlanillaLocal(planilla!)" style="width:100%; padding:8px; font-size:11px;">
                  <option value="">(Ninguno)</option>
                  <option value="Limpieza">Limpieza de vía</option>
                  <option value="Bacheo">Bacheo</option>
                  <option value="Desbroce">Desbroce</option>
                  <option value="Derrumbe">Atención a derrumbe</option>
                  <option value="Otro">Otro / Eventualidad</option>
                </select>
              </div>
              <div class="kilometer-control">
                <label>KM</label>
                <input type="number" class="input-like" v-model="detalle.kilometraje" @input="guardarPlanillaLocal(planilla!)" placeholder="Ej. 45" style="width:100%; padding:8px; font-size:11px;" step="0.1" />
              </div>
            </div>
          </div>
          <div v-else></div> <!-- mantiene la alineacion de la cuadricula -->

          <!-- asistencia -->
          <div class="field-control">
            <label>ESTADO DE ASISTENCIA</label>
            <div class="segmented">
              <button :disabled="esPersonal" :class="{'selected-green': detalle.estado === 'Presente'}" @click="marcar(detalle.idPersonal, 'Presente')">Presente</button>
              <button :disabled="esPersonal" :class="{'selected-red': detalle.estado === 'Falta'}" @click="marcar(detalle.idPersonal, 'Falta')">Falta</button>
              <button :disabled="esPersonal" :class="{'selected-yellow': detalle.estado === 'NoDisponible'}" @click="marcar(detalle.idPersonal, 'NoDisponible')">N/D</button>
            </div>
          </div>

          <!-- evidencia fotografica -->
          <div class="field-control" v-if="detalle.estado === 'Presente'">
            <label>FOTO EVIDENCIA</label>
            <label class="photo-button" style="cursor:pointer;" :style="detalle.fotoBase64 ? 'border-style:solid; border-color:#78c89e; background:#e5f6ed;' : ''">
              <svg v-if="!detalle.fotoBase64" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M14.5 4h-5L7 7H4a2 2 0 0 0-2 2v9a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2V9a2 2 0 0 0-2-2h-3l-2.5-3z"/><circle cx="12" cy="13" r="3"/></svg>
              <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="#0e7c43" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
              <b :style="detalle.fotoBase64 ? 'color:#0e7c43' : ''">{{ detalle.fotoBase64 ? 'VER / CAMBIAR' : 'TOMAR FOTO' }}</b>
              <small :style="detalle.fotoBase64 ? 'color:#0e7c43' : ''">{{ detalle.fotoBase64 ? (detalle.latitud ? 'Con coordenadas' : 'Sin coordenadas') : 'Requerido' }}</small>
              <input type="file" accept="image/*" capture="environment" @change="adjuntarFoto(detalle.idPersonal, $event)" style="display:none;" :disabled="esPersonal" />
            </label>
          </div>
          <div class="field-control" v-else>
            <label>FOTO EVIDENCIA</label>
            <div class="photo-button" style="opacity:0.6; cursor:not-allowed; background:#f4f5f4; justify-content:center; padding:12px;">
              <small>No requerida ({{ detalle.estado }})</small>
            </div>
          </div>

          <!-- vista previa -->
          <div v-if="detalle.fotoBase64 && detalle.estado === 'Presente'" style="width: 48px; height: 48px; border-radius:6px; overflow:hidden; border:1px solid var(--linea);">
            <img :src="detalle.fotoBase64" style="width:100%; height:100%; object-fit:cover;" />
          </div>
          <div v-else style="width: 48px; height: 48px;"></div>
        </article>
      </section>

      <!-- guardar y firmar -->
      <section v-if="!esPersonal" style="display:flex; justify-content:space-between; align-items:center; margin-top:24px; padding-top:24px; border-top:1px solid var(--linea);">
        <button class="primary-button" :disabled="guardando || planilla.estado === 'Cerrada'" @click="guardar" style="height:48px; padding:0 32px;">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M19 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h11l5 5v11a2 2 0 0 1-2 2z"/><polyline points="17 21 17 13 7 13 7 21"/><polyline points="7 3 7 8 15 8"/></svg>
          {{ guardando ? 'GUARDANDO...' : 'GUARDAR ASISTENCIA' }}
        </button>

        <div v-if="planilla.estado !== 'Cerrada'" style="display:flex; gap:12px;">
          <button class="secondary-button" @click="mostrarFirma = !mostrarFirma">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 20h9"/><path d="M16.5 3.5a2.121 2.121 0 0 1 3 3L7 19l-4 1 1-4L16.5 3.5z"/></svg>
            {{ planilla.tieneFirma ? 'Actualizar firma' : 'Firmar planilla' }}
          </button>
          <button class="secondary-button" :disabled="!planilla.tieneFirma" @click="cerrar" :style="planilla.tieneFirma ? 'color:var(--red); border-color:#e0aaa6;' : ''">
            Cerrar planilla
          </button>
        </div>
      </section>
      
      <FirmaPad v-if="mostrarFirma" @firma="guardarFirma" style="margin-top:24px;" />
      
      <div v-if="mensaje" class="toast">
        <div class="toast-icon" style="background:#e5f6ed; color:var(--green);">
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
        </div>
        <div>
          <b style="color:var(--green)">Éxito</b>
          <p>{{ mensaje }}</p>
        </div>
        <button class="toast-close" @click="mensaje = ''">✕</button>
      </div>

    </template>
    
    <div v-else-if="!cargando && !personal.length" style="padding: 20px; text-align: center; color: #666; background:white; border-radius:10px; border:1px dashed var(--linea); margin-top:20px;">
      No hay personal activo asignado a este tramo.
    </div>
  </div>
</template>

<style scoped>
/* estilos globales */
</style>
