import { Routes } from '@angular/router';

import { requiereSesion, soloSinSesion } from './core/auth/auth.guard';

/**
 * Las rutas se cargan con loadComponent, o sea bajo demanda: el navegador
 * solo descarga el código de una pantalla cuando el usuario entra a ella.
 * En una aplicación chica se nota poco, pero es la forma correcta y evita
 * tener que reorganizarlo todo cuando crece.
 */
export const routes: Routes = [
  {
    path: 'login',
    canActivate: [soloSinSesion],
    loadComponent: () => import('./features/login/login').then((m) => m.Login),
  },
  {
    // Todo lo que cuelga de aquí vive dentro del marco con la barra de arriba
    // y exige sesión iniciada.
    path: '',
    canActivate: [requiereSesion],
    loadComponent: () => import('./layout/shell').then((m) => m.Shell),
    children: [
      {
        path: 'agenda',
        loadComponent: () => import('./features/agenda/agenda').then((m) => m.Agenda),
      },
      { path: '', pathMatch: 'full', redirectTo: 'agenda' },
    ],
  },
  { path: '**', redirectTo: '' },
];
