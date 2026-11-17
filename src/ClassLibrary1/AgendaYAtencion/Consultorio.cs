using Domain.Organizacion;

namespace Domain.AgendaYAtencion
{
    /// <summary>
    /// Espacio físico donde se atiende una cita. Mapea la tabla Consultorios tal
    /// como viene en el esquema entregado: Piso es texto (admite "PB", "Mezanine"),
    /// Codigo es obligatorio y la sucursal también.
    /// </summary>
    public class Consultorio
    {
        public int ConsultorioID { get; set; }

        public int SucursalID { get; set; }

        public int? AreaID { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string? Piso { get; set; }

        public bool Activo { get; set; } = true;

        public Sucursal? Sucursal { get; set; }

        public Area? Area { get; set; }

        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}
