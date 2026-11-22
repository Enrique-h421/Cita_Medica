/**
 * El expediente clínico del paciente: la carpeta donde se archivan todas sus
 * atenciones. Se abre solo, en la primera consulta, y no se elimina nunca.
 */
export interface Expediente {
  expedienteId: number;

  pacienteId: number;
  pacienteNombre: string | null;

  numeroExpediente: string;

  fechaApertura: string;

  observaciones: string | null;

  activo: boolean;

  /** Cuántas atenciones lleva registradas. */
  totalAtenciones: number;
}
