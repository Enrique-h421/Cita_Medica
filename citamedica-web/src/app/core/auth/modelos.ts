/**
 * Lo que viaja entre el frontend y el endpoint de autenticación.
 * Los nombres calzan exactamente con el SesionDto del microservicio de
 * Catálogos, porque la API serializa en camelCase.
 */

export interface Credenciales {
  nombreUsuario: string;
  password: string;
}

export interface Sesion {
  token: string;
  expiracion: string;
  usuarioId: number;
  nombreUsuario: string;
  nombreCompleto: string | null;
  rolId: number;
  /** Rol normalizado, sin acentos. Es el que viaja dentro del token. */
  rol: string;
  /** Rol como está en la base, con acentos. Para mostrar en pantalla. */
  rolNombre: string | null;
  /** Ficha de médico, si el usuario es médico. Null si no lo es. */
  medicoId: number | null;
}

/**
 * Los roles del sistema, escritos igual que en el backend
 * (Domain/Seguridad/RolesSistema.cs). Sin acentos: así es como el servidor
 * los mete dentro del token.
 */
export const ROLES = {
  administrador: 'Administrador',
  medico: 'Medico',
  recepcion: 'Recepcion',
} as const;
