import { inject, Service } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { API } from '../api.config';
import { Cita, CrearCita } from '../modelos/cita';

/**
 * Habla con el microservicio de Agenda.
 *
 * No hace falta poner el token: el interceptor se lo pega a toda llamada que
 * sale de la aplicación.
 */
@Service()
export class Citas {
  private readonly http = inject(HttpClient);

  private readonly base = `${API.agenda}/api/Citas`;

  obtenerTodas(): Promise<Cita[]> {
    return firstValueFrom(this.http.get<Cita[]>(this.base));
  }

  obtenerPorId(citaId: number): Promise<Cita> {
    return firstValueFrom(this.http.get<Cita>(`${this.base}/${citaId}`));
  }

  crear(cita: CrearCita): Promise<Cita> {
    return firstValueFrom(this.http.post<Cita>(this.base, cita));
  }
}
