import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router } from '@angular/router';
import { form, FormField } from '@angular/forms/signals';

import { Auth } from '../../core/auth/auth';
import { Citas } from '../../core/servicios/citas';
import { Atenciones } from '../../core/servicios/atenciones';
import { Diagnosticos } from '../../core/servicios/diagnosticos';
import { Cita } from '../../core/modelos/cita';
import { Atencion, ESTADO_ATENCION } from '../../core/modelos/atencion';
import {
  AtencionDiagnostico,
  Diagnostico,
  TIPO_DIAGNOSTICO,
} from '../../core/modelos/diagnostico';
import { fechaCorta, hora } from '../../core/fechas';

/**
 * La consulta médica, de principio a fin.
 *
 * Al entrar se mira si el paciente ya tiene una consulta abierta. Si la tiene,
 * se retoma — así cerrar el navegador a media consulta no pierde lo escrito.
 * Si no la tiene, se ofrece abrirla.
 */
@Component({
  selector: 'app-atender',
  imports: [FormField],
  templateUrl: './atender.html',
  styleUrl: './atender.css',
})
export class Atender {
  private readonly ruta = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly servicioCitas = inject(Citas);
  private readonly servicioAtenciones = inject(Atenciones);
  private readonly servicioDiagnosticos = inject(Diagnosticos);

  protected readonly auth = inject(Auth);

  protected readonly hora = hora;
  protected readonly fechaCorta = fechaCorta;

  protected readonly cita = signal<Cita | null>(null);
  protected readonly atencion = signal<Atencion | null>(null);

  protected readonly cargando = signal(true);
  protected readonly trabajando = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly hallazgos = signal({
    motivoConsulta: '',
    enfermedadActual: '',
    exploracionFisica: '',
    observaciones: '',
  });

  protected readonly formulario = form(this.hallazgos);

  protected readonly consultaAbierta = computed(
    () => this.atencion()?.estado === ESTADO_ATENCION.abierta,
  );

  // ----------------------------- Diagnósticos -----------------------------

  protected readonly diagnosticos = signal<AtencionDiagnostico[]>([]);
  protected readonly busqueda = signal('');
  protected readonly resultados = signal<Diagnostico[]>([]);
  protected readonly buscando = signal(false);
  protected readonly guardandoDiagnostico = signal(false);
  protected readonly errorDiagnostico = signal<string | null>(null);

  /** Mínimo de letras antes de ir al servidor. Con una sola, todo coincide. */
  private static readonly MinimoBusqueda = 2;

  private temporizador: ReturnType<typeof setTimeout> | null = null;

  private readonly alDestruir = inject(DestroyRef);

  constructor() {
    // Si el médico sale de la pantalla con una búsqueda a medio escribir, el
    // temporizador pendiente se cancela en lugar de dispararse en el vacío.
    this.alDestruir.onDestroy(() => this.cancelarBusquedaPendiente());

    this.cargar();
  }

  private async cargar(): Promise<void> {
    this.cargando.set(true);
    this.error.set(null);

    const citaId = Number(this.ruta.snapshot.paramMap.get('citaId'));

    if (!Number.isFinite(citaId) || citaId <= 0) {
      this.error.set('La cita indicada no es válida.');
      this.cargando.set(false);
      return;
    }

    try {
      const cita = await this.servicioCitas.obtenerPorId(citaId);
      this.cita.set(cita);

      // Si quedó una consulta abierta de este paciente, se retoma en lugar de
      // intentar abrir otra: el servidor no permite dos abiertas a la vez.
      const historial = await this.servicioAtenciones.obtenerPorPaciente(cita.pacienteId);
      const abierta = historial.find((a) => a.estado === ESTADO_ATENCION.abierta);

      if (abierta) {
        this.adoptar(abierta);
      }
    } catch (fallo) {
      this.error.set(this.explicar(fallo));
    } finally {
      this.cargando.set(false);
    }
  }

  protected async iniciar(): Promise<void> {
    const cita = this.cita();

    if (!cita || this.trabajando()) {
      return;
    }

    this.trabajando.set(true);
    this.error.set(null);

    try {
      const atencion = await this.servicioAtenciones.abrir({
        pacienteId: cita.pacienteId,
        medicoId: cita.medicoId,
        citaId: cita.citaId,
        motivoConsulta: cita.motivo,
      });

      this.adoptar(atencion);
    } catch (fallo) {
      this.error.set(this.explicar(fallo));
    } finally {
      this.trabajando.set(false);
    }
  }

  protected async cerrar(): Promise<void> {
    const atencion = this.atencion();

    if (!atencion || this.trabajando()) {
      return;
    }

    this.trabajando.set(true);
    this.error.set(null);

    try {
      await this.servicioAtenciones.cerrar(atencion.atencionId, this.hallazgos());

      // La cita ya quedó en "Atendida" del lado del servidor; al volver a la
      // agenda se ve el cambio.
      await this.router.navigateByUrl('/agenda');
    } catch (fallo) {
      this.error.set(this.explicar(fallo));
      this.trabajando.set(false);
    }
  }

  protected volver(): void {
    this.router.navigateByUrl('/agenda');
  }

  /** Mete la atención en el estado del componente y precarga el formulario. */
  private adoptar(atencion: Atencion): void {
    this.atencion.set(atencion);

    this.hallazgos.set({
      motivoConsulta: atencion.motivoConsulta ?? '',
      enfermedadActual: atencion.enfermedadActual ?? '',
      exploracionFisica: atencion.exploracionFisica ?? '',
      observaciones: atencion.observaciones ?? '',
    });

    // Los diagnósticos vienen dentro de la atención, así que retomar una
    // consulta a medias los trae de una vez: no hace falta otra petición.
    this.diagnosticos.set(atencion.diagnosticos ?? []);
  }

  // ----------------------------- Diagnósticos -----------------------------

  /**
   * Se espera a que el médico deje de escribir antes de buscar. Sin esto sale
   * una petición por cada tecla y las respuestas llegan desordenadas: se
   * escribe "faringitis" y en pantalla quedan los resultados de "farin".
   */
  protected alEscribir(evento: Event): void {
    this.busqueda.set((evento.target as HTMLInputElement).value);
    this.cancelarBusquedaPendiente();
    this.temporizador = setTimeout(() => this.buscar(), 250);
  }

  private cancelarBusquedaPendiente(): void {
    if (this.temporizador !== null) {
      clearTimeout(this.temporizador);
      this.temporizador = null;
    }
  }

  private async buscar(): Promise<void> {
    const texto = this.busqueda().trim();

    if (texto.length < Atender.MinimoBusqueda) {
      this.resultados.set([]);
      return;
    }

    this.buscando.set(true);

    try {
      const encontrados = await this.servicioDiagnosticos.buscar(texto);

      // Los que ya están puestos no se vuelven a ofrecer: elegirlos solo
      // produciría un error de duplicado.
      const puestos = new Set(this.diagnosticos().map((d) => d.diagnosticoId));
      this.resultados.set(encontrados.filter((d) => !puestos.has(d.diagnosticoId)));
    } catch {
      // Que falle el buscador no debe tumbar la consulta: se queda sin
      // sugerencias y el médico sigue documentando.
      this.resultados.set([]);
    } finally {
      this.buscando.set(false);
    }
  }

  protected async agregarDiagnostico(diagnostico: Diagnostico): Promise<void> {
    const atencion = this.atencion();

    if (!atencion || this.guardandoDiagnostico()) {
      return;
    }

    this.guardandoDiagnostico.set(true);
    this.errorDiagnostico.set(null);

    try {
      // El tipo va en null a propósito: el servidor pone PRINCIPAL en el
      // primero y SECUNDARIO en los demás.
      const agregado = await this.servicioDiagnosticos.agregar(atencion.atencionId, {
        diagnosticoId: diagnostico.diagnosticoId,
        tipoDiagnostico: null,
        observaciones: null,
      });

      this.diagnosticos.update((lista) => [...lista, agregado]);
      this.limpiarBusqueda();
    } catch (fallo) {
      this.errorDiagnostico.set(this.explicarDiagnostico(fallo));
    } finally {
      this.guardandoDiagnostico.set(false);
    }
  }

  protected async quitarDiagnostico(diagnostico: AtencionDiagnostico): Promise<void> {
    const atencion = this.atencion();

    if (!atencion || this.guardandoDiagnostico()) {
      return;
    }

    this.guardandoDiagnostico.set(true);
    this.errorDiagnostico.set(null);

    try {
      await this.servicioDiagnosticos.quitar(
        atencion.atencionId,
        diagnostico.atencionDiagnosticoId,
      );

      this.diagnosticos.update((lista) =>
        lista.filter(
          (d) => d.atencionDiagnosticoId !== diagnostico.atencionDiagnosticoId,
        ),
      );
    } catch (fallo) {
      this.errorDiagnostico.set(this.explicarDiagnostico(fallo));
    } finally {
      this.guardandoDiagnostico.set(false);
    }
  }

  protected limpiarBusqueda(): void {
    this.cancelarBusquedaPendiente();
    this.busqueda.set('');
    this.resultados.set([]);
  }

  /** Enter en el buscador no debe enviar el formulario y cerrar la consulta. */
  protected evitarEnvio(evento: Event): void {
    evento.preventDefault();
  }

  protected esPrincipal(diagnostico: AtencionDiagnostico): boolean {
    return diagnostico.tipoDiagnostico === TIPO_DIAGNOSTICO.principal;
  }

  protected textoTipo(diagnostico: AtencionDiagnostico): string {
    return this.esPrincipal(diagnostico) ? 'Principal' : 'Secundario';
  }

  protected claseTipo(diagnostico: AtencionDiagnostico): string {
    return this.esPrincipal(diagnostico)
      ? 'etiqueta etiqueta-principal'
      : 'etiqueta etiqueta-secundario';
  }

  /**
   * Los mensajes del servidor aquí son buenos ("ya tiene un diagnóstico
   * principal", "ya está registrado"), así que se muestran tal cual en vez de
   * taparlos con un texto genérico.
   */
  private explicarDiagnostico(fallo: unknown): string {
    if (fallo instanceof HttpErrorResponse) {
      if (fallo.status === 0) {
        return 'No se pudo contactar al servidor.';
      }

      if (fallo.status === 403) {
        return 'Su rol no puede registrar diagnósticos.';
      }

      return fallo.error?.errorMessage ?? 'No se pudo guardar el diagnóstico.';
    }

    return 'Ocurrió un error inesperado.';
  }

  private explicar(fallo: unknown): string {
    if (fallo instanceof HttpErrorResponse) {
      if (fallo.status === 0) {
        return 'No se pudo contactar al servidor.';
      }

      if (fallo.status === 403) {
        return 'Su rol no puede registrar atenciones.';
      }

      if (fallo.status === 404) {
        return 'No se encontró la cita.';
      }

      return fallo.error?.errorMessage ?? 'No se pudo completar la operación.';
    }

    return 'Ocurrió un error inesperado.';
  }
}
