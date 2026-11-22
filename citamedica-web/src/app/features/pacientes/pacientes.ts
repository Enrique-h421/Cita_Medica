import { Component, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

import { Auth } from '../../core/auth/auth';
import { Catalogos } from '../../core/servicios/catalogos';
import { Paciente } from '../../core/modelos/paciente';
import { fechaCorta } from '../../core/fechas';

@Component({
  selector: 'app-pacientes',
  templateUrl: './pacientes.html',
  styleUrl: './pacientes.css',
})
export class PacientesLista {
  private readonly servicio = inject(Catalogos);
  private readonly router = inject(Router);

  protected readonly auth = inject(Auth);

  protected readonly fechaCorta = fechaCorta;

  protected readonly pacientes = signal<Paciente[]>([]);
  protected readonly cargando = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly busqueda = signal('');

  protected readonly visibles = computed(() => {
    const texto = this.busqueda().trim().toLowerCase();

    if (texto === '') {
      return this.pacientes();
    }

    // Se busca por nombre, código y documento a la vez: en una recepción
    // nadie se acuerda de cuál de los tres tiene a mano.
    return this.pacientes().filter(
      (p) =>
        p.nombreCompleto.toLowerCase().includes(texto) ||
        p.codigoPaciente.toLowerCase().includes(texto) ||
        p.numeroDocumento.toLowerCase().includes(texto),
    );
  });

  constructor() {
    this.cargar();
  }

  protected async cargar(): Promise<void> {
    this.cargando.set(true);
    this.error.set(null);

    try {
      this.pacientes.set(await this.servicio.obtenerPacientes());
    } catch (fallo) {
      this.error.set(this.explicar(fallo));
    } finally {
      this.cargando.set(false);
    }
  }

  protected buscar(evento: Event): void {
    this.busqueda.set((evento.target as HTMLInputElement).value);
  }

  protected registrar(): void {
    this.router.navigateByUrl('/pacientes/nuevo');
  }

  protected verExpediente(paciente: Paciente): void {
    this.router.navigate(['/expedientes', paciente.pacienteId]);
  }

  private explicar(fallo: unknown): string {
    if (fallo instanceof HttpErrorResponse) {
      if (fallo.status === 0) {
        return 'No se pudo contactar al servidor.';
      }

      return fallo.error?.errorMessage ?? 'No se pudieron cargar los pacientes.';
    }

    return 'Ocurrió un error inesperado.';
  }
}
