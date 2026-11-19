import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { Auth } from './auth';

/**
 * Deja pasar solo a quien inició sesión.
 *
 * Esto es comodidad, no seguridad. Un guardián de rutas solo decide qué
 * pantalla se dibuja, y cualquiera con las herramientas del navegador puede
 * saltárselo. La seguridad de verdad está en el servidor, que responde 401 o
 * 403 sin importar lo que haga el frontend. Aquí lo que se evita es que el
 * usuario llegue a una pantalla que se le va a llenar de errores.
 */
export const requiereSesion: CanActivateFn = () => {
  const auth = inject(Auth);
  const router = inject(Router);

  if (auth.autenticado()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};

/**
 * Deja pasar solo a los roles indicados.
 *
 * Se usa así en las rutas:  canActivate: [requiereSesion, requiereRol('Medico')]
 */
export function requiereRol(...roles: string[]): CanActivateFn {
  return () => {
    const auth = inject(Auth);
    const router = inject(Router);

    if (auth.tieneAlgunRol(...roles)) {
      return true;
    }

    return router.createUrlTree(['/agenda']);
  };
}

/** Evita que alguien con sesión abierta vuelva a la pantalla de login. */
export const soloSinSesion: CanActivateFn = () => {
  const auth = inject(Auth);
  const router = inject(Router);

  return auth.autenticado() ? router.createUrlTree(['/agenda']) : true;
};
