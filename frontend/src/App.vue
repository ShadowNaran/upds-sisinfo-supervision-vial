<script setup lang="ts">
import { onMounted, computed, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useConnection } from '@/composables/useConnection'
import UpdatePrompt from '@/components/UpdatePrompt.vue'
import BandejaSync from '@/components/BandejaSync.vue'
import AlertaCritica from '@/components/AlertaCritica.vue'
import { initSignalR } from '@/services/signalr'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()
const { pendientes } = useConnection() // estado de conexion y pendientes
const mostrarBandeja = ref(false)

const installPrompt = ref<any>(null)
const mostrarInstalar = ref(false)

window.addEventListener('beforeinstallprompt', (e) => {
  e.preventDefault()
  installPrompt.value = e
  mostrarInstalar.value = true
})

async function instalarPWA() {
  if (!installPrompt.value) return
  installPrompt.value.prompt()
  const { outcome } = await installPrompt.value.userChoice
  if (outcome === 'accepted') {
    mostrarInstalar.value = false
    installPrompt.value = null
  }
}

onMounted(async () => {
  await auth.init()
  if (auth.perfil) {
    if (auth.perfil.rol === 'Administrador') {
      initSignalR()
    }
  }
  for (const evento of ['pointerdown', 'keydown', 'touchstart']) {
    window.addEventListener(evento, auth.registrarActividad)
  }
})

const esLogin = computed(() => route.name === 'login')
const esCampo = computed(() => route.name === 'campo')
const esPanel = computed(() => route.name === 'panel')


async function cerrarSesion() {
  await auth.logout()
  await router.push('/login')
}
</script>

<template>
  <div class="app">
    <!-- encabezado -->
    <header class="topbar" v-if="!esLogin">
      <div class="topbar-inner">
        <!-- marca -->
        <button class="brand" @click="$router.push(auth.rutaInicial())" aria-label="AROOMAF inicio">
          <span class="brand-mark"><span /><span /><span /></span>
          <span>AROOMAF<small>SUPERVISIÓN VIAL</small></span>
        </button>

        <!-- navegacion -->
        <nav class="main-nav" aria-label="Navegacion principal">
          <button
            :class="{ active: esPanel || esCampo === false && !esLogin }"
            @click="$router.push('/panel')"
            v-if="auth.perfil?.rol === 'Administrador' || auth.perfil?.rol === 'SupervisorCampo'"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><rect x="5" y="4" width="14" height="18" rx="2" /><path d="M9 4V2h6v2M9 10h6m-6 4h6m-6 4h4"/></svg>
            Panel central
          </button>
          <button
            :class="{ active: esCampo }"
            @click="$router.push('/campo')"
          >
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m3 6 6-3 6 3 6-3v15l-6 3-6-3-6 3zM9 3v15m6-12v15"/></svg>
            Vista de campo
          </button>
        </nav>

        <!-- acciones -->
        <div class="header-actions">
          <button class="connection" style="border:none;cursor:pointer;" @click="mostrarBandeja = true" :title="pendientes > 0 ? `${pendientes} pendientes` : 'Sincronizado'">
            <span class="pulse" />
            <div>
              <b>{{ pendientes > 0 ? `${pendientes} PENDIENTE${pendientes > 1 ? 'S' : ''}` : 'EN LÍNEA' }}</b>
              <small>{{ pendientes > 0 ? 'Toca para ver cola' : 'Sincronizado ahora' }}</small>
            </div>
          </button>
          <div class="avatar" v-if="auth.perfil" :title="auth.perfil.nombre">
            {{ auth.perfil.nombre.split(' ').map((n: string) => n[0]).slice(0,2).join('') }}
          </div>
        </div>
      </div>
    </header>

    <main class="app__main">
      <RouterView v-slot="{ Component }">
        <transition name="fade" mode="out-in">
          <component :is="Component" />
        </transition>
      </RouterView>
    </main>

    <footer v-if="!esLogin">
      <div class="brand" aria-label="AROOMAF">
        <span class="brand-mark"><span /><span /><span /></span>
        <span>AROOMAF<small>SUPERVISIÓN VIAL</small></span>
      </div>
      <span class="footer-meta">AROOMAF · Sistema de Supervisión Vial · v2.4.1</span>
      <div class="footer-actions">
        <button v-if="mostrarInstalar" @click="instalarPWA" style="background:var(--green); color:white; border:none; padding:4px 10px; border-radius:4px; font-weight:600; cursor:pointer;">Instalar App</button>
        <button @click="cerrarSesion" v-if="auth.perfil">Cerrar sesion</button>
      </div>
    </footer>

    <UpdatePrompt />
    <BandejaSync v-if="mostrarBandeja" @cerrar="mostrarBandeja = false" />
    <AlertaCritica />
  </div>
</template>

<style scoped>
.app {
  min-height: 100dvh;
  display: flex;
  flex-direction: column;
  background: var(--hormigon);
}
.app__main { flex: 1; }
.fade-enter-active, .fade-leave-active { transition: opacity 180ms ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
.footer-actions { display:flex; gap:12px; padding-bottom:12px; }

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

.app__btn-badge {
  background: none;
  border: none;
  padding: 0;
  cursor: pointer;
  border-radius: 999px;
  outline: none;
}
.app__btn-badge:focus-visible {
  outline: 0.2rem solid var(--amarillo-ruta);
  outline-offset: 0.1rem;
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