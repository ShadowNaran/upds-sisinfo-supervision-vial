import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

declare module 'vue-router' {
  interface RouteMeta {
    publica?: boolean
    titulo?: string
    roles?: string[]
  }
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { publica: true, titulo: 'Iniciar sesion' },
    },
    {
      path: '/campo',
      name: 'campo',
      component: () => import('@/views/CampoDashboard.vue'),
      meta: { roles: ['Administrador', 'SupervisorCampo', 'PersonalMicroempresa'], titulo: 'Modulo de campo' },
    },
    {
      path: '/',
      redirect: '/login',
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/login',
    },
  ],
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()
  await auth.init()

  if (to.meta.publica) {
    if (auth.estaAutenticado()) {
      return { path: auth.rutaInicial() }
    }
    return true
  }

  if (!auth.estaAutenticado()) {
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  const rolesPermitidos = to.meta.roles
  if (rolesPermitidos && auth.perfil && !rolesPermitidos.includes(auth.perfil.rol)) {
    return { path: auth.rutaInicial() }
  }

  return true
})

export default router