import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

import { Expedientes as ServicioExpedientes } from '../../core/servicios/expedientes';
import { Expediente } from '../../core/modelos/expediente';
import { fechaCorta } from '../../core/fechas';

@Component({
  selector: 'app-expedientes',
  templateUrl: './expedientes.html',
  styleUrl: './expedientes.css',
})
export class ExpedientesLista {
  private readonly servicio = inject(ServicioExpedientes);
  private readonly router = inject(Router);

  protected readonly fechaCorta = fechaCorta;

  protected readonly expedientes = signal<Expediente[]>([]);
  protected readonly cargando = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.cargar();
  }

  protected async cargar(): Promise<void> {
    this.cargando.set(true);
    this.error.set(null);

    try {
      const expedientes = await this.servicio.obtenerTodos();
      this.expedientes.set(expedientes);
    } catch (fallo) {
      this.error.set(this.explicar(fallo));
    } finally {
      this.cargando.set(false);
    }
  }

  protected abrir(expediente: Expediente): void {
    this.router.navigate(['/expedientes', expediente.pacienteId]);
  }

  private explicar(fallo: unknown): string {
    if (fallo instanceof HttpErrorResponse) {
      if (fallo.status === 0) {
        return 'No se pudo contactar al servidor.';
      }

      return fallo.error?.errorMessage ?? 'No se pudieron cargar los expedientes.';
    }

    return 'Ocurrió un error inesperado.';
  }
}
