import { Component, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

import { Auth } from '../../core/auth/auth';
import { Citas } from '../../core/servicios/citas';
import { acentoDeEstado, Cita, sePuedeAtender } from '../../core/modelos/cita';
import { fechaCorta, hora } from '../../core/fechas';

/** Opción "todas" del filtro de estados. */
const TODAS = 'TODAS';

@Component({
  selector: 'app-agenda',
  templateUrl: './agenda.html',
  styleUrl: './agenda.css',
})
export class Agenda {
  private readonly servicio = inject(Citas);
  private readonly router = inject(Router);

  protected readonly auth = inject(Auth);

  // Las funciones de formato se exponen como campos porque la plantilla solo
  // puede llamar a miembros de la clase, no a funciones importadas.
  protected readonly hora = hora;
  protected readonly fechaCorta = fechaCorta;
  protected readonly sePuedeAtender = sePuedeAtender;

  protected readonly citas = signal<Cita[]>([]);
  protected readonly cargando = signal(true);
  protected readonly error = signal<string | null>(null);

  protected readonly filtroEstado = signal<string>(TODAS);
  protected readonly soloMias = signal(false);

  /** El filtro "solo mis citas" únicamente tiene sentido para un médico. */
  protected readonly puedeFiltrarPorMedico = computed(() => this.auth.medicoId() !== null);

  /**
   * Los estados que se ofrecen como filtro salen de los datos, no de una lista
   * fija: si mañana el ingeniero agrega un estado nuevo a la tabla, aparece
   * aquí solo, sin tocar el frontend.
   */
  protected readonly estadosDisponibles = computed(() => {
    const vistos = new Map<string, string>();

    for (const cita of this.citas()) {
      vistos.set(cita.estadoCodigo, cita.estadoNombre);
    }

    return [...vistos].map(([codigo, nombre]) => ({ codigo, nombre }));
  });

  protected readonly visibles = computed(() => {
    const estado = this.filtroEstado();
    const medicoId = this.auth.medicoId();
    const soloMias = this.soloMias() && medicoId !== null;

    return this.citas().filter((cita) => {
      if (estado !== TODAS && cita.estadoCodigo !== estado) {
        return false;
      }

      if (soloMias && cita.medicoId !== medicoId) {
        return false;
      }

      return true;
    });
  });

  protected readonly hayFiltroActivo = computed(
    () => this.filtroEstado() !== TODAS || this.soloMias(),
  );

  constructor() {
    this.cargar();
  }

  protected async cargar(): Promise<void> {
    this.cargando.set(true);
    this.error.set(null);

    try {
      const citas = await this.servicio.obtenerTodas();

      // De la más reciente a la más vieja: lo que importa hoy va arriba.
      this.citas.set(
        [...citas].sort(
          (a, b) =>
            new Date(b.fechaHoraInicio).getTime() - new Date(a.fechaHoraInicio).getTime(),
        ),
      );
    } catch (fallo) {
      this.error.set(this.explicar(fallo));
    } finally {
      this.cargando.set(false);
    }
  }

  protected filtrarPor(codigo: string): void {
    this.filtroEstado.set(codigo);
  }

  protected alternarSoloMias(): void {
    this.soloMias.update((valor) => !valor);
  }

  protected limpiarFiltros(): void {
    this.filtroEstado.set(TODAS);
    this.soloMias.set(false);
  }

  protected atender(cita: Cita): void {
    this.router.navigate(['/atender', cita.citaId]);
  }

  protected agendar(): void {
    this.router.navigateByUrl('/citas/nueva');
  }

  /** Clase CSS del distintivo según el estado, para el semáforo de colores. */
  /** El color del filo de la fila. Sale del modelo, igual que en el inicio. */
  protected acento(codigo: string): string {
    return acentoDeEstado(codigo);
  }

  protected claseEstado(codigo: string): string {
    switch (codigo) {
      case 'ATEN':
        return 'estado estado-exito';
      case 'CONF':
        return 'estado estado-info';
      case 'PEND':
        return 'estado estado-aviso';
      case 'CANC':
        return 'estado estado-error';
      default:
        return 'estado estado-neutro';
    }
  }

  protected readonly TODAS = TODAS;

  private explicar(fallo: unknown): string {
    if (fallo instanceof HttpErrorResponse) {
      if (fallo.status === 0) {
        return 'No se pudo contactar al servidor. Revise que el sistema esté encendido.';
      }

      if (fallo.status === 503) {
        return 'El catálogo de pacientes y médicos no está disponible en este momento.';
      }

      return fallo.error?.errorMessage ?? 'No se pudo cargar la agenda.';
    }

    return 'Ocurrió un error inesperado al cargar la agenda.';
  }
}
