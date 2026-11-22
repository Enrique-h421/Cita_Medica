/**
 * Un usuario del sistema, tal como lo devuelve el microservicio de Catálogos.
 *
 * Fijate en lo que NO está: el hash de la contraseña. La API nunca lo expone,
 * ni siquiera al administrador.
 */
export interface Usuario {
  usuarioId: number;

  nombreUsuario: string;

  nombreCompleto: string | null;

  email: string | null;

  rolId: number;

  /** Nombre del rol como está en la base, con acentos. */
  rolNombre: string | null;

  /** Ficha de médico, si el usuario es médico. Null si no lo es. */
  medicoId: number | null;

  activo: boolean;

  fechaCreacion: string;
}

export interface Rol {
  rolId: number;
  nombre: string;
  descripcion: string | null;
  activo: boolean;
  totalUsuarios: number;
}
