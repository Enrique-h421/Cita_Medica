import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { autenticacionInterceptor } from './core/auth/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),

    // El interceptor va registrado aquí, una sola vez, y a partir de este
    // momento toda llamada HTTP de la aplicación sale con el token puesto.
    provideHttpClient(withInterceptors([autenticacionInterceptor])),
  ],
};
