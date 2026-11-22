/**
 * Un paciente tal como lo devuelve el microservicio de Catálogos.
 */
export interface Paciente {
  pacienteId: number;
  codigoPaciente: string;

  tipoDocumento: string;
  numeroDocumento: string;

  nombres: string;
  apellidos: string;

  /** Ya viene armado del servidor, listo para pintar. */
  nombreCompleto: string;

  fechaNacimiento: string;

  /** Calculada al vuelo por el servidor; no se guarda en la base. */
  edad: number | null;

  sexo: string;
  estadoCivil: string | null;
  telefono: string | null;
  telefonoSecundario: string | null;
  email: string | null;
  direccion: string | null;
  ciudad: string | null;
  pais: string | null;
  ocupacion: string | null;
  tipoSangre: string | null;

  activo: boolean;
  fechaRegistro: string;
}

/** Lo que se manda para registrar un paciente nuevo. */
export interface CrearPaciente {
  codigoPaciente: string;
  tipoDocumento: string;
  numeroDocumento: string;
  nombres: string;
  apellidos: string;
  /** Solo la fecha: "1990-05-14". */
  fechaNacimiento: string;
  sexo: string;
  estadoCivil?: string | null;
  telefono?: string | null;
  telefonoSecundario?: string | null;
  email?: string | null;
  direccion?: string | null;
  ciudad?: string | null;
  pais?: string | null;
  ocupacion?: string | null;
  tipoSangre?: string | null;
  activo: boolean;
}

export const TIPOS_DOCUMENTO = ['DUI', 'Pasaporte', 'Carné de minoridad', 'NIT'] as const;

export const ESTADOS_CIVILES = ['Soltero/a', 'Casado/a', 'Acompañado/a', 'Divorciado/a', 'Viudo/a'] as const;

export const TIPOS_SANGRE = ['O+', 'O-', 'A+', 'A-', 'B+', 'B-', 'AB+', 'AB-'] as const;
