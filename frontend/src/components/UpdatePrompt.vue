<script setup lang="ts">
import { onMounted, ref, shallowRef } from 'vue'
import type { RegisterSWOptions } from 'virtual:pwa-register'

const show = ref(false)
const needRefresh = shallowRef(false)
const offlineReady = ref(false)

let updateSW: (() => Promise<void>) | undefined

onMounted(async () => {
  if (import.meta.env.PROD) {
    const mod = await import('virtual:pwa-register')
    const options: RegisterSWOptions = {
      immediate: true,
      onNeedRefresh() {
        needRefresh.value = true
        show.value = true
      },
      onOfflineReady() {
        offlineReady.value = true
        show.value = true
      },
    }
    updateSW = mod.registerSW(options)
  }
})

function actualizar() {
  if (updateSW) {
    updateSW().then(() => {
      window.location.reload()
    })
  }
}

function descartar() {
  show.value = false
  needRefresh.value = false
  offlineReady.value = false
}
</script>

<template>
  <transition name="slide-up">
    <div v-if="show" class="update-prompt" role="alertdialog" aria-modal="true" aria-labelledby="update-title">
      <div class="update-prompt__contenido">
        <div class="update-prompt__icono" aria-hidden="true">Actualizar</div>
        <h3 id="update-title" class="update-prompt__titulo">
          <span v-if="needRefresh">Nueva version disponible</span>
          <span v-else-if="offlineReady">Listo para uso offline</span>
        </h3>
        <p class="update-prompt__texto">
          <span v-if="needRefresh">
            Se ha descargado una nueva version de AROOMAF.
            Pulsa "Actualizar" para recargar y disfrutar de las mejoras.
          </span>
          <span v-else-if="offlineReady">
            La aplicacion ya esta lista para funcionar sin conexion.
          </span>
        </p>
        <div class="update-prompt__acciones">
          <button v-if="needRefresh" class="update-prompt__btn update-prompt__btn--primario" @click="actualizar">
            Actualizar ahora
          </button>
          <button class="update-prompt__btn update-prompt__btn--secundario" @click="descartar">
            Luego
          </button>
        </div>
      </div>
    </div>
  </transition>
</template>

<style scoped>
.update-prompt {
  position: fixed;
  bottom: 1.5rem;
  left: 1.5rem;
  right: 1.5rem;
  max-width: 28rem;
  margin: 0 auto;
  z-index: 1000;
  animation: slideUp 0.3s ease;
}

@keyframes slideUp {
  from {
    opacity: 0;
    transform: translateY(1rem);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.update-prompt__contenido {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding: 1.25rem;
  background: var(--asfalto);
  color: var(--hogar);
  border-radius: var(--radio-lg);
  border: 0.14rem solid var(--amarillo-ruta);
  box-shadow: var(--sombra-lg);
}

.update-prompt__icono {
  font-size: 2rem;
  line-height: 1;
  text-align: center;
}

.update-prompt__titulo {
  margin: 0;
  font: 700 1.1rem var(--font-titulo);
  text-align: center;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: var(--amarillo-ruta);
}

.update-prompt__texto {
  margin: 0;
  font-size: 0.95rem;
  line-height: 1.5;
  text-align: center;
  color: color-mix(in srgb, var(--hogar) 92%, transparent);
}

.update-prompt__acciones {
  display: flex;
  gap: 0.75rem;
  margin-top: 0.25rem;
}

.update-prompt__btn {
  flex: 1;
  padding: 0.75rem 1rem;
  font: 600 0.9rem var(--font-cuerpo);
  border: 0;
  border-radius: var(--radio-md);
  cursor: pointer;
  transition: background var(--trans-rapida), transform var(--trans-rapida);
}

.update-prompt__btn--primario {
  color: var(--hogar);
  background: var(--naranja-obra);
}

.update-prompt__btn--primario:hover {
  background: var(--naranja-obra-oscuro);
}

.update-prompt__btn--secundario {
  color: var(--asfalto);
  background: var(--hormigon);
  border: 0.14rem solid var(--linea);
}

.update-prompt__btn--secundario:hover {
  background: color-mix(in srgb, var(--asfalto) 6%, transparent);
}

.update-prompt__btn:focus-visible {
  outline: 0.2rem solid var(--amarillo-ruta);
  outline-offset: 0.15rem;
}

@media (min-width: 600px) {
  .update-prompt {
    bottom: 2rem;
    left: auto;
    right: 2rem;
    max-width: 32rem;
  }
}
</style>