namespace Domain.Diagnosticos
{
    /// <summary>
    /// Una entrada del catálogo de diagnósticos, codificada con CIE-10 (la
    /// Clasificación Internacional de Enfermedades de la OMS). No es un
    /// diagnóstico de un paciente: es la etiqueta que el médico escoge.
    ///
    /// El catálogo lo mantiene la clínica y se siembra por SQL
    /// (sql/05_Diagnosticos.sql). Nunca se borra una entrada: se marca
    /// Activo = false, porque las atenciones antiguas siguen apuntando a ella
    /// y el expediente tiene que poder mostrar qué decía.
    /// </summary>
    public class Diagnostico
    {
        public int DiagnosticoID { get; set; }

        /// <summary>
        /// Código CIE-10, por ejemplo "J00" o "E11.9". Es opcional en la base
        /// porque una clínica puede registrar un diagnóstico propio que todavía
        /// no tiene código asignado.
        /// </summary>
        public string? CodigoCIE10 { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        /// <summary>Si está en false ya no se ofrece al buscar, pero sigue
        /// resolviendo el nombre en los expedientes que lo usaron.</summary>
        public bool Activo { get; set; } = true;
    }
}
