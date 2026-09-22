<script setup lang="ts">
import { computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import AppBrand from '@/components/AppBrand.vue'
import ConnectionBadge from '@/components/ConnectionBadge.vue'

const auth = useAuthStore()

const rolLabel = computed(() => {
  switch (auth.perfil?.rol) {
    case 'SupervisorCampo':
      return 'Supervisor de campo'
    case 'PersonalMicroempresa':
      return 'Personal de microempresa'
    default:
      return auth.perfil?.rol ?? 'Campo'
  }
})

const formatearExpiracion = computed(() => {
  if (!auth.perfil) return '-'
  const diff = auth.perfil.expiresAt ? new Date(auth.perfil.expiresAt).getTime() - Date.now() : 0
  if (diff <= 0) return 'Expirado'
  const horas = Math.floor(diff / 3_600_000)
  const minutos = Math.floor((diff % 3_600_000) / 60_000)
  return `${horas}h ${minutos}m`
})

async function cerrarSesion() {
  await auth.logout()
}
</script>

<template>
  <div class="campo">
    <header class="campo__header">
      <div class="campo__izquierda">
        <AppBrand compacto />
        <span class="campo__modulo">Modulo de campo</span>
      </div>
      <div class="campo__derecha">
        <ConnectionBadge />
        <div class="campo__usuario">
          <span class="campo__nombre">{{ auth.perfil?.nombre }}</span>
          <span class="campo__rol">{{ rolLabel }}</span>
        </div>
        <button class="campo__logout" @click="cerrarSesion" aria-label="Cerrar sesion">
          Cerrar sesion
        </button>
      </div>
    </header>

    <main class="campo__contenido">
      <section class="campo__bienvenida">
        <h1>Bienvenido, {{ auth.perfil?.nombre }}</h1>
        <p>Tu sesion esta activa. Aqui accederas al registro de asistencia y evidencias de tramos viales.</p>
      </section>

      <section class="campo__estado" aria-label="Estado de la sesion">
        <dl>
          <dt>Usuario</dt>
          <dd>{{ auth.perfil?.username }}</dd>
          <dt>Rol</dt>
          <dd>{{ rolLabel }}</dd>
          <dt>Expira en</dt>
          <dd>{{ formatearExpiracion }}</dd>
        </dl>
      </section>
    </main>
  </div>
</template>

<style scoped>
.campo {
  min-height: 100dvh;
  display: flex;
  flex-direction: column;
  background: var(--hormigon);
}

.campo__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 1rem 1.5rem;
  background: var(--asfalto);
  color: var(--hogar);
  border-bottom: 0.14rem solid color-mix(in srgb, var(--amarillo-ruta) 35%, transparent);
  flex-wrap: wrap;
}

.campo__izquierda {
  display: flex;
  align-items: center;
  gap: 1rem;
}

.campo__modulo {
  font: 600 0.95rem var(--font-titulo);
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: var(--amarillo-ruta);
}

.campo__derecha {
  display: flex;
  align-items: center;
  gap: 1rem;
  flex-wrap: wrap;
}

.campo__usuario {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  text-align: right;
}

.campo__nombre {
  font-weight: 600;
  font-size: 0.95rem;
  line-height: 1.2;
}

.campo__rol {
  font-size: 0.75rem;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--amarillo-ruta);
}

.campo__logout {
  padding: 0.55rem 1.1rem;
  font: 600 0.85rem var(--font-cuerpo);
  color: var(--hogar);
  background: var(--rojo-senal);
  border: 0;
  border-radius: 0.5rem;
  cursor: pointer;
  transition: background 120ms ease;
}

.campo__logout:hover {
  background: color-mix(in srgb, var(--rojo-senal) 88%, #000);
}

.campo__logout:focus-visible {
  outline: 0.2rem solid var(--amarillo-ruta);
  outline-offset: 0.15rem;
}

.campo__contenido {
  flex: 1;
  padding: 2rem 1.5rem;
  display: grid;
  gap: 1.5rem;
}

.campo__bienvenida h1 {
  font: 700 1.6rem var(--font-titulo);
  color: var(--asfalto);
  margin: 0 0 0.5rem;
}

.campo__bienvenida p {
  color: color-mix(in srgb, var(--asfalto) 72%, transparent);
  line-height: 1.5;
  margin: 0;
}

.campo__estado dl {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 0.5rem 1.5rem;
  max-width: 24rem;
  font-size: 0.95rem;
  color: var(--asfalto);
}

.campo__estado dt {
  font-weight: 600;
  color: color-mix(in srgb, var(--asfalto) 75%, transparent);
}

.campo__estado dd {
  margin: 0;
  font-family: var(--font-cuerpo);
}

@media (min-width: 600px) {
  .campo__contenido {
    max-width: 48rem;
    margin: 0 auto;
    width: 100%;
  }
}
</style>