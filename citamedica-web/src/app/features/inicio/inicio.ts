import { Component, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';

import { Auth } from '../../core/auth/auth';
import { Citas } from '../../core/servicios/citas';
import { Expedientes } from '../../core/servicios/expedientes';
import { Catalogos } from '../../core/servicios/catalogos';
import {
  acentoDeEstado,
  Cita,
  ESTADO_CITA,
  sePuedeAtender,
} from '../../core/modelos/cita';
import { claveDia, fechaLarga, hora } from '../../core/fechas';

/**
 * La pantalla de bienvenida: en qué va el día.
 *
 * No tiene endpoints propios. Se arma con lo que ya devuelven la agenda, los
 * expedientes y el padrón de pacientes, y hace las cuentas aquí. Para una
 * clínica de este tamaño traer las citas del día y contarlas en el navegador
 * cuesta menos que mantener un endpoint de resumen que haya que tocar cada
 * vez que cambie un estado.
 */
@Component({
  selector: 'app-inicio',
  imports: [RouterLink],
  templateUrl: './inicio.html',
  styleUrl: './inicio.css',
})
export class Inicio {
  private readonly servicioCitas = inject(Citas);
  private readonly servicioExpedientes = inject(Expedientes);
  private readonly servicioCatalogos = inject(Catalogos);

  protected readonly auth = inject(Auth);
  protected readonly hora = hora;

  protected readonly citas = signal<Cita[]>([]);
  protected readonly totalPacientes = signal<number | null>(null);
  protected readonly totalExpedientes = signal<number | null>(null);

  protected readonly cargando = signal(true);
  protected readonly error = signal<string | null>(null);

  /** El día de hoy en el formato que usa claveDia: "2026-11-22". */
  private readonly hoy = claveDia(new Date());

  protected readonly fechaDeHoy = fechaLarga(new Date());

  protected readonly saludo = Inicio.saludoSegunLaHora(new Date().getHours());

  /** Solo el nombre de pila: "Buenos días, Ana" se lee mejor que el completo. */
  protected readonly nombre = computed(
    () => this.auth.nombreParaMostrar().split(' ')[0],
  );

  /**
   * Las citas de hoy. Si quien entró es médico, solo las suyas: su panel tiene
   * que hablarle de su jornada, no de la de toda la clínica.
   */
  protected readonly citasHoy = computed(() => {
    const mio = this.auth.medicoId();

    return this.citas()
      .filter((c) => claveDia(c.fechaHoraInicio) === this.hoy)
      .filter((c) => mio === null || c.medicoId === mio)
      .sort((a, b) => a.fechaHoraInicio.localeCompare(b.fechaHoraInicio));
  });

  protected readonly atendidas = computed(() => this.contar(ESTADO_CITA.atendida));
  protected readonly confirmadas = computed(() => this.contar(ESTADO_CITA.confirmada));
  protected readonly pendientes = computed(() => this.contar(ESTADO_CITA.pendiente));

  protected readonly noVinieron = computed(
    () => this.contar(ESTADO_CITA.cancelada) + this.contar(ESTADO_CITA.noAsistio),
  );

  /** Las que de verdad cuentan para la jornada: sin canceladas ni ausencias. */
  protected readonly enPie = computed(() => this.citasHoy().length - this.noVinieron());

  protected readonly avance = computed(() => {
    const total = this.enPie();
    return total === 0 ? 0 : Math.round((this.atendidas() / total) * 100);
  });

  /** Las siguientes de la jornada, las que todavía se pueden atender. */
  protected readonly proximas = computed(() =>
    this.citasHoy().filter(sePuedeAtender).slice(0, 4),
  );

  constructor() {
    this.cargar();
  }

  protected async cargar(): Promise<void> {
    this.cargando.set(true);
    this.error.set(null);

    // allSettled y no all: si Catálogos está caído, el panel sigue mostrando
    // la jornada en vez de quedarse en blanco. Lo mismo al revés.
    const [citas, pacientes, expedientes] = await Promise.allSettled([
      this.servicioCitas.obtenerTodas(),
      this.servicioCatalogos.obtenerPacientes(),
      this.auth.puedeAtender()
        ? this.servicioExpedientes.obtenerTodos()
        : Promise.resolve(null),
    ]);

    if (citas.status === 'fulfilled') {
      this.citas.set(citas.value);
    } else {
      this.error.set(this.explicar(citas.reason));
    }

    this.totalPacientes.set(
      pacientes.status === 'fulfilled' ? pacientes.value.length : null,
    );

    this.totalExpedientes.set(
      expedientes.status === 'fulfilled' && expedientes.value !== null
        ? expedientes.value.length
        : null,
    );

    this.cargando.set(false);
  }

  /** El mismo filo de color que lleva la cita en la agenda. */
  protected acento(codigo: string): string {
    return acentoDeEstado(codigo);
  }

  private contar(codigo: string): number {
    return this.citasHoy().filter((c) => c.estadoCodigo === codigo).length;
  }

  private static saludoSegunLaHora(hora: number): string {
    if (hora < 12) {
      return 'Buenos días';
    }

    return hora < 19 ? 'Buenas tardes' : 'Buenas noches';
  }

  private explicar(fallo: unknown): string {
    if (fallo instanceof HttpErrorResponse && fallo.status === 0) {
      return 'No se pudo contactar al servidor. Revise que el sistema esté encendido.';
    }

    return 'No se pudo cargar la jornada de hoy.';
  }
}
