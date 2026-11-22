import { Component, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router } from '@angular/router';
import { form, FormField, required } from '@angular/forms/signals';

import { Auth } from '../../core/auth/auth';

@Component({
  selector: 'app-login',
  imports: [FormField],
  templateUrl: './login.html',
  styleUrl: './login.css',
})
export class Login {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);

  protected readonly modelo = signal({ nombreUsuario: '', password: '' });

  protected readonly formulario = form(this.modelo, (campo) => {
    required(campo.nombreUsuario);
    required(campo.password);
  });

  protected readonly enviando = signal(false);
  protected readonly error = signal<string | null>(null);

  /**
   * Si la contraseña se ve en claro. Apagado por defecto: en una recepción
   * siempre hay alguien mirando la pantalla.
   */
  protected readonly mostrarClave = signal(false);

  protected readonly puedeEntrar = computed(() => {
    const { nombreUsuario, password } = this.modelo();
    return nombreUsuario.trim().length > 0 && password.length > 0 && !this.enviando();
  });

  protected alternarClave(): void {
    this.mostrarClave.update((visible) => !visible);
  }

  protected async entrar(): Promise<void> {
    if (!this.puedeEntrar()) {
      return;
    }

    this.error.set(null);
    this.enviando.set(true);

    try {
      await this.auth.iniciarSesion(this.modelo());
      await this.router.navigateByUrl('/inicio');
    } catch (fallo) {
      this.error.set(this.explicar(fallo));
    } finally {
      this.enviando.set(false);
    }
  }

  /**
   * Traduce el fallo a algo que una recepcionista pueda entender.
   *
   * El caso 0 es el que más va a aparecer mientras desarrollamos: significa
   * que el navegador ni siquiera llegó al servidor, casi siempre porque los
   * microservicios no están levantados.
   */
  private explicar(fallo: unknown): string {
    if (fallo instanceof HttpErrorResponse) {
      switch (fallo.status) {
        case 0:
          return 'No se pudo contactar al servidor. Revise que el sistema esté encendido.';
        case 401:
          return 'Usuario o contraseña incorrectos.';
        case 403:
          return fallo.error?.errorMessage ?? 'La cuenta está desactivada.';
        default:
          return fallo.error?.errorMessage ?? 'No se pudo iniciar sesión. Intente de nuevo.';
      }
    }

    return 'Ocurrió un error inesperado.';
  }
}
