/**
 * Diagnósticos codificados con CIE-10, la clasificación de enfermedades de la
 * OMS. El catálogo lo mantiene la clínica; el médico solo escoge de él.
 */

/** Una entrada del catálogo, como sale en el buscador. */
export interface Diagnostico {
  diagnosticoId: number;

  /** Código CIE-10, por ejemplo "J02.9". Puede venir vacío. */
  codigoCIE10: string | null;

  nombre: string;
  descripcion: string | null;
}

/** Un diagnóstico ya puesto en una consulta. */
export interface AtencionDiagnostico {
  /** Id del registro puente. Es el que se usa para quitarlo. */
  atencionDiagnosticoId: number;

  atencionId: number;
  diagnosticoId: number;

  codigoCIE10: string | null;
  nombre: string | null;

  /** PRINCIPAL o SECUNDARIO. */
  tipoDiagnostico: string;

  observaciones: string | null;
}

/** Lo que se manda para agregarle un diagnóstico a la consulta. */
export interface AgregarDiagnostico {
  diagnosticoId: number;

  /**
   * Si va en null, el servidor lo resuelve: el primero de la consulta queda
   * como principal y los siguientes como secundarios. Así el médico no tiene
   * que pensar en la codificación mientras atiende.
   */
  tipoDiagnostico?: string | null;

  observaciones?: string | null;
}

export const TIPO_DIAGNOSTICO = {
  principal: 'PRINCIPAL',
  secundario: 'SECUNDARIO',
} as const;

export function esPrincipal(diagnostico: AtencionDiagnostico): boolean {
  return diagnostico.tipoDiagnostico === TIPO_DIAGNOSTICO.principal;
}

/** "J02.9 — Faringitis aguda", o solo el nombre si no tiene código. */
export function etiqueta(diagnostico: Diagnostico | AtencionDiagnostico): string {
  const nombre = diagnostico.nombre ?? '';
  return diagnostico.codigoCIE10 ? `${diagnostico.codigoCIE10} — ${nombre}` : nombre;
}
