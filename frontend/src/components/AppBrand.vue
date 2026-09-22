<script setup lang="ts">
withDefaults(
  defineProps<{
    compacto?: boolean
    claro?: boolean
  }>(),
  { compacto: false, claro: false },
)
</script>

<template>
  <div class="marca" :class="{ 'marca--compacto': compacto, 'marca--claro': claro }">
    <div class="marca__senal" aria-hidden="true">
      <span class="marca__linea" />
    </div>
    <div class="marca__texto">
      <span class="marca__nombre">AROOMAF</span>
      <span class="marca__subtitulo">Supervision de tramos viales</span>
    </div>
  </div>
</template>

<style scoped>
.marca {
  display: inline-flex;
  align-items: center;
  gap: 0.9rem;
}

.marca__senal {
  width: 3rem;
  height: 3rem;
  flex: none;
  border-radius: 0.55rem;
  background: var(--asfalto);
  display: grid;
  place-items: center;
  border: 0.18rem solid var(--amarillo-ruta);
}

.marca__linea {
  width: 1.5rem;
  border-top: 0.32rem dashed var(--amarillo-ruta);
}

.marca--compacto .marca__senal {
  width: 2.35rem;
  height: 2.35rem;
}

.marca--compacto .marca__linea {
  width: 1.1rem;
}

.marca__texto {
  display: flex;
  flex-direction: column;
}

.marca__nombre {
  font-family: var(--font-titulo);
  font-weight: 700;
  font-size: 1.4rem;
  line-height: 1;
  text-transform: uppercase;
  letter-spacing: 0.08em;
  color: var(--asfalto);
}

.marca--compacto .marca__nombre {
  font-size: 1.1rem;
}

.marca__subtitulo {
  font-size: 0.78rem;
  line-height: 1.2;
  color: color-mix(in srgb, var(--asfalto) 72%, transparent);
  margin-top: 0.2rem;
}

.marca--claro .marca__nombre {
  color: var(--hogar);
}

.marca--claro .marca__subtitulo {
  color: color-mix(in srgb, var(--hogar) 82%, transparent);
}
</style>