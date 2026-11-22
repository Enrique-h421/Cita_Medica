import { inject, Service } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { API } from '../api.config';
import { ConsultorioOpcion, EstadoCitaOpcion } from '../modelos/catalogos';

/**
 * Los catálogos propios de la agenda: consultorios y estados de cita. Estos sí
 * viven en el microservicio de Agenda, en el 5104.
 *
 * Están separados de los de Catálogos a propósito, para que en el código se
 * vea de dónde sale cada cosa.
 */
@Service()
export class CatalogosAgenda {
  private readonly http = inject(HttpClient);

  obtenerConsultorios(): Promise<ConsultorioOpcion[]> {
    return firstValueFrom(
      this.http.get<ConsultorioOpcion[]>(`${API.agenda}/api/Consultorios`),
    );
  }

  obtenerEstados(): Promise<EstadoCitaOpcion[]> {
    return firstValueFrom(
      this.http.get<EstadoCitaOpcion[]>(`${API.agenda}/api/EstadosCita`),
    );
  }
}
