import { Component, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';

import { Citas } from '../../core/servicios/citas';
import { Catalogos } from '../../core/servicios/catalogos';
import { CatalogosAgenda } from '../../core/servicios/agenda';
import {
  ConsultorioOpcion,
  EstadoCitaOpcion,
  MedicoOpcion,
} from '../../core/modelos/catalogos';
import { Paciente } from '../../core/modelos/paciente';
import { ESTADO_CITA } from '../../core/modelos/cita';

/** Duración por defecto de una consulta, en minutos. */
const DURACION_POR_DEFECTO = 30;

@Component({
  selector: 'app-nueva-cita',
  templateUrl: './nueva-cita.html',
  styleUrl: './nueva-cita.css',
})
export class NuevaCita {
  private readonly servicioCitas = inject(Citas);
  private readonly catalogos = inject(Catalogos);
  private readonly catalogosAgenda = inject(CatalogosAgenda);
  private readonly router = inject(Router);

  protected readonly pacientes = signal<Paciente[]>([]);
  protected readonly medicos = signal<MedicoOpcion[]>([]);
  protected readonly consultorios = signal<ConsultorioOpcion[]>([]);
  protected readonly estados = signal<EstadoCitaOpcion[]>([]);

  protected readonly cargando = signal(true);
  protected readonly guardando = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly pacienteId = signal<number | null>(null);
  protected readonly medicoId = signal<number | null>(null);
  protected readonly consultorioId = signal<number | null>(null);
  protected readonly estadoCitaId = signal<number | null>(null);
  protected readonly fecha = signal(this.hoy());
  protected readonly horaInicio = signal('09:00');
  protected readonly horaFin = signal(this.sumarMinutos('09:00', DURACION_POR_DEFECTO));
  protected readonly motivo = signal('');
  protected readonly observaciones = signal('');

  /**
   * La especialidad no se pide: se toma la principal del médico elegido.
   * Pedirla aparte sería hacerle escribir a recepción algo que el sistema ya
   * sabe, y abriría la puerta a mandar una especialidad que ese médico no
   * ejerce.
   */
  protected readonly medicoElegido = computed(() =>
    this.medicos().find((m) => m.medicoId === this.medicoId()) ?? null,
  );

  protected readonly horaFinInvalida = computed(() => {
    const fin = this.horaFin();
    return fin !== '' && fin <= this.horaInicio();
  });

  protected readonly puedeGuardar = computed(
    () =>
      this.pacienteId() !== null &&
      this.medicoId() !== null &&
      this.estadoCitaId() !== null &&
      this.fecha() !== '' &&
      this.horaInicio() !== '' &&
      !this.horaFinInvalida() &&
      !this.guardando(),
  );

  constructor() {
    this.cargarCatalogos();
  }

  private async cargarCatalogos(): Promise<void> {
    this.cargando.set(true);
    this.error.set(null);

    try {
      // Las cuatro listas salen a la vez. Dos van al microservicio de
      // Catálogos y dos al de Agenda: para el usuario es una sola pantalla,
      // pero por detrás son dos servicios distintos respondiendo en paralelo.
      const [pacientes, medicos, consultorios, estados] = await Promise.all([
        this.catalogos.obtenerPacientes(),
        this.catalogos.obtenerMedicos(),
        this.catalogosAgenda.obtenerConsultorios(),
        this.catalogosAgenda.obtenerEstados(),
      ]);

      this.pacientes.set(pacientes.filter((p) => p.activo));
      this.medicos.set(medicos.filter((m) => m.activo));
      this.consultorios.set(consultorios.filter((c) => c.activo));
      this.estados.set(estados);

      // Una cita nueva nace confirmada salvo que se diga otra cosa.
      const confirmada = estados.find((e) => e.codigo === ESTADO_CITA.confirmada);
      this.estadoCitaId.set(confirmada?.estadoCitaId ?? estados[0]?.estadoCitaId ?? null);
    } catch (fallo) {
      this.error.set(this.explicar(fallo));
    } finally {
      this.cargando.set(false);
    }
  }

  // ----------------------------- Cambios --------------------------------

  protected cambiarPaciente(evento: Event): void {
    this.pacienteId.set(this.valorNumerico(evento));
  }

  protected cambiarMedico(evento: Event): void {
    this.medicoId.set(this.valorNumerico(evento));
  }

  protected cambiarConsultorio(evento: Event): void {
    this.consultorioId.set(this.valorNumerico(evento));
  }

  protected cambiarEstado(evento: Event): void {
    this.estadoCitaId.set(this.valorNumerico(evento));
  }

  protected cambiarFecha(evento: Event): void {
    this.fecha.set(this.valorTexto(evento));
  }

  protected cambiarHoraInicio(evento: Event): void {
    const nueva = this.valorTexto(evento);
    this.horaInicio.set(nueva);

    // La hora de fin se corre sola para mantener la duración, salvo que el
    // usuario ya la haya puesto antes que la de inicio.
    if (nueva !== '') {
      this.horaFin.set(this.sumarMinutos(nueva, DURACION_POR_DEFECTO));
    }
  }

  protected cambiarHoraFin(evento: Event): void {
    this.horaFin.set(this.valorTexto(evento));
  }

  protected cambiarMotivo(evento: Event): void {
    this.motivo.set(this.valorTexto(evento));
  }

  protected cambiarObservaciones(evento: Event): void {
    this.observaciones.set(this.valorTexto(evento));
  }

  // ----------------------------- Guardar --------------------------------

  protected async guardar(): Promise<void> {
    if (!this.puedeGuardar()) {
      return;
    }

    this.guardando.set(true);
    this.error.set(null);

    try {
      await this.servicioCitas.crear({
        pacienteId: this.pacienteId()!,
        medicoId: this.medicoId()!,
        especialidadId: this.medicoElegido()?.especialidadPrincipalId ?? null,
        consultorioId: this.consultorioId(),
        estadoCitaId: this.estadoCitaId()!,
        fechaHoraInicio: `${this.fecha()}T${this.horaInicio()}:00`,
        fechaHoraFin: this.horaFin() ? `${this.fecha()}T${this.horaFin()}:00` : null,
        motivo: this.motivo().trim() || null,
        observaciones: this.observaciones().trim() || null,
      });

      await this.router.navigateByUrl('/agenda');
    } catch (fallo) {
      this.error.set(this.explicar(fallo));
      this.guardando.set(false);
    }
  }

  protected cancelar(): void {
    this.router.navigateByUrl('/agenda');
  }

  // ----------------------------- Auxiliares ------------------------------

  private valorTexto(evento: Event): string {
    return (evento.target as HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement).value;
  }

  private valorNumerico(evento: Event): number | null {
    const texto = this.valorTexto(evento);
    return texto === '' ? null : Number(texto);
  }

  /** La fecha de hoy en formato yyyy-mm-dd, que es lo que pide el input date. */
  private hoy(): string {
    const ahora = new Date();
    const mes = String(ahora.getMonth() + 1).padStart(2, '0');
    const dia = String(ahora.getDate()).padStart(2, '0');

    return `${ahora.getFullYear()}-${mes}-${dia}`;
  }

  /** "09:00" + 30 => "09:30". Da la vuelta al pasar de medianoche. */
  private sumarMinutos(hora: string, minutos: number): string {
    const [h, m] = hora.split(':').map(Number);

    if (!Number.isFinite(h) || !Number.isFinite(m)) {
      return hora;
    }

    const total = (h * 60 + m + minutos) % (24 * 60);

    const horas = String(Math.floor(total / 60)).padStart(2, '0');
    const resto = String(total % 60).padStart(2, '0');

    return `${horas}:${resto}`;
  }

  private explicar(fallo: unknown): string {
    if (fallo instanceof HttpErrorResponse) {
      if (fallo.status === 0) {
        return 'No se pudo contactar al servidor.';
      }

      if (fallo.status === 403) {
        return 'Su rol no puede agendar citas.';
      }

      if (fallo.status === 503) {
        return 'El catálogo de pacientes y médicos no está disponible en este momento.';
      }

      return fallo.error?.errorMessage ?? 'No se pudo agendar la cita.';
    }

    return 'Ocurrió un error inesperado.';
  }
}
