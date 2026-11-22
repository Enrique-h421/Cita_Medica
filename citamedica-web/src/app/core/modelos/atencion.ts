import { AtencionDiagnostico } from './diagnostico';

/**
 * La consulta médica tal como la devuelve el microservicio de Agenda.
 */
export interface Atencion {
  atencionId: number;

  pacienteId: number;
  pacienteNombre: string | null;

  medicoId: number;
  medicoNombre: string | null;

  /** Cita que la originó. Null si fue una consulta espontánea. */
  citaId: number | null;

  expedienteId: number;
  numeroExpediente: string | null;

  fechaInicio: string;
  fechaFin: string | null;

  /** Cuánto duró. Null mientras siga abierta. */
  duracionMinutos: number | null;

  motivoConsulta: string | null;
  enfermedadActual: string | null;
  exploracionFisica: string | null;
  observaciones: string | null;

  estado: string;

  /**
   * Lo que el médico concluyó: primero el principal, después los secundarios.
   * Viene vacío mientras no se le haya puesto ninguno.
   */
  diagnosticos: AtencionDiagnostico[];
}

/** Lo que se manda para abrir la consulta. */
export interface AbrirAtencion {
  pacienteId: number;
  medicoId: number;
  citaId?: number | null;
  motivoConsulta?: string | null;
}

/** Los hallazgos con los que el médico la cierra. */
export interface CerrarAtencion {
  motivoConsulta?: string | null;
  enfermedadActual?: string | null;
  exploracionFisica?: string | null;
  observaciones?: string | null;
}

export const ESTADO_ATENCION = {
  abierta: 'ABIERTA',
  cerrada: 'CERRADA',
  anulada: 'ANULADA',
} as const;

export function estaAbierta(atencion: Atencion): boolean {
  return atencion.estado === ESTADO_ATENCION.abierta;
}
