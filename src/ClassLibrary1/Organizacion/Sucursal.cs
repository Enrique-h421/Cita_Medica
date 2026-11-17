namespace Domain.Organizacion
{
    /// <summary>
    /// Sede física de la clínica. No es un módulo del alcance elegido: se mapea
    /// porque Consultorios.SucursalID es obligatorio en el esquema entregado y
    /// sin ella no se puede registrar un consultorio.
    /// </summary>
    public class Sucursal
    {
        public int SucursalID { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Direccion { get; set; }

        public string? Telefono { get; set; }

        public string? Email { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaCreacion { get; set; }

        public ICollection<Area> Areas { get; set; } = new List<Area>();
    }
}
