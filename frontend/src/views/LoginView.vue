<script setup lang="ts">
import { computed, reactive, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useOnline } from '@/composables/useConnection'
import { useAuthStore } from '@/stores/auth'
import type { ErroresLogin } from '@/validators/login'
import { tieneErrores, validarLogin } from '@/validators/login'

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
  <main class="login-page">
    <section class="login-visual" aria-label="Presentacion AROOMAF">
      <button class="brand" aria-label="AROOMAF inicio">
        <span class="brand-mark"><span></span><span></span><span></span></span>
        <span style="color:white">AROOMAF<small style="color:#899195">SUPERVISION VIAL</small></span>
      </button>
      <div class="road-art" aria-hidden="true"><span></span><span></span><span></span></div>
      <div class="login-message">
        <p>CONTROL EN CADA KILOMETRO</p>
        <h1>Carreteras seguras.<br>Decisiones claras.</h1>
        <span>Supervision vial y evidencia de campo, incluso sin conexion.</span>
      </div>
      <div class="visual-footer">
        <span>ADMINISTRADORA BOLIVIANA DE CARRETERAS</span>
        <div class="connection" style="background:#ffffff0c;border-color:#ffffff20;">
          <span class="pulse"></span>
          <div>
            <b style="color:#b9e9cf">EN LINEA</b>
            <small style="color:#85918b">Sistema disponible</small>
          </div>
        </div>
      </div>
    </section>

    <section class="login-form" aria-label="Acceso al sistema">
      <div class="mobile-brand">
        <button class="brand">
          <span class="brand-mark"><span></span><span></span><span></span></span>
          <span>AROOMAF<small>SUPERVISION VIAL</small></span>
        </button>
      </div>
      <div class="form-wrap">
        <p class="eyebrow">BIENVENIDO DE NUEVO</p>
        <h2>Iniciar sesion</h2>
        <p>Ingresa tus credenciales para acceder al sistema.</p>
        <form novalidate @submit.prevent="enviar">
          <label for="username">USUARIO
            <div class="login-input" :style="errores.username ? 'border-color:var(--red)' : ''">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="8" r="4"/><path d="M4 22a8 8 0 0 1 16 0"/></svg>
              <input id="username" v-model="formulario.username" type="text" autocomplete="username" autocapitalize="none" spellcheck="false" :aria-invalid="Boolean(errores.username)" @input="revalidar('username')" placeholder="usuario@aroomaf.bo" />
            </div>
            <span v-if="errores.username" style="display:block;font:600 10px Archivo;color:var(--red);margin-top:4px;">{{ errores.username }}</span>
          </label>
          <label for="password">CONTRASENA
            <div class="login-input" :style="errores.password ? 'border-color:var(--red)' : ''">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 22s8-4 8-11V5l-8-3-8 3v6c0 7 8 11 8 11z"/></svg>
              <input id="password" v-model="formulario.password" :type="verContrasena ? 'text' : 'password'" autocomplete="current-password" :aria-invalid="Boolean(errores.password)" @input="revalidar('password')" />
              <button type="button" @click="verContrasena = !verContrasena" :aria-label="verContrasena ? 'Ocultar' : 'Mostrar'">
                <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M2 12s3.5-6 10-6 10 6 10 6-3.5 6-10 6S2 12 2 12"/><circle cx="12" cy="12" r="2.5"/></svg>
              </button>
            </div>
            <span v-if="errores.password" style="display:block;font:600 10px Archivo;color:var(--red);margin-top:4px;">{{ errores.password }}</span>
          </label>
          <div v-if="errorServidor" role="alert" style="margin:12px 0;padding:10px 12px;background:color-mix(in srgb, var(--red) 10%, transparent);border-left:3px solid var(--red);border-radius:6px;font:600 11px Archivo;color:var(--red);">
            {{ errorServidor }}
          </div>
          <div class="remember">
            <label><input type="checkbox" /> Recordarme</label>
            <details style="font-size:10px;">
              <summary style="cursor:pointer;color:var(--orange);font-weight:600;">Cuentas de prueba</summary>
              <div style="margin-top:6px;line-height:1.7;color:#666;">
                <div><b>admin</b> / Admin.123!</div>
                <div><b>supervisor</b> / Sup.123!</div>
                <div><b>personal</b> / Per.123!</div>
              </div>
            </details>
          </div>
          <button type="submit" class="login-button" :disabled="enviando">
            {{ botonTexto.toUpperCase() }}
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="m9 18 6-6-6-6"/></svg>
          </button>
          <small class="secure">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 22s8-4 8-11V5l-8-3-8 3v6c0 7 8 11 8 11z"/></svg>
            Conexion segura y datos protegidos
          </small>
        </form>
      </div>
    </section>
  </main>
</template>

<style scoped>
/* acceso */
.login-page {
  display: flex;
  min-height: 100vh;
  font-family: 'Inter', 'Archivo', sans-serif;
  background: #0f172a;
}

/* panel visual */
.login-visual {
  flex: 1;
  position: relative;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 60px;
  background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
  color: white;
}

.login-visual::before {
  content: "";
  position: absolute;
  top: -10%;
  left: -20%;
  width: 60vw;
  height: 60vw;
  background: radial-gradient(circle, rgba(233, 80, 14, 0.15) 0%, transparent 60%);
  filter: blur(80px);
  z-index: 0;
  animation: float 12s ease-in-out infinite;
}

.login-visual::after {
  content: "";
  position: absolute;
  bottom: -20%;
  right: -20%;
  width: 50vw;
  height: 50vw;
  background: radial-gradient(circle, rgba(34, 197, 94, 0.1) 0%, transparent 60%);
  filter: blur(80px);
  z-index: 0;
  animation: float 10s ease-in-out infinite reverse;
}

@keyframes float {
  0% { transform: translate(0, 0) scale(1); }
  50% { transform: translate(40px, -40px) scale(1.05); }
  100% { transform: translate(0, 0) scale(1); }
}

.login-visual * {
  position: relative;
  z-index: 1;
}

.login-message {
  margin: auto 0;
}
.login-message h1 {
  font-size: 3.5rem;
  line-height: 1.1;
  margin-bottom: 20px;
  background: linear-gradient(180deg, #ffffff 0%, #cbd5e1 100%);
  -webkit-background-clip: text;
  -webkit-text-fill-color: transparent;
}
.login-message p {
  color: #e9500e;
  font-weight: 700;
  letter-spacing: 0.1em;
  font-size: 13px;
  margin-bottom: 10px;
}
.login-message span {
  font-size: 1.1rem;
  color: #94a3b8;
  max-width: 400px;
  display: block;
}

/* formulario */
.login-form {
  flex: 0 0 480px;
  background: rgba(255, 255, 255, 0.98);
  display: flex;
  flex-direction: column;
  padding: 60px;
  box-shadow: -20px 0 50px rgba(0, 0, 0, 0.2);
  z-index: 10;
}

.form-wrap {
  margin: auto 0;
}
.form-wrap h2 {
  font-size: 2rem;
  color: #0f172a;
  margin-bottom: 8px;
}
.form-wrap p {
  color: #64748b;
  margin-bottom: 30px;
}
.form-wrap .eyebrow {
  color: #e9500e;
  font-weight: 700;
  font-size: 11px;
  letter-spacing: 0.05em;
  margin-bottom: 10px;
}

/* campos */
.form-wrap > form > label {
  display: block;
  font-size: 12px;
  font-weight: 700;
  letter-spacing: .08em;
  color: #64748b;
  margin-top: 28px;
}

.login-input {
  display: flex;
  align-items: center;
  background: #f1f5f9;
  border: 2px solid transparent;
  border-radius: 16px;
  padding: 16px 22px;
  margin-top: 12px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: inset 0 2px 5px rgba(0,0,0,0.03);
}
.login-input:hover {
  background: #e2e8f0;
}
.login-input:focus-within {
  border-color: #e9500e;
  background: #ffffff;
  box-shadow: 0 15px 35px -5px rgba(233, 80, 14, 0.15), 0 10px 15px -5px rgba(233, 80, 14, 0.1);
  transform: translateY(-2px);
}
.login-input input {
  border: none;
  background: transparent;
  flex: 1;
  outline: none;
  font-size: 16px;
  color: #0f172a;
  margin-left: 16px;
  font-weight: 600;
}
.login-input input::placeholder {
  color: #94a3b8;
  font-weight: 500;
}
.login-input svg {
  color: #94a3b8;
  transition: color 0.3s ease;
}
.login-input:focus-within svg {
  color: #e9500e;
}

/* boton */
.login-button {
  background: linear-gradient(135deg, #f97316 0%, #ea580c 100%);
  color: white;
  border: none;
  border-radius: 12px;
  padding: 16px;
  width: 100%;
  font-weight: 600;
  font-size: 15px;
  cursor: pointer;
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 10px;
  margin-top: 32px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  box-shadow: 0 8px 20px rgba(234, 88, 12, 0.25);
}
.login-button:hover:not(:disabled) {
  transform: translateY(-3px);
  box-shadow: 0 12px 25px rgba(234, 88, 12, 0.35);
}
.login-button:active:not(:disabled) {
  transform: translateY(0);
}

/* detalles */
.remember {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 24px;
}
.remember label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 13px;
  color: #64748b;
  cursor: pointer;
}
.secure {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  margin-top: 24px;
  color: #94a3b8;
  font-size: 12px;
}

@media (max-width: 900px) {
  .login-page { flex-direction: column; }
  .login-visual { display: none; }
  .login-form { flex: 1; padding: 30px; box-shadow: none; }
}
</style>
