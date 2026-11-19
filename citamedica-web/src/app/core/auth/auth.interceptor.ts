import { inject } from '@angular/core';
import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { catchError, throwError } from 'rxjs';

import { Auth } from './auth';

/**
 * Le pega el token a todas las llamadas salientes.
 *
 * Está centralizado a propósito: si cada servicio tuviera que acordarse de
 * poner la cabecera, tarde o temprano uno se olvida y aparece un 401 que
 * nadie entiende. Aquí se pone una vez y vale para toda la aplicación.
 *
 * La excepción es el propio login: si hiciera falta un token para pedir un
 * token, nadie podría entrar nunca.
 */
export const autenticacionInterceptor: HttpInterceptorFn = (peticion, siguiente) => {
  const auth = inject(Auth);
  const token = auth.token();

  const esLogin = peticion.url.includes('/api/Auth/login');

  const conCredencial =
    token && !esLogin
      ? peticion.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : peticion;

  return siguiente(conCredencial).pipe(
    catchError((error: HttpErrorResponse) => {
      // 401 significa que el token ya no sirve: caducó o la firma no cuadra.
      // No tiene sentido dejar al usuario dando vueltas con una sesión muerta.
      if (error.status === 401 && !esLogin) {
        auth.cerrarSesion();
      }

      return throwError(() => error);
    }),
  );
};
