/**
 * Formato de fechas para El Salvador.
 *
 * Se hace con Intl y no con el DatePipe de Angular para no depender de los
 * paquetes de idioma: Intl viene en el propio navegador y siempre está.
 */

const FECHA_LARGA = new Intl.DateTimeFormat('es-SV', {
  weekday: 'long',
  day: 'numeric',
  month: 'long',
});

const FECHA_CORTA = new Intl.DateTimeFormat('es-SV', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
});

const HORA = new Intl.DateTimeFormat('es-SV', {
  hour: 'numeric',
  minute: '2-digit',
  hour12: true,
});

/** "lunes, 20 de noviembre" */
export function fechaLarga(valor: string | Date | null | undefined): string {
  const fecha = aFecha(valor);
  return fecha ? FECHA_LARGA.format(fecha) : '';
}

/** "20/11/2026" */
export function fechaCorta(valor: string | Date | null | undefined): string {
  const fecha = aFecha(valor);
  return fecha ? FECHA_CORTA.format(fecha) : '';
}

/** "9:00 a. m." */
export function hora(valor: string | Date | null | undefined): string {
  const fecha = aFecha(valor);
  return fecha ? HORA.format(fecha) : '';
}

/** "20/11/2026, 9:00 a. m." */
export function fechaYHora(valor: string | Date | null | undefined): string {
  const fecha = aFecha(valor);
  return fecha ? `${FECHA_CORTA.format(fecha)}, ${HORA.format(fecha)}` : '';
}

/** Para comparar días sin que la hora estorbe: "2026-11-20". */
export function claveDia(valor: string | Date | null | undefined): string {
  const fecha = aFecha(valor);

  if (!fecha) {
    return '';
  }

  const mes = String(fecha.getMonth() + 1).padStart(2, '0');
  const dia = String(fecha.getDate()).padStart(2, '0');

  return `${fecha.getFullYear()}-${mes}-${dia}`;
}

function aFecha(valor: string | Date | null | undefined): Date | null {
  if (!valor) {
    return null;
  }

  const fecha = valor instanceof Date ? valor : new Date(valor);

  // Una fecha inválida da NaN al pedirle el tiempo. Mejor devolver cadena
  // vacía que pintar "Invalid Date" en medio de la agenda.
  return Number.isNaN(fecha.getTime()) ? null : fecha;
}
