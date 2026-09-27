export type EstadoAsistencia = 'Presente' | 'Falta' | 'NoDisponible'

export interface Tramo { id: string; codigo: string; nombre: string; descripcion?: string; latitudInicio?: number; longitudInicio?: number; latitudFin?: number; longitudFin?: number; kmInicio?: number; kmFin?: number; activo: boolean }
export type EstadoValidacionPersonal = 'Aprobado' | 'PendienteValidacion' | 'Rechazado'
export interface Personal { id: string; nombreCompleto: string; documento: string; cargo?: string; idTramo: string; estadoValidacion: EstadoValidacionPersonal }
export interface DetallePlanilla { id: string; idPersonal: string; personal: string; estado: EstadoAsistencia; observacion?: string; clasificacion?: string; severidad?: 0 | 1 | 2; fotoBase64?: string; latitud?: number; longitud?: number; kilometraje?: number; accionMitigacion?: string; horaMitigacion?: string; esFalsoPositivo?: boolean }
export interface Planilla { id: string; fecha: string; idTramo: string; tramo: string; estado: 'Borrador' | 'Cerrada'; observaciones?: string; tieneFirma: boolean; firmaBase64?: string; detalles: DetallePlanilla[]; timestampLocal?: number }