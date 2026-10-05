namespace Core.Configs
{
    /// <summary>URLs base de los otros microservicios a los que llama Pedidos.</summary>
    public class DownstreamOptions
    {
        public string PagosBaseUrl { get; set; } = string.Empty;
        public string InventarioBaseUrl { get; set; } = string.Empty;
        public string ReportesBaseUrl { get; set; } = string.Empty;
    }
}
