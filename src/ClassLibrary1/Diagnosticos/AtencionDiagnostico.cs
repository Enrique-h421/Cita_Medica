using Domain.AgendaYAtencion;

namespace Domain.Diagnosticos
{
    /// <summary>
    /// El diagnóstico que el médico le puso a una consulta concreta. Es la
    /// tabla puente entre la atención y el catálogo: una misma consulta puede
    /// cerrar con varios diagnósticos, y un mismo código aparece en miles de
    /// consultas distintas.
    ///
    /// Tiene llave propia (AtencionDiagnosticoID) en lugar de llave compuesta.
    /// Eso es a propósito: así se puede quitar un diagnóstico mal puesto con un
    /// solo Id, igual que cualquier otro registro del sistema.
    /// </summary>
    public class AtencionDiagnostico
    {
        public long AtencionDiagnosticoID { get; set; }

        public long AtencionID { get; set; }

        public int DiagnosticoID { get; set; }

        /// <summary>PRINCIPAL o SECUNDARIO. Ver <see cref="TiposDiagnostico"/>.</summary>
        public string TipoDiagnostico { get; set; } = TiposDiagnostico.Principal;

        /// <summary>Matiz del médico sobre este diagnóstico en esta consulta;
        /// por ejemplo "cuadro leve, sin fiebre".</summary>
        public string? Observaciones { get; set; }

        public Atencion? Atencion { get; set; }
        public Diagnostico? Diagnostico { get; set; }
    }

    /// <summary>
    /// Los dos papeles que puede tener un diagnóstico dentro de una consulta.
    /// La columna es varchar(30) libre, así que las constantes están aquí para
    /// que nadie escriba el texto a mano y se equivoque de mayúscula.
    ///
    /// En codificación clínica el principal es el que motivó la consulta y solo
    /// puede haber uno; los demás son secundarios (comorbilidades, hallazgos).
    /// </summary>
    public static class TiposDiagnostico
    {
        /// <summary>El que explica la consulta. Solo uno por atención.</summary>
        public const string Principal = "PRINCIPAL";

        /// <summary>Lo demás que el médico encontró o que el paciente ya traía.</summary>
        public const string Secundario = "SECUNDARIO";

        /// <summary>
        /// Deja el texto en la forma canónica para poder compararlo. Acepta lo
        /// que venga del cliente en minúsculas o con espacios de sobra.
        /// Devuelve null si no es ninguno de los dos tipos válidos.
        /// </summary>
        public static string? Normalizar(string? tipo)
        {
            if (string.IsNullOrWhiteSpace(tipo))
            {
                return null;
            }

            var limpio = tipo.Trim().ToUpperInvariant();

            return limpio switch
            {
                Principal => Principal,
                Secundario => Secundario,
                _ => null
            };
        }
    }
}
