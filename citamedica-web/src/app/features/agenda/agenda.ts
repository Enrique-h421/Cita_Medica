import { Component, inject } from '@angular/core';

import { Auth } from '../../core/auth/auth';

/**
 * Agenda de citas. Por ahora solo confirma que la sesión llegó entera hasta
 * aquí; el listado de citas se construye en el siguiente paso.
 */
@Component({
  selector: 'app-agenda',
  template: `
    <h1>Agenda</h1>

    <div class="caja">
      <p>Sesión iniciada como <strong>{{ auth.nombreParaMostrar() }}</strong>.</p>
      <p>Rol: <strong>{{ auth.rolParaMostrar() }}</strong></p>

      @if (auth.medicoId(); as medicoId) {
        <p>Ficha de médico: <strong>#{{ medicoId }}</strong></p>
      }

      <p class="nota">
        @if (auth.puedeAtender()) {
          Este rol puede abrir y cerrar atenciones.
        } @else {
          Este rol puede agendar, pero no escribir en el expediente clínico.
        }
      </p>
    </div>
  `,
  styles: `
    h1 {
      margin-bottom: 18px;
      font-size: 22px;
    }

    .caja {
      padding: 20px 24px;
      background: var(--superficie);
      border: 1px solid var(--borde);
      border-radius: var(--radio);
      box-shadow: var(--sombra);
    }

    p {
      margin: 0 0 8px;
    }

    .nota {
      margin-top: 16px;
      padding-top: 16px;
      border-top: 1px solid var(--borde);
      color: var(--texto-suave);
      font-size: 14px;
    }
  `,
})
export class Agenda {
  protected readonly auth = inject(Auth);
}
