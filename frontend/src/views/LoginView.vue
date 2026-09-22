<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useOnline } from '@/composables/useConnection'
import { useAuthStore } from '@/stores/auth'
import type { ErroresLogin } from '@/validators/login'
import { tieneErrores, validarLogin } from '@/validators/login'
import AppBrand from '@/components/AppBrand.vue'
import ConnectionBadge from '@/components/ConnectionBadge.vue'

const router = useRouter()
const auth = useAuthStore()
const { enLinea } = useOnline()

const formulario = reactive({
  username: '',
  password: '',
})

const errores = ref<ErroresLogin>({})
const errorServidor = ref('')
const enviando = ref(false)
const verContrasena = ref(false)

const botonTexto = computed(() =>
  enviando.value ? 'Ingresando...' : 'Iniciar sesion',
)

async function enviar(): Promise<void> {
  errorServidor.value = ''
  const validados = validarLogin(formulario)
  errores.value = validados

  if (tieneErrores(validados)) {
    const clave = Object.keys(validados)[0]
    const campo = document.getElementById(clave)
    campo?.focus()
    return
  }

  if (!enLinea.value) {
    await auth.init()
    if (auth.estaAutenticado()) {
      await router.replace(auth.rutaInicial())
      return
    }
    errorServidor.value = 'Sin conexion. Necesitas internet para iniciar sesion por primera vez.'
    return
  }

  enviando.value = true
  try {
    await auth.login(formulario)
    await router.replace(auth.rutaInicial())
  } catch (error) {
    errorServidor.value = auth.traducirError(error)
  } finally {
    enviando.value = false
  }
}

function revalidar(campo: 'username' | 'password'): void {
  errores.value = validarLogin(formulario)
  void campo
}
</script>

<template>
  <main class="login">
    <section class="login__brand" aria-label="Presentacion de AROOMAF">
      <div class="login__brand-contenido">
        <AppBrand claro />

        <div class="login__lema">
          <h1 class="login__titulo">Tu tramo, al dia.</h1>
          <p class="login__parrafo">
            Registro de asistencia y estado de la ruta desde la misma obra,
            con o sin senal.
          </p>
        </div>

        <p class="login__institucion">Sistemas de Informacion I - UPDS Tarija</p>
      </div>
      <div class="login__rutas" aria-hidden="true">
        <span class="login__ruta login__ruta--1" />
        <span class="login__ruta login__ruta--2" />
        <span class="login__ruta login__ruta--3" />
      </div>
    </section>

    <section class="login__panel" aria-label="Acceso al sistema">
      <div class="login__tarjeta">
        <header class="login__superior">
          <AppBrand compacto class="login__marca-movil" />
          <ConnectionBadge />
        </header>

        <h2 class="login__ingreso">Iniciar sesion</h2>
        <p class="login__ayuda">
          Usa el usuario y la contrasena que te asigno la oficina central.
        </p>

        <form class="login__form" novalidate @submit.prevent="enviar">
          <div class="campo" :class="{ 'campo--error': errores.username }">
            <label class="campo__etiqueta" for="username">Usuario</label>
            <input
              id="username"
              v-model="formulario.username"
              class="campo__control"
              type="text"
              name="username"
              autocomplete="username"
              inputmode="text"
              autocapitalize="none"
              spellcheck="false"
              :aria-invalid="Boolean(errores.username)"
              aria-describedby="error-username"
              @input="revalidar('username')"
            />
            <p v-if="errores.username" id="error-username" class="campo__mensaje" role="alert">
              {{ errores.username }}
            </p>
          </div>

          <div class="campo" :class="{ 'campo--error': errores.password }">
            <label class="campo__etiqueta" for="password">Contrasena</label>
            <div class="campo__con-ojo">
              <input
                id="password"
                v-model="formulario.password"
                class="campo__control"
                :type="verContrasena ? 'text' : 'password'"
                name="password"
                autocomplete="current-password"
                :aria-invalid="Boolean(errores.password)"
                aria-describedby="error-password"
                @input="revalidar('password')"
              />
              <button
                type="button"
                class="campo__ojo"
                :aria-label="verContrasena ? 'Ocultar contrasena' : 'Mostrar contrasena'"
                :aria-pressed="verContrasena"
                @click="verContrasena = !verContrasena"
              >
                {{ verContrasena ? 'Ocultar' : 'Ver' }}
              </button>
            </div>
            <p v-if="errores.password" id="error-password" class="campo__mensaje" role="alert">
              {{ errores.password }}
            </p>
          </div>

          <div v-if="errorServidor" class="login__error" role="alert">
            {{ errorServidor }}
          </div>

          <button type="submit" class="login__boton" :disabled="enviando">
            {{ botonTexto }}
          </button>
        </form>

        <details class="login__demo">
          <summary>Cuentas de prueba</summary>
          <ul>
            <li><strong>admin</strong> / Admin.123! - Administracion</li>
            <li><strong>supervisor</strong> / Sup.123! - Supervisor de campo</li>
            <li><strong>personal</strong> / Per.123! - Personal de microempresa</li>
          </ul>
        </details>
      </div>
    </section>
  </main>
</template>

<style scoped>
.login {
  min-height: 100dvh;
  display: grid;
  grid-template-rows: auto 1fr;
  background: var(--hormigon);
}

/* panel de marca */
.login__brand {
  position: relative;
  overflow: hidden;
  background: var(--asfalto);
  color: var(--hogar);
  padding: 1.6rem 1.5rem 3.5rem;
}

.login__rutas {
  position: absolute;
  inset: 0;
  pointer-events: none;
}

.login__ruta {
  position: absolute;
  left: -10%;
  width: 120%;
  border-top: 0.22rem dashed color-mix(in srgb, var(--amarillo-ruta) 38%, transparent);
  transform: rotate(-14deg);
}

.login__ruta--1 { top: 46%; }
.login__ruta--2 { top: 58%; }
.login__ruta--3 { top: 70%; }

.login__lema {
  margin-top: 2.6rem;
  max-width: 34ch;
}

.login__titulo {
  font-family: var(--font-titulo);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  font-size: clamp(2rem, 8vw, 2.6rem);
  line-height: 1.05;
  color: var(--hogar);
  margin: 0;
}

.login__parrafo {
  margin-top: 0.9rem;
  font-size: 1.02rem;
  line-height: 1.5;
  color: color-mix(in srgb, var(--hogar) 84%, transparent);
}

.login__institucion {
  margin-top: 1.6rem;
  font-size: 0.8rem;
  letter-spacing: 0.06em;
  color: color-mix(in srgb, var(--hogar) 68%, transparent);
}

/* panel del formulario */
.login__panel {
  display: grid;
  place-items: start center;
  padding: 2.2rem 1.25rem 3rem;
}

.login__tarjeta {
  width: 100%;
  max-width: 26rem;
}

.login__superior {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
}

.login__ingreso {
  font-family: var(--font-titulo);
  font-weight: 600;
  font-size: 1.5rem;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  color: var(--asfalto);
  margin: 2rem 0 0.35rem;
}

.login__ayuda {
  font-size: 0.95rem;
  color: color-mix(in srgb, var(--asfalto) 72%, transparent);
  margin: 0 0 1.6rem;
}

/* campo de formulario */
.campo {
  margin-bottom: 1.15rem;
}

.campo__etiqueta {
  display: block;
  font-weight: 600;
  font-size: 0.9rem;
  color: var(--asfalto);
  margin-bottom: 0.4rem;
}

.campo__control {
  width: 100%;
  min-height: 3.4rem;
  padding: 0 1rem;
  font: inherit;
  font-size: 1rem;
  color: var(--asfalto);
  background: var(--hormigon);
  border: 0.14rem solid var(--linea);
  border-radius: 0.6rem;
  transition: border-color 120ms ease, box-shadow 120ms ease;
}

.campo__control:focus-visible {
  outline: none;
  border-color: var(--naranja-obra);
  box-shadow: 0 0 0 0.22rem color-mix(in srgb, var(--naranja-obra) 28%, transparent);
}

.campo--error .campo__control {
  border-color: var(--rojo-senal);
}

.campo__con-ojo {
  position: relative;
}

.campo__con-ojo .campo__control {
  padding-right: 4.6rem;
}

.campo__ojo {
  position: absolute;
  top: 0.45rem;
  right: 0.45rem;
  height: 2.5rem;
  padding: 0 0.9rem;
  font: 600 0.82rem/1 var(--font-cuerpo);
  color: var(--asfalto);
  background: transparent;
  border: 0;
  border-radius: 0.5rem;
  cursor: pointer;
}

.campo__ojo:hover {
  background: color-mix(in srgb, var(--asfalto) 6%, transparent);
}

.campo__mensaje {
  margin: 0.4rem 0 0;
  font-size: 0.84rem;
  font-weight: 600;
  color: var(--rojo-senal);
}

/* errores y accion */
.login__error {
  margin: 0.4rem 0 1.1rem;
  padding: 0.8rem 1rem;
  border-radius: 0.6rem;
  border-left: 0.32rem solid var(--rojo-senal);
  background: color-mix(in srgb, var(--rojo-senal) 10%, transparent);
  color: color-mix(in srgb, var(--rojo-senal) 92%, #000);
  font-size: 0.92rem;
  font-weight: 600;
  line-height: 1.4;
}

.login__boton {
  width: 100%;
  min-height: 3.6rem;
  margin-top: 0.4rem;
  font: 700 1.12rem var(--font-titulo);
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--hogar);
  background: var(--naranja-obra);
  border: 0;
  border-radius: 0.6rem;
  cursor: pointer;
  transition: background 120ms ease, transform 80ms ease;
}

.login__boton:not(:disabled):hover {
  background: var(--naranja-obra-oscuro);
}

.login__boton:not(:disabled):active {
  transform: translateY(1px);
}

.login__boton:focus-visible {
  outline: 0.22rem solid var(--asfalto);
  outline-offset: 0.18rem;
}

.login__boton:disabled {
  opacity: 0.65;
  cursor: progress;
}

.login__demo {
  margin-top: 1.8rem;
  font-size: 0.85rem;
  color: color-mix(in srgb, var(--asfalto) 66%, transparent);
}

.login__demo summary {
  cursor: pointer;
  font-weight: 600;
}

.login__demo ul {
  margin: 0.6rem 0 0;
  padding-left: 1.1rem;
  line-height: 1.7;
}

/* escritorio */
@media (min-width: 900px) {
  .login {
    grid-template-columns: 46% 54%;
    grid-template-rows: 1fr;
  }

  .login__brand {
    display: grid;
    place-items: center;
    padding: 3rem;
  }

  .login__brand-contenido {
    max-width: 26rem;
    width: 100%;
  }

  .login__lema {
    margin-top: 3.4rem;
  }

  .login__panel {
    place-items: center;
    padding: 3rem;
  }

  .login__tarjeta {
    max-width: 24rem;
  }

  .login__marca-movil {
    display: none;
  }
}

@media (max-width: 899px) {
  .login__marca-movil {
    display: inherit;
  }
}
</style>