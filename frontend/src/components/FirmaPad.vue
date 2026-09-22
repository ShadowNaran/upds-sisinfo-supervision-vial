<script setup lang="ts">
import { onMounted, ref } from 'vue'

const emit = defineEmits<{ firma: [valor: string] }>()
const canvas = ref<HTMLCanvasElement | null>(null)
let dibujando = false
let tieneTrazo = false

function punto(event: PointerEvent) {
  const elemento = canvas.value!
  const rect = elemento.getBoundingClientRect()
  return { x: (event.clientX - rect.left) * (elemento.width / rect.width), y: (event.clientY - rect.top) * (elemento.height / rect.height) }
}
function iniciar(event: PointerEvent) {
  dibujando = true; tieneTrazo = true; canvas.value?.setPointerCapture(event.pointerId)
  const ctx = canvas.value?.getContext('2d'); if (!ctx) return
  const p = punto(event); ctx.beginPath(); ctx.moveTo(p.x, p.y)
}
function mover(event: PointerEvent) {
  if (!dibujando) return
  const ctx = canvas.value?.getContext('2d'); if (!ctx) return
  const p = punto(event); ctx.lineTo(p.x, p.y); ctx.stroke()
}
function terminar() { dibujando = false }
function limpiar() { const ctx = canvas.value?.getContext('2d'); if (ctx && canvas.value) ctx.clearRect(0, 0, canvas.value.width, canvas.value.height); tieneTrazo = false }
function guardar() { if (tieneTrazo && canvas.value) emit('firma', canvas.value.toDataURL('image/png')) }
onMounted(() => { const ctx = canvas.value?.getContext('2d'); if (ctx) { ctx.strokeStyle = '#23282E'; ctx.lineWidth = 3; ctx.lineCap = 'round' } })
</script>

<template>
  <div class="firma">
    <canvas ref="canvas" width="720" height="240" aria-label="Área para firmar" @pointerdown="iniciar" @pointermove="mover" @pointerup="terminar" @pointerleave="terminar" />
    <div class="firma__acciones"><button type="button" @click="limpiar">Limpiar</button><button type="button" class="firma__guardar" @click="guardar">Guardar firma</button></div>
  </div>
</template>

<style scoped>
.firma { padding: .75rem; background: var(--hogar); border: 1px solid var(--linea); border-radius: var(--radio-sm); }.firma canvas { display: block; width: 100%; height: auto; touch-action: none; background: white; border: 1px dashed var(--linea); }.firma__acciones { display: flex; justify-content: flex-end; gap: .5rem; margin-top: .6rem; }.firma button { border: 1px solid var(--linea); border-radius: var(--radio-sm); padding: .55rem .8rem; cursor: pointer; font: 600 .8rem var(--font-cuerpo); }.firma__guardar { color: white; background: var(--naranja-obra); }
</style>