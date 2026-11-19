using Catalogos.Domain.Entities;

namespace Catalogos.Core.Services
{
    /// <summary>
    /// Emite el token de sesión. Core define el contrato; la implementación vive
    /// en la capa de API porque es la que conoce la librería de JWT. Core no
    /// sabe que el token es un JWT: para Core es "una credencial firmada".
    /// </summary>
    public interface IAuthTokenService
    {
        /// <param name="usuario">Usuario autenticado, con su rol cargado.</param>
        /// <param name="rol">Nombre del rol ya normalizado, sin acentos.</param>
        /// <param name="medicoId">
        /// Ficha de médico del usuario, si tiene. Va dentro del token para que
        /// el frontend pueda mostrar "mis citas" sin preguntar otra vez.
        /// </param>
        TokenGenerado Generar(Usuario usuario, string rol, int? medicoId);
    }

    public class TokenGenerado
    {
        public string Token { get; set; } = string.Empty;
        public DateTime Expiracion { get; set; }
    }
}
