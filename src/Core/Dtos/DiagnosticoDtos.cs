using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Core.Dtos
{
    /// <summary>
    /// Una entrada del catálogo CIE-10, como se la ofrece al médico en el
    /// buscador de diagnósticos.
    /// </summary>
    public class DiagnosticoDto
    {
        public int DiagnosticoId { get; set; }

        /// <summary>Código CIE-10, por ejemplo "J00". Puede venir vacío si la
        /// clínica registró un diagnóstico propio sin código.</summary>
        public string? CodigoCIE10 { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }
    }

    /// <summary>
    /// Un diagnóstico ya puesto en una consulta. Trae el código y el nombre
    /// resueltos para que la pantalla no tenga que ir a buscarlos al catálogo.
    /// </summary>
    public class AtencionDiagnosticoDto
    {
        /// <summary>Id del registro puente. Es el que se usa para quitarlo.</summary>
        public long AtencionDiagnosticoId { get; set; }

        public long AtencionId { get; set; }

        public int DiagnosticoId { get; set; }

        public string? CodigoCIE10 { get; set; }

        public string? Nombre { get; set; }

        /// <summary>PRINCIPAL o SECUNDARIO.</summary>
        public string TipoDiagnostico { get; set; } = string.Empty;

        public string? Observaciones { get; set; }
    }

    /// <summary>
    /// Lo que manda la pantalla de atención para agregarle un diagnóstico a la
    /// consulta que el médico tiene abierta.
    /// </summary>
    public class AgregarDiagnosticoDto
    {
        [DefaultValue(1)]
        [Range(1, int.MaxValue, ErrorMessage = "Debe indicar el diagnóstico del catálogo.")]
        public int DiagnosticoId { get; set; }

        /// <summary>
        /// PRINCIPAL o SECUNDARIO. Si no se manda, se asume PRINCIPAL cuando la
        /// consulta todavía no tiene uno, y SECUNDARIO cuando ya lo tiene: así
        /// la pantalla puede dejar que el médico solo escoja el diagnóstico.
        /// </summary>
        [DefaultValue(null)]
        public string? TipoDiagnostico { get; set; }

        [StringLength(1000, ErrorMessage = "Las observaciones no pueden pasar de 1000 caracteres.")]
        public string? Observaciones { get; set; }
    }
}
