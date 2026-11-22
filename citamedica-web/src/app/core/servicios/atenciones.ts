import { inject, Service } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { API } from '../api.config';
import { AbrirAtencion, Atencion, CerrarAtencion } from '../modelos/atencion';

@Service()
export class Atenciones {
  private readonly http = inject(HttpClient);

  private readonly base = `${API.agenda}/api/Atenciones`;

  obtenerPorId(atencionId: number): Promise<Atencion> {
    return firstValueFrom(this.http.get<Atencion>(`${this.base}/${atencionId}`));
  }

  /** Historial clínico del paciente, de la atención más reciente a la más vieja. */
  obtenerPorPaciente(pacienteId: number): Promise<Atencion[]> {
    return firstValueFrom(this.http.get<Atencion[]>(`${this.base}/paciente/${pacienteId}`));
  }

  /**
   * Abre la consulta. Si el paciente no tenía expediente, el servidor se lo
   * abre solo y lo devuelve ya con su número.
   */
  abrir(datos: AbrirAtencion): Promise<Atencion> {
    return firstValueFrom(this.http.post<Atencion>(this.base, datos));
  }

  /** La cierra con los hallazgos y deja la cita de origen en "Atendida". */
  cerrar(atencionId: number, hallazgos: CerrarAtencion): Promise<Atencion> {
    return firstValueFrom(
      this.http.put<Atencion>(`${this.base}/${atencionId}/cerrar`, hallazgos),
    );
  }
}
