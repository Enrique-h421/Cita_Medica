import { inject, Service } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { API } from '../api.config';
import { Expediente } from '../modelos/expediente';

/**
 * Los expedientes son de solo lectura desde la API: no hay forma de crear uno
 * ni de borrarlo. Nacen solos en la primera atención del paciente, y en una
 * clínica un expediente no se elimina.
 */
@Service()
export class Expedientes {
  private readonly http = inject(HttpClient);

  private readonly base = `${API.agenda}/api/Expedientes`;

  obtenerTodos(): Promise<Expediente[]> {
    return firstValueFrom(this.http.get<Expediente[]>(this.base));
  }

  obtenerPorPaciente(pacienteId: number): Promise<Expediente> {
    return firstValueFrom(this.http.get<Expediente>(`${this.base}/paciente/${pacienteId}`));
  }
}
