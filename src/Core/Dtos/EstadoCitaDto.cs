namespace Core.Dtos
{
    /// <summary>
    /// Un estado del catálogo Estados_Cita, listo para pintar en un combo.
    ///
    /// El código es lo estable: el Id depende del orden en que se sembraron
    /// los estados, así que la lógica de negocio compara por código y no por
    /// número. Ver Domain/AgendaYAtencion/CodigosEstadoCita.cs.
    /// </summary>
    public class EstadoCitaDto
    {
        public int EstadoCitaId { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;
    }
}
