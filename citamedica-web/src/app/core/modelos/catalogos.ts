/**
 * Lo mínimo que el frontend necesita de los catálogos para armar un
 * formulario. No son los DTO completos del backend: aquí solo se declara lo
 * que se usa, que es como debe ser — declarar campos que no se tocan solo
 * crea la ilusión de que el frontend depende de ellos.
 */

export interface MedicoOpcion {
  medicoId: number;
  codigoMedico: string;
  nombreCompleto: string;
  especialidadPrincipalId: number | null;
  especialidadPrincipalNombre: string | null;
  activo: boolean;
}

export interface ConsultorioOpcion {
  consultorioId: number;
  codigo: string;
  nombre: string;
  /** "C-101 — Consultorio 1 (Consulta Externa)", ya listo para el combo. */
  descripcion: string;
  activo: boolean;
}

export interface EstadoCitaOpcion {
  estadoCitaId: number;
  codigo: string;
  nombre: string;
}
