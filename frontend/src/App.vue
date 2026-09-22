<script setup lang="ts">
import { onMounted, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConnection } from '@/composables/useConnection'
import AppBrand from '@/components/AppBrand.vue'
import ConnectionBadge from '@/components/ConnectionBadge.vue'
import UpdatePrompt from '@/components/UpdatePrompt.vue'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()
useConnection() // inicializa listeners conexion y pendientes para ConnectionBadge

onMounted(async () => {
  await auth.init()
})

const esLogin = computed(() => route.name === 'login')
const esCampo = computed(() => route.name === 'campo')
const esPanel = computed(() => route.name === 'panel')

const rolLabel = computed(() => {
  if (!auth.perfil) return ''
  switch (auth.perfil.rol) {
    case 'Administrador': return 'Administrador'
    case 'SupervisorCampo': return 'Supervisor'
    case 'PersonalMicroempresa': return 'Personal'
    default: return auth.perfil.rol
  }
})

async function cerrarSesion() {
  await auth.logout()
  await router.push('/login')
}
</script>

<template>
  <div class="app">
    <header class="app__header" v-if="!esLogin">
      <div class="app__header-izq">
        <AppBrand compacto />
        <span class="app__titulo-ruta" v-if="esCampo">Modulo de campo</span>
        <span class="app__titulo-ruta" v-if="esPanel">Panel de oficina</span>
      </div>
      <div class="app__header-der">
        <ConnectionBadge />
        <div class="app__usuario" v-if="auth.perfil">
          <span class="app__nombre">{{ auth.perfil.nombre }}</span>
          <span class="app__rol">{{ rolLabel }}</span>
        </div>
        <button v-if="auth.perfil" class="app__logout" @click="cerrarSesion" aria-label="Cerrar sesion">
          Cerrar sesion
        </button>
      </div>
    </header>

    <main class="app__main">
      <RouterView v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </RouterView>
    </main>

    <UpdatePrompt />
  </div>
</template>

<style scoped>
.app {
  min-height: 100dvh;
  display: flex;
  flex-direction: column;
  background: var(--hormigon);
}

.app__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.85rem 1.25rem;
  background: var(--asfalto);
  color: var(--hogar);
  border-bottom: 0.14rem solid color-mix(in srgb, var(--amarillo-ruta) 35%, transparent);
  flex-wrap: wrap;
}

.app__header-izq {
  display: flex;
  align-items: center;
  gap: 0.9rem;
}

.app__titulo-ruta {
  font: 600 0.9rem var(--font-titulo);
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: var(--amarillo-ruta);
}

.app__header-der {
  display: flex;
  align-items: center;
  gap: 0.85rem;
  flex-wrap: wrap;
}

.app__usuario {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  text-align: right;
}

.app__nombre {
  font-weight: 600;
  font-size: 0.9rem;
  line-height: 1.2;
}

.app__rol {
  font-size: 0.7rem;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--amarillo-ruta);
}

.app__logout {
  padding: 0.5rem 1rem;
  font: 600 0.8rem var(--font-cuerpo);
  color: var(--hogar);
  background: var(--rojo-senal);
  border: 0;
  border-radius: 0.5rem;
  cursor: pointer;
  transition: background 120ms ease;
}

.app__logout:hover {
  background: color-mix(in srgb, var(--rojo-senal) 88%, #000);
}

.app__logout:focus-visible {
  outline: 0.2rem solid var(--amarillo-ruta);
  outline-offset: 0.15rem;
}

.app__main {
  flex: 1;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 180ms ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>