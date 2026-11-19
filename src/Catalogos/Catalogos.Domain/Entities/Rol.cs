namespace Catalogos.Domain.Entities
{
    /// <summary>
    /// Rol del sistema: Administrador, Médico o Recepción. La tabla no tiene
    /// columna de código, solo el nombre, así que para comparar roles en el
    /// código se usa el nombre normalizado. Ver <see cref="Seguridad.RolesSistema"/>.
    /// </summary>
    public class Rol
    {
        public int RolID { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
