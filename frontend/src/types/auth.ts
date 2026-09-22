export type Rol = 'Administrador' | 'SupervisorCampo' | 'PersonalMicroempresa'

export interface Perfil {
  token: string
  rol: Rol
  nombre: string
  username: string
  /** ISO 8601 UTC de expiracion del token. */
  expiresAt: string
  /** Segundos que restan de vigencia. */
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