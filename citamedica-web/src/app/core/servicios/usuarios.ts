import { inject, Service } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { API } from '../api.config';
import { Rol, Usuario } from '../modelos/usuario';

/**
 * Usuarios y roles. Estos endpoints el servidor solo se los sirve al
 * administrador: con cualquier otro rol responden 403.
 */
@Service()
export class Usuarios {
  private readonly http = inject(HttpClient);

  private readonly base = `${API.catalogos}/api/Usuarios`;

  obtenerTodos(): Promise<Usuario[]> {
    return firstValueFrom(this.http.get<Usuario[]>(this.base));
  }

  obtenerRoles(): Promise<Rol[]> {
    return firstValueFrom(this.http.get<Rol[]>(`${this.base}/roles`));
  }
}
