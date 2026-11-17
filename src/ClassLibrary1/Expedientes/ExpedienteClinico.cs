using Domain.AgendaYAtencion;

namespace Domain.Expedientes
{
    /// <summary>
    /// Expediente clínico del paciente: la carpeta donde se van guardando todas
    /// sus atenciones. Se abre una sola vez por paciente, en su primera consulta,
    /// y a partir de ahí todas las atenciones cuelgan de ella.
    ///
    /// Vive en esta base (ClinicaDB) junto con las atenciones, así que sí tiene
    /// navegación. El paciente en cambio vive en el microservicio de Catálogos:
    /// aquí solo se guarda su Id.
    /// </summary>
    public class ExpedienteClinico
    {
        public long ExpedienteID { get; set; }

        /// <summary>Id del paciente en el microservicio de Catálogos.</summary>
        public long PacienteID { get; set; }

        /// <summary>Número correlativo visible, del estilo EXP-000001.</summary>
        public string NumeroExpediente { get; set; } = string.Empty;

        public DateTime FechaApertura { get; set; }

        public string? Observaciones { get; set; }

        public bool Activo { get; set; } = true;

        public ICollection<Atencion> Atenciones { get; set; } = new List<Atencion>();
    }
}
