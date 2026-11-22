import { inject, Service } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { API } from '../api.config';
import {
  AgregarDiagnostico,
  AtencionDiagnostico,
  Diagnostico,
} from '../modelos/diagnostico';

/**
 * El catálogo CIE-10 y los diagnósticos de cada consulta.
 *
 * Los dos viven en Agenda: el catálogo porque se consulta desde la pantalla de
 * atención, y los diagnósticos porque cuelgan de la atención y no existen sin
 * ella.
 */
@Service()
export class Diagnosticos {
  private readonly http = inject(HttpClient);

  private readonly catalogo = `${API.agenda}/api/Diagnosticos`;
  private readonly atenciones = `${API.agenda}/api/Atenciones`;

  /**
   * Busca en el catálogo por nombre o por código. Sin texto devuelve los
   * primeros en orden alfabético, que es lo que hace falta para llenar el
   * buscador antes de que el médico escriba algo.
   */
  buscar(texto: string, tope = 20): Promise<Diagnostico[]> {
    let parametros = new HttpParams().set('tope', tope);

    if (texto.trim().length > 0) {
      parametros = parametros.set('busqueda', texto.trim());
    }

    return firstValueFrom(
      this.http.get<Diagnostico[]>(this.catalogo, { params: parametros }),
    );
  }

  /** Los diagnósticos que el médico le puso a una consulta. */
  deAtencion(atencionId: number): Promise<AtencionDiagnostico[]> {
    return firstValueFrom(
      this.http.get<AtencionDiagnostico[]>(
        `${this.atenciones}/${atencionId}/diagnosticos`,
      ),
    );
  }

  /** Agrega uno. Solo funciona mientras la consulta siga abierta. */
  agregar(
    atencionId: number,
    datos: AgregarDiagnostico,
  ): Promise<AtencionDiagnostico> {
    return firstValueFrom(
      this.http.post<AtencionDiagnostico>(
        `${this.atenciones}/${atencionId}/diagnosticos`,
        datos,
      ),
    );
  }

  /** Quita uno mal puesto. También solo con la consulta abierta. */
  quitar(atencionId: number, atencionDiagnosticoId: number): Promise<void> {
    return firstValueFrom(
      this.http.delete<void>(
        `${this.atenciones}/${atencionId}/diagnosticos/${atencionDiagnosticoId}`,
      ),
    );
  }
}
