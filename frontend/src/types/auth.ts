export type Rol = 'Administrador' | 'SupervisorCampo' | 'PersonalMicroempresa'

export interface Perfil {
  token: string
  rol: Rol
  nombre: string
  username: string
  /** expiracion del token en utc (iso 8601) */
  expiresAt: string
  /** segundos restantes de vigencia */
  expiresInSeconds: number
}

export interface LoginCredenciales {
  username: string
  password: string
}

export interface SesionLocal {
  perfil: Perfil
  guardadoEn: number
}

export type EstadoSesion = 'indefinido' | 'autenticado' | 'anonimo'