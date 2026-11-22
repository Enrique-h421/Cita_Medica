import { Component, computed, inject, signal, WritableSignal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

import { Catalogos } from '../../core/servicios/catalogos';
import {
  ESTADOS_CIVILES,
  TIPOS_DOCUMENTO,
  TIPOS_SANGRE,
} from '../../core/modelos/paciente';

@Component({
  selector: 'app-nuevo-paciente',
  templateUrl: './nuevo-paciente.html',
  styleUrl: './nuevo-paciente.css',
})
export class NuevoPaciente {
  private readonly servicio = inject(Catalogos);
  private readonly router = inject(Router);

  protected readonly tiposDocumento = TIPOS_DOCUMENTO;
  protected readonly estadosCiviles = ESTADOS_CIVILES;
  protected readonly tiposSangre = TIPOS_SANGRE;

  protected readonly guardando = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly codigo = signal('');
  protected readonly tipoDocumento = signal('DUI');
  protected readonly numeroDocumento = signal('');
  protected readonly nombres = signal('');
  protected readonly apellidos = signal('');
  protected readonly fechaNacimiento = signal('');
  protected readonly sexo = signal('');
  protected readonly estadoCivil = signal('');
  protected readonly telefono = signal('');
  protected readonly email = signal('');
  protected readonly direccion = signal('');
  protected readonly ciudad = signal('San Salvador');
  protected readonly pais = signal('El Salvador');
  protected readonly ocupacion = signal('');
  protected readonly tipoSangre = signal('');

  protected readonly puedeGuardar = computed(
    () =>
      this.codigo().trim() !== '' &&
      this.numeroDocumento().trim() !== '' &&
      this.nombres().trim() !== '' &&
      this.apellidos().trim() !== '' &&
      this.fechaNacimiento() !== '' &&
      this.sexo() !== '' &&
      !this.guardando(),
  );

  constructor() {
    this.sugerirCodigo();
  }

  /**
   * Propone el siguiente código correlativo mirando los que ya existen.
   *
   * Es solo una sugerencia editable: si la clínica usa otra numeración, se
   * escribe a mano. Evita que recepción tenga que ir a averiguar cuál sigue.
   */
  private async sugerirCodigo(): Promise<void> {
    try {
      const pacientes = await this.servicio.obtenerPacientes();

      const mayor = pacientes.reduce((maximo, paciente) => {
        const digitos = paciente.codigoPaciente.match(/\d+/);
        const numero = digitos ? Number(digitos[0]) : 0;

        return Number.isFinite(numero) && numero > maximo ? numero : maximo;
      }, 0);

      this.codigo.set(`PAC-${String(mayor + 1).padStart(4, '0')}`);
    } catch {
      // Si no se puede consultar, no pasa nada: el campo queda vacío y se
      // escribe a mano. No vale la pena molestar al usuario por esto.
    }
  }

  // ----------------------------- Cambios --------------------------------

  /**
   * Un solo manejador para todos los campos: se le pasa la señal que hay que
   * actualizar. Todas son de texto, así que no hace falta una función por
   * campo ni indexar por nombre.
   */
  protected cambiar(destino: WritableSignal<string>, evento: Event): void {
    destino.set(this.texto(evento));
  }

  // ----------------------------- Guardar --------------------------------

  protected async guardar(): Promise<void> {
    if (!this.puedeGuardar()) {
      return;
    }

    this.guardando.set(true);
    this.error.set(null);

    try {
      await this.servicio.crearPaciente({
        codigoPaciente: this.codigo().trim(),
        tipoDocumento: this.tipoDocumento(),
        numeroDocumento: this.numeroDocumento().trim(),
        nombres: this.nombres().trim(),
        apellidos: this.apellidos().trim(),
        fechaNacimiento: this.fechaNacimiento(),
        sexo: this.sexo(),
        estadoCivil: this.estadoCivil() || null,
        telefono: this.telefono().trim() || null,
        email: this.email().trim() || null,
        direccion: this.direccion().trim() || null,
        ciudad: this.ciudad().trim() || null,
        pais: this.pais().trim() || null,
        ocupacion: this.ocupacion().trim() || null,
        tipoSangre: this.tipoSangre() || null,
        activo: true,
      });

      await this.router.navigateByUrl('/pacientes');
    } catch (fallo) {
      this.error.set(this.explicar(fallo));
      this.guardando.set(false);
    }
  }

  protected cancelar(): void {
    this.router.navigateByUrl('/pacientes');
  }

  private texto(evento: Event): string {
    return (evento.target as HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement).value;
  }

  private explicar(fallo: unknown): string {
    if (fallo instanceof HttpErrorResponse) {
      if (fallo.status === 0) {
        return 'No se pudo contactar al servidor.';
      }

      if (fallo.status === 403) {
        return 'Su rol no puede registrar pacientes.';
      }

      return (
        fallo.error?.errorMessage ??
        'No se pudo registrar el paciente. Revise que el código y el documento no estén repetidos.'
      );
    }

    return 'Ocurrió un error inesperado.';
  }
}
