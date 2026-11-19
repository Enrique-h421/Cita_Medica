using System.ComponentModel.DataAnnotations;

namespace Catalogos.Core.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El usuario es obligatorio.")]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lo que recibe el frontend al iniciar sesión. Trae el token y, de una vez,
    /// quién es el usuario: así la pantalla puede dibujar el menú según el rol
    /// sin tener que decodificar el token ni hacer otra llamada.
    ///
    /// Acá no va el hash de la contraseña. Nunca.
    /// </summary>
    public class SesionDto
    {
        public string Token { get; set; } = string.Empty;

        public DateTime Expiracion { get; set; }

        public int UsuarioId { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string? NombreCompleto { get; set; }

        public int RolId { get; set; }

        /// <summary>Rol normalizado, sin acentos. Es el que viaja en el token.</summary>
        public string Rol { get; set; } = string.Empty;

        /// <summary>Rol como está en la base, con acentos. Para mostrar en pantalla.</summary>
        public string? RolNombre { get; set; }

        /// <summary>Ficha de médico del usuario, si es médico. Null si no lo es.</summary>
        public int? MedicoId { get; set; }
    }

    public class UsuarioDto
    {
        public int UsuarioId { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string? NombreCompleto { get; set; }

        public string? Email { get; set; }

        public int RolId { get; set; }

        public string? RolNombre { get; set; }

        public int? MedicoId { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }
    }

    public class RolDto
    {
        public int RolId { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public bool Activo { get; set; }

        public int TotalUsuarios { get; set; }
    }
}
