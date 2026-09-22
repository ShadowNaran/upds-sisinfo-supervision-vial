<script setup lang="ts">
import { useConnection } from '@/composables/useConnection'

const { estado, etiqueta, pendientes } = useConnection()
</script>

<template>
  <div
    class="c-badge"
    :class="['c-badge--' + estado]"
    role="status"
    :aria-label="etiqueta"
    :title="etiqueta"
  >
    <span class="c-badge__dot" aria-hidden="true" />
    <span class="c-badge__texto">{{ etiqueta }}</span>
    <span v-if="estado === 'online-con-pendientes'" class="c-badge__contador" aria-hidden="true">
      {{ pendientes }}
    </span>
  </div>
</template>

<style scoped>
.c-badge {
  display: inline-flex;
  align-items: center;
  gap: 0.4rem;
  padding: 0.35rem 0.75rem;
  border-radius: 999px;
  font: 500 0.78rem/1 var(--font-cuerpo);
  letter-spacing: 0.02em;
  transition: background 0.2s ease, color 0.2s ease, border-color 0.2s ease;
  white-space: nowrap;
}

.c-badge__dot {
  width: 0.5rem;
  height: 0.5rem;
  border-radius: 50%;
  background: currentColor;
  flex-shrink: 0;
}

.c-badge__texto {
  font-weight: 500;
}

.c-badge__contador {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 1.3rem;
  height: 1.3rem;
  padding: 0 0.35rem;
  font: 600 0.65rem var(--font-titulo);
  border-radius: 999px;
  background: currentColor;
  color: var(--hogar);
  opacity: 0.9;
}

/* estados */

/* online verde */
.c-badge--online {
  color: var(--verde-senal);
  background: color-mix(in srgb, var(--verde-senal) 12%, transparent);
  border: 0.1rem solid color-mix(in srgb, var(--verde-senal) 35%, transparent);
}

/* online con pendientes naranja */
.c-badge--online-con-pendientes {
  color: var(--naranja-obra);
  background: color-mix(in srgb, var(--naranja-obra) 12%, transparent);
  border: 0.1rem solid color-mix(in srgb, var(--naranja-obra) 35%, transparent);
}

/* offline rojo */
.c-badge--offline {
  color: var(--rojo-senal);
  background: color-mix(in srgb, var(--rojo-senal) 12%, transparent);
  border: 0.1rem solid color-mix(in srgb, var(--rojo-senal) 35%, transparent);
}

/* responsive movil solo punto y numero si hay pendientes */
@media (max-width: 480px) {
  .c-badge__texto {
    display: none;
  }
  .c-badge {
    padding: 0.35rem 0.5rem;
  }
}
</style>