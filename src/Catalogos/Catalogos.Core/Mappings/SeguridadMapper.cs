using Catalogos.Core.Dtos;
using Catalogos.Domain.Entities;

namespace Catalogos.Core.Mappings
{
    public static class SeguridadMapper
    {
        /// <summary>
        /// Convierte el usuario a DTO. Fijate que PasswordHash no se copia a
        /// ningún lado: la única forma de que un hash salga por la API sería
        /// agregarlo aquí a mano, y eso no va a pasar.
        /// </summary>
        public static UsuarioDto ToDto(this Usuario u) => new UsuarioDto
        {
            UsuarioId = u.UsuarioID,
            NombreUsuario = u.NombreUsuario,
            NombreCompleto = u.NombreCompleto,
            Email = u.Email,
            RolId = u.RolID,
            RolNombre = u.Rol?.Nombre,
            Activo = u.Activo,
            FechaCreacion = u.FechaCreacion
        };

        public static List<UsuarioDto> ToDtoList(this IEnumerable<Usuario> items) =>
            items.Select(x => x.ToDto()).ToList();

        public static RolDto ToDto(this Rol r) => new RolDto
        {
            RolId = r.RolID,
            Nombre = r.Nombre,
            Descripcion = r.Descripcion,
            Activo = r.Activo,
            TotalUsuarios = r.Usuarios?.Count ?? 0
        };

        public static List<RolDto> ToDtoList(this IEnumerable<Rol> items) =>
            items.Select(x => x.ToDto()).ToList();
    }
}
