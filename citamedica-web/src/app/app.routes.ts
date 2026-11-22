import { Routes } from '@angular/router';

import { requiereRol, requiereSesion, soloSinSesion } from './core/auth/auth.guard';
import { ROLES } from './core/auth/modelos';

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
        // La pantalla de bienvenida: en qué va el día. Es la primera que se
        // ve al entrar, así que va de primera también aquí.
        path: 'inicio',
        loadComponent: () => import('./features/inicio/inicio').then((m) => m.Inicio),
      },
      {
        path: 'agenda',
        loadComponent: () => import('./features/agenda/agenda').then((m) => m.Agenda),
      },
      {
        // Todo el mundo necesita consultar pacientes: recepción para agendar,
        // el médico para saber a quién atiende.
        path: 'pacientes',
        loadComponent: () =>
          import('./features/pacientes/pacientes').then((m) => m.PacientesLista),
      },
      {
        // Darlos de alta es trabajo de ventanilla.
        path: 'pacientes/nuevo',
        canActivate: [requiereRol(ROLES.administrador, ROLES.recepcion)],
        loadComponent: () =>
          import('./features/pacientes/nuevo-paciente').then((m) => m.NuevoPaciente),
      },
      {
        // Agendar es trabajo de recepción y del administrador. Un médico no
        // se pone citas a sí mismo.
        path: 'citas/nueva',
        canActivate: [requiereRol(ROLES.administrador, ROLES.recepcion)],
        loadComponent: () =>
          import('./features/citas/nueva-cita').then((m) => m.NuevaCita),
      },
      {
        // Escribir en el expediente clínico es acto médico. El guardián evita
        // llegar a una pantalla que el servidor va a rechazar igual con 403.
        path: 'atender/:citaId',
        canActivate: [requiereRol(ROLES.administrador, ROLES.medico)],
        loadComponent: () => import('./features/atender/atender').then((m) => m.Atender),
      },
      {
        // El contenido clínico tampoco es para recepción: ella agenda, no lee
        // diagnósticos.
        path: 'expedientes',
        canActivate: [requiereRol(ROLES.administrador, ROLES.medico)],
        loadComponent: () =>
          import('./features/expedientes/expedientes').then((m) => m.ExpedientesLista),
      },
      {
        path: 'expedientes/:pacienteId',
        canActivate: [requiereRol(ROLES.administrador, ROLES.medico)],
        loadComponent: () =>
          import('./features/expedientes/historial').then((m) => m.Historial),
      },
      {
        // Quién tiene acceso a la clínica no es información para recepción ni
        // para los médicos.
        path: 'usuarios',
        canActivate: [requiereRol(ROLES.administrador)],
        loadComponent: () =>
          import('./features/usuarios/usuarios').then((m) => m.UsuariosLista),
      },
      { path: '', pathMatch: 'full', redirectTo: 'inicio' },
    ],
  },
  { path: '**', redirectTo: '' },
];
