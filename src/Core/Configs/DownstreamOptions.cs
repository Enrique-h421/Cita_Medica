namespace Core.Configs
{
    /// <summary>URLs base de los otros microservicios a los que llama Agenda.</summary>
    public class DownstreamOptions
    {
        /// <summary>Microservicio de Catálogos y Seguridad: pacientes, médicos y especialidades.</summary>
        public string CatalogosBaseUrl { get; set; } = string.Empty;

        public string PagosBaseUrl { get; set; } = string.Empty;
        public string InventarioBaseUrl { get; set; } = string.Empty;
        public string ReportesBaseUrl { get; set; } = string.Empty;
    }
}
