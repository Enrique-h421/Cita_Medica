using Newtonsoft.Json;

namespace Domain.Models
{
    /// <summary>
    /// Lo que Agenda necesita saber de un paciente que vive en el microservicio
    /// de Catálogos. Es un resumen a propósito: Agenda no es dueña de esos datos
    /// y no debe arrastrar el expediente completo.
    /// </summary>
    public class PacienteResumen
    {
        [JsonProperty("pacienteId")]
        public long PacienteId { get; set; }

        [JsonProperty("nombreCompleto")]
        public string? NombreCompleto { get; set; }

        [JsonProperty("activo")]
        public bool Activo { get; set; }
    }

    public class MedicoResumen
    {
        [JsonProperty("medicoId")]
        public int MedicoId { get; set; }

        [JsonProperty("nombreCompleto")]
        public string? NombreCompleto { get; set; }

        [JsonProperty("especialidadId")]
        public int? EspecialidadId { get; set; }

        [JsonProperty("especialidadNombre")]
        public string? EspecialidadNombre { get; set; }

        [JsonProperty("activo")]
        public bool Activo { get; set; }
    }

    public class EspecialidadResumen
    {
        [JsonProperty("especialidadId")]
        public int EspecialidadId { get; set; }

        [JsonProperty("nombre")]
        public string? Nombre { get; set; }

        [JsonProperty("activo")]
        public bool Activo { get; set; }
    }
}
