/**
 * Una cita tal como la devuelve el microservicio de Agenda.
 *
 * Viene plana a propósito: los nombres de paciente y médico ya vienen
 * resueltos desde Catálogos, así que el frontend no tiene que ir a buscarlos.
 * Si Catálogos estuviera caído llegarían en null y la agenda se muestra igual,
 * solo que con los identificadores en lugar de los nombres.
 */
export interface Cita {
  citaId: number;

  pacienteId: number;
  pacienteNombre: string | null;

  medicoId: number;
  medicoNombre: string | null;

  especialidadId: number | null;
  especialidadNombre: string | null;

  consultorioId: number | null;
  consultorioNombre: string | null;

  estadoCitaId: number;
  estadoCodigo: string;
  estadoNombre: string;

  fechaHoraInicio: string;
  fechaHoraFin: string | null;

  motivo: string | null;
  observaciones: string | null;
  fechaCreacion: string;
}

/**
 * Lo que se manda para agendar una cita. No lleva CitaId ni FechaCreacion:
 * esos los pone el servidor.
 */
export interface CrearCita {
  pacienteId: number;
  medicoId: number;
  especialidadId?: number | null;
  consultorioId?: number | null;
  estadoCitaId: number;
  /** Sin zona horaria: "2026-11-24T09:00:00". */
  fechaHoraInicio: string;
  fechaHoraFin?: string | null;
  motivo?: string | null;
  observaciones?: string | null;
}

/**
 * Códigos de la tabla Estados_Cita. Son los mismos que usa el backend en
 * Domain/AgendaYAtencion/CodigosEstadoCita.cs.
 */
export const ESTADO_CITA = {
  pendiente: 'PEND',
  confirmada: 'CONF',
  atendida: 'ATEN',
  cancelada: 'CANC',
  noAsistio: 'NOAS',
} as const;

/**
 * Una cita se puede atender mientras no haya sido ya atendida ni cancelada.
 * Esta regla también vive en el servidor; aquí solo sirve para no ofrecer un
 * botón que va a fallar.
 */
export function sePuedeAtender(cita: Cita): boolean {
  return (
    cita.estadoCodigo !== ESTADO_CITA.atendida &&
    cita.estadoCodigo !== ESTADO_CITA.cancelada &&
    cita.estadoCodigo !== ESTADO_CITA.noAsistio
  );
}

/**
 * El color del filo que lleva la cita en la agenda y en el panel de inicio.
 * Vive aquí y no en cada pantalla para que las dos pinten igual el mismo
 * estado: si un día cambia, cambia en un solo sitio.
 */
export function acentoDeEstado(codigo: string): string {
  switch (codigo) {
    case ESTADO_CITA.atendida:
      return 'exito';
    case ESTADO_CITA.confirmada:
      return 'info';
    case ESTADO_CITA.pendiente:
      return 'aviso';
    case ESTADO_CITA.cancelada:
      return 'error';
    default:
      return 'neutro';
  }
}
