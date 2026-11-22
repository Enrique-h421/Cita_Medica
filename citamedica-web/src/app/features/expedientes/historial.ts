import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';

import { Expedientes as ServicioExpedientes } from '../../core/servicios/expedientes';
import { Atenciones } from '../../core/servicios/atenciones';
import { Expediente } from '../../core/modelos/expediente';
import { Atencion, ESTADO_ATENCION } from '../../core/modelos/atencion';
import {
  AtencionDiagnostico,
  TIPO_DIAGNOSTICO,
} from '../../core/modelos/diagnostico';
import { fechaCorta, fechaLarga, hora } from '../../core/fechas';

/**
 * El historial clínico de un paciente: su expediente y todas las consultas
 * que se le han registrado, de la más reciente a la más vieja.
 */
@Component({
  selector: 'app-historial',
  templateUrl: './historial.html',
  styleUrl: './historial.css',
})
export class Historial {
  private readonly ruta = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly servicioExpedientes = inject(ServicioExpedientes);
  private readonly servicioAtenciones = inject(Atenciones);

  protected readonly hora = hora;
  protected readonly fechaCorta = fechaCorta;
  protected readonly fechaLarga = fechaLarga;

  protected readonly expediente = signal<Expediente | null>(null);
  protected readonly atenciones = signal<Atencion[]>([]);
  protected readonly cargando = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.cargar();
  }

  private async cargar(): Promise<void> {
    this.cargando.set(true);
    this.error.set(null);

    const pacienteId = Number(this.ruta.snapshot.paramMap.get('pacienteId'));

    if (!Number.isFinite(pacienteId) || pacienteId <= 0) {
      this.error.set('El paciente indicado no es válido.');
      this.cargando.set(false);
      return;
    }

    try {
      // Las dos llamadas salen a la vez: no dependen una de otra, así que no
      // tiene sentido esperar a que termine la primera para lanzar la segunda.
      const [expediente, atenciones] = await Promise.all([
        this.servicioExpedientes.obtenerPorPaciente(pacienteId),
        this.servicioAtenciones.obtenerPorPaciente(pacienteId),
      ]);

      this.expediente.set(expediente);
      this.atenciones.set(atenciones);
    } catch (fallo) {
      this.error.set(this.explicar(fallo));
    } finally {
      this.cargando.set(false);
    }
  }

  protected volver(): void {
    this.router.navigateByUrl('/expedientes');
  }

  protected estaAbierta(atencion: Atencion): boolean {
    return atencion.estado === ESTADO_ATENCION.abierta;
  }

  protected estaAnulada(atencion: Atencion): boolean {
    return atencion.estado === ESTADO_ATENCION.anulada;
  }

  /**
   * Las consultas viejas pueden no traer el arreglo si se guardaron antes de
   * que existieran los diagnósticos, así que se devuelve vacío en vez de dejar
   * que la plantilla tropiece.
   */
  protected diagnosticosDe(atencion: Atencion): AtencionDiagnostico[] {
    return atencion.diagnosticos ?? [];
  }

  protected esPrincipal(diagnostico: AtencionDiagnostico): boolean {
    return diagnostico.tipoDiagnostico === TIPO_DIAGNOSTICO.principal;
  }

  protected claseEstado(estado: string): string {
    switch (estado) {
      case ESTADO_ATENCION.cerrada:
        return 'estado estado-exito';
      case ESTADO_ATENCION.abierta:
        return 'estado estado-aviso';
      default:
        return 'estado estado-neutro';
    }
  }

  /** Texto del estado en lenguaje de clínica, no de base de datos. */
  protected textoEstado(estado: string): string {
    switch (estado) {
      case ESTADO_ATENCION.cerrada:
        return 'Cerrada';
      case ESTADO_ATENCION.abierta:
        return 'En curso';
      case ESTADO_ATENCION.anulada:
        return 'Anulada';
      default:
        return estado;
    }
  }

  private explicar(fallo: unknown): string {
    if (fallo instanceof HttpErrorResponse) {
      if (fallo.status === 0) {
        return 'No se pudo contactar al servidor.';
      }

      if (fallo.status === 404) {
        return 'Este paciente todavía no tiene expediente. Se le abre en su primera consulta.';
      }

      return fallo.error?.errorMessage ?? 'No se pudo cargar el historial.';
    }

    return 'Ocurrió un error inesperado.';
  }
}
