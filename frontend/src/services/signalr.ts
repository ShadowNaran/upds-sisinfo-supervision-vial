import { HubConnection, HubConnectionBuilder, LogLevel } from '@microsoft/signalr'
import { useAuthStore } from '@/stores/auth'

let connection: HubConnection | null = null
type Callback = (data: any) => void
const callbacks: Record<string, Callback[]> = {}

export function initSignalR() {
  if (connection) return

  const auth = useAuthStore()
  const token = auth.perfil?.token
  if (!token) return

  const baseUrl = import.meta.env.VITE_API_URL ?? 'https://upds-sisinfo-supervision-vial.onrender.com'

  connection = new HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/alertas?access_token=${token}`)
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build()

  connection.on('RecibirAlertaCritica', (alerta) => {
    if (callbacks['RecibirAlertaCritica']) {
      callbacks['RecibirAlertaCritica'].forEach(cb => cb(alerta))
    }
  })

  connection.start().catch(err => console.error('Error conectando a SignalR', err))
}

export function onAlertaCritica(cb: Callback) {
  if (!callbacks['RecibirAlertaCritica']) callbacks['RecibirAlertaCritica'] = []
  callbacks['RecibirAlertaCritica'].push(cb)
}

export function detenerSignalR() {
  if (connection) {
    connection.stop()
    connection = null
  }
}
