export type EstadoAsistencia = 'Presente' | 'Falta' | 'NoDisponible'

export interface Tramo { id: string; codigo: string; nombre: string }
export interface Personal { id: string; nombreCompleto: string; documento: string; cargo?: string; idTramo: string }
export interface DetallePlanilla { id: string; idPersonal: string; personal: string; estado: EstadoAsistencia; observacion?: string; clasificacion?: string; fotoBase64?: string }
export interface Planilla { id: string; fecha: string; idTramo: string; tramo: string; estado: 'Borrador' | 'Cerrada'; observaciones?: string; tieneFirma: boolean; detalles: DetallePlanilla[] }