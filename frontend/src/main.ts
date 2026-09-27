import { createApp } from 'vue'
import { createPinia } from 'pinia'
import router from '@/router'

import App from './App.vue'

// fuentes incluidas para uso sin conexion
import '@fontsource-variable/oswald/wght.css'
import '@fontsource-variable/archivo/wght.css'

// estilos globales
import './style.css'

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.mount('#app')