/**
 * Direcciones de los dos microservicios.
 *
 * Están separadas a propósito: el frontend habla con dos servicios distintos,
 * no con uno solo. Catálogos emite el token y guarda pacientes, médicos y
 * especialidades; Agenda guarda citas, consultorios, atenciones y expedientes.
 *
 * Se usan los puertos HTTP y no los HTTPS para evitar que el navegador se
 * ponga quisquilloso con el certificado de desarrollo. Los dos servicios
 * escuchan en ambos.
 */
export const API = {
  catalogos: 'http://localhost:5110',
  agenda: 'http://localhost:5104',
} as const;
