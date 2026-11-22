import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';

import { Usuarios as ServicioUsuarios } from '../../core/servicios/usuarios';
import { Rol, Usuario } from '../../core/modelos/usuario';
import { fechaCorta } from '../../core/fechas';

@Component({
  selector: 'app-usuarios',
  templateUrl: './usuarios.html',
  styleUrl: './usuarios.css',
})
export class UsuariosLista {
  private readonly servicio = inject(ServicioUsuarios);

  protected readonly fechaCorta = fechaCorta;

  protected readonly usuarios = signal<Usuario[]>([]);
  protected readonly roles = signal<Rol[]>([]);
  protected readonly cargando = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.cargar();
  }

  protected async cargar(): Promise<void> {
    this.cargando.set(true);
    this.error.set(null);

    try {
      const [usuarios, roles] = await Promise.all([
        this.servicio.obtenerTodos(),
        this.servicio.obtenerRoles(),
      ]);

      this.usuarios.set(usuarios);
      this.roles.set(roles);
    } catch (fallo) {
      this.error.set(this.explicar(fallo));
    } finally {
      this.cargando.set(false);
    }
  }

  private explicar(fallo: unknown): string {
    if (fallo instanceof HttpErrorResponse) {
      if (fallo.status === 0) {
        return 'No se pudo contactar al servidor.';
      }

      if (fallo.status === 403) {
        return 'Solo el administrador puede ver los usuarios del sistema.';
      }

      return fallo.error?.errorMessage ?? 'No se pudieron cargar los usuarios.';
    }

    return 'Ocurrió un error inesperado.';
  }
}
