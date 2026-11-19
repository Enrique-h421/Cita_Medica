import { computed, inject, Service, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { firstValueFrom } from 'rxjs';

import { API } from '../api.config';
import { Credenciales, ROLES, Sesion } from './modelos';

/** Dónde se guarda la sesión en el navegador. */
const CLAVE_SESION = 'citamedica.sesion';

/**
 * Quién está usando el sistema.
 *
 * Guarda la sesión en localStorage para que recargar la página no te saque.
 * Ojo con lo que eso significa: el token queda escrito en el navegador. Para
 * una clínica de verdad habría que valorar si conviene, pero para un sistema
 * interno que se usa toda la jornada es lo habitual — la alternativa es que
 * el usuario tenga que volver a entrar cada vez que refresca.
 *
 * El token caduca a las 8 horas y el servidor lo rechaza solo, así que aquí
 * se descarta en cuanto se nota vencido en lugar de arrastrarlo.
 */
@Service()
export class Auth {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);

  private readonly _sesion = signal<Sesion | null>(this.recuperarSesionGuardada());

  readonly sesion = this._sesion.asReadonly();

  readonly autenticado = computed(() => this._sesion() !== null);

  readonly rol = computed(() => this._sesion()?.rol ?? null);

  readonly rolParaMostrar = computed(() => {
    const sesion = this._sesion();
    return sesion?.rolNombre ?? sesion?.rol ?? '';
  });

  readonly nombreParaMostrar = computed(() => {
    const sesion = this._sesion();
    return sesion?.nombreCompleto ?? sesion?.nombreUsuario ?? '';
  });

  /** Ficha de médico del usuario. Es lo que permite mostrar "mis citas". */
  readonly medicoId = computed(() => this._sesion()?.medicoId ?? null);

  /** Quién puede escribir en el expediente clínico. */
  readonly puedeAtender = computed(() =>
    this.tieneAlgunRol(ROLES.administrador, ROLES.medico),
  );

  /** Quién puede agendar, mover y cancelar citas. */
  readonly puedeAgendar = computed(() =>
    this.tieneAlgunRol(ROLES.administrador, ROLES.recepcion),
  );

  readonly esAdministrador = computed(() => this.rol() === ROLES.administrador);

  async iniciarSesion(credenciales: Credenciales): Promise<void> {
    const sesion = await firstValueFrom(
      this.http.post<Sesion>(`${API.catalogos}/api/Auth/login`, credenciales),
    );

    this._sesion.set(sesion);
    localStorage.setItem(CLAVE_SESION, JSON.stringify(sesion));
  }

  cerrarSesion(): void {
    this._sesion.set(null);
    localStorage.removeItem(CLAVE_SESION);
    this.router.navigateByUrl('/login');
  }

  /** El token para la cabecera Authorization. Lo usa el interceptor. */
  token(): string | null {
    return this._sesion()?.token ?? null;
  }

  tieneAlgunRol(...roles: string[]): boolean {
    const rol = this.rol();
    return rol !== null && roles.includes(rol);
  }

  /**
   * Lee la sesión guardada al arrancar la aplicación. Si está vencida o el
   * contenido quedó corrupto, se descarta en silencio: es preferible mandar
   * al usuario al login que dejarlo entrar con un token que el servidor va a
   * rechazar en la primera llamada.
   */
  private recuperarSesionGuardada(): Sesion | null {
    const guardado = localStorage.getItem(CLAVE_SESION);

    if (!guardado) {
      return null;
    }

    try {
      const sesion = JSON.parse(guardado) as Sesion;

      if (!sesion?.token || new Date(sesion.expiracion) <= new Date()) {
        localStorage.removeItem(CLAVE_SESION);
        return null;
      }

      return sesion;
    } catch {
      localStorage.removeItem(CLAVE_SESION);
      return null;
    }
  }
}
