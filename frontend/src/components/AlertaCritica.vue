<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { onAlertaCritica } from '@/services/signalr'

const abiertas = ref<any[]>([])

onMounted(() => {
  onAlertaCritica((alerta) => {
    abiertas.value.push({ ...alerta, _id: Date.now() })
  })
})

function descartar(id: number) {
  abiertas.value = abiertas.value.filter(a => a._id !== id)
}

function descartarTodas() {
  abiertas.value = []
}

const alertasOrdenadas = computed(() => {
  return [...abiertas.value].sort((a, b) => {
    if (a.hora !== b.hora) return b.hora.localeCompare(a.hora) // primero las mas recientes
    return (a.kilometraje || 0) - (b.kilometraje || 0)
  })
})

const alertasVisibles = computed(() => alertasOrdenadas.value.slice(0, 5))
const alertasExtra = computed(() => Math.max(0, alertasOrdenadas.value.length - 5))
</script>

<template>
  <div class="alertas-criticas" v-if="abiertas.length > 0">
    
    <div class="alertas-criticas__header" v-if="alertasExtra > 0">
      <span class="contador">⚠️ +{{ alertasExtra }} alertas esperando revisión</span>
      <button class="btn-descartar-todas" @click="descartarTodas">Descartar todas</button>
    </div>

    <div v-for="alerta in alertasVisibles" :key="alerta._id" class="alerta-critica">
      <div class="alerta-critica__header">
        <span class="alerta-critica__icono">⚠️</span>
        <h3>Alerta Crítica - {{ alerta.severidad }}</h3>
        <button class="btn-descartar" @click="descartar(alerta._id)">✕</button>
      </div>
      <div class="alerta-critica__body">
        <div class="alerta-critica__datos">
          <p><strong>Tramo:</strong> {{ alerta.tramo }}</p>
          <p><strong>Kilómetro:</strong> {{ alerta.kilometraje?.toFixed(3) || 'Desconocido' }}</p>
          <p><strong>Hora de reporte:</strong> {{ alerta.hora }}</p>
        </div>
        <div class="alerta-critica__foto" v-if="alerta.fotoBase64">
          <img :src="alerta.fotoBase64" alt="Evidencia de alerta" />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.alertas-criticas {
  position: fixed;
  bottom: 2rem;
  right: 2rem;
  z-index: 9999;
  display: flex;
  flex-direction: column;
  gap: 1rem;
  align-items: flex-end;
}

.alertas-criticas__header {
  background: white;
  padding: 0.5rem 1rem;
  border-radius: var(--radio-md);
  box-shadow: var(--sombra-md);
  display: flex;
  align-items: center;
  gap: 1rem;
  border: 1px solid var(--rojo-senal);
  animation: slideUp 0.3s ease-out;
}

.contador {
  font-weight: 600;
  color: var(--rojo-senal);
  font-size: 0.9rem;
}

.btn-descartar-todas {
  background: var(--asfalto);
  color: white;
  border: none;
  padding: 0.3rem 0.8rem;
  border-radius: var(--radio-sm);
  cursor: pointer;
  font-size: 0.8rem;
  font-weight: 600;
}

.alerta-critica {
  background: white;
  border: 2px solid var(--rojo-senal);
  border-radius: var(--radio-md);
  box-shadow: 0 10px 25px rgba(220, 38, 38, 0.4);
  width: 380px;
  max-width: 90vw;
  overflow: hidden;
  animation: slideUp 0.3s ease-out;
}

@keyframes slideUp {
  from { transform: translateY(100%); opacity: 0; }
  to { transform: translateY(0); opacity: 1; }
}

.alerta-critica__header {
  background: var(--rojo-senal);
  color: white;
  padding: 0.75rem 1rem;
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.alerta-critica__header h3 {
  margin: 0;
  font-size: 1.1rem;
  flex: 1;
}

.btn-descartar {
  background: none;
  border: none;
  color: white;
  font-size: 1.2rem;
  cursor: pointer;
  opacity: 0.8;
}
.btn-descartar:hover {
  opacity: 1;
}

.alerta-critica__body {
  padding: 1rem;
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.alerta-critica__datos p {
  margin: 0.2rem 0;
  font-size: 0.95rem;
  color: var(--asfalto);
}

.alerta-critica__foto img {
  width: 100%;
  max-height: 200px;
  object-fit: cover;
  border-radius: var(--radio-sm);
  border: 1px solid var(--linea);
}
</style>
