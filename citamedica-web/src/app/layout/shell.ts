import { Component, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { Auth } from '../core/auth/auth';

/**
 * El marco de las pantallas con sesión iniciada: la barra de arriba con quién
 * sos y el menú, y debajo la pantalla que toque.
 *
 * El menú se dibuja según el rol. Eso es comodidad, no seguridad — esconder
 * un botón no protege nada. Lo que de verdad protege es que el servidor
 * responda 403, cosa que hace aunque alguien fuerce la petición a mano.
 */
@Component({
  selector: 'app-shell',
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.html',
  styleUrl: './shell.css',
})
export class Shell {
  protected readonly auth = inject(Auth);
}
