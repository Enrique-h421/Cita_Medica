import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/**
 * La raíz de la aplicación. No hace más que dibujar la ruta activa: el marco
 * con la barra de arriba vive en Shell, porque la pantalla de login no lo
 * lleva.
 */
@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css',
})
export class App {}
