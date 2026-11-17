namespace Domain.Organizacion
{
    /// <summary>
    /// División dentro de una sucursal (Consulta Externa, Emergencia...).
    /// Igual que Sucursal, se mapea por ser dependencia de Consultorios.
    /// </summary>
    public class Area
    {
        public int AreaID { get; set; }

        public int SucursalID { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public bool Activo { get; set; } = true;

        public Sucursal? Sucursal { get; set; }
    }
}
