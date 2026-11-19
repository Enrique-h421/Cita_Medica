namespace Catalogos.Domain.Entities
{
    /// <summary>
    /// Usuario que entra al sistema. La contraseña nunca se guarda: lo que vive
    /// en PasswordHash es un derivado PBKDF2-HMAC-SHA256 con sal propia, en el
    /// formato PBKDF2$iteraciones$sal_base64$hash_base64. De ahí no se puede
    /// volver a la contraseña; solo se puede verificar si una coincide.
    ///
    /// Un usuario con rol Médico se amarra a su ficha en la tabla Medicos
    /// (Medicos.UsuarioID). Esa es la pieza que permite que el sistema sepa
    /// "quién soy yo como médico" cuando alguien inicia sesión.
    /// </summary>
    public class Usuario
    {
        public int UsuarioID { get; set; }

        public int RolID { get; set; }

        public int? SucursalID { get; set; }

        public string NombreUsuario { get; set; } = string.Empty;

        public string? NombreCompleto { get; set; }

        public string? Email { get; set; }

        /// <summary>Hash PBKDF2. Nunca sale de la capa de dominio hacia un DTO.</summary>
        public string PasswordHash { get; set; } = string.Empty;

        public bool Activo { get; set; }

        public DateTime FechaCreacion { get; set; }

        public Rol? Rol { get; set; }
    }
}
