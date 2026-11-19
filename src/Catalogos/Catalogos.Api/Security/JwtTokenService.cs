using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Catalogos.Core.Configs;
using Catalogos.Core.Services;
using Catalogos.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Catalogos.Api.Security
{
    /// <summary>
    /// Arma el JWT. Está en la capa de API y no en Core porque es lo único que
    /// depende de la librería de tokens; Core solo conoce la interfaz.
    ///
    /// El token va firmado, no cifrado: cualquiera puede leer lo que lleva
    /// dentro pegándolo en jwt.io. Por eso adentro solo van identificadores y el
    /// rol — nada de contraseñas ni datos clínicos. Lo que la firma garantiza no
    /// es el secreto, es que nadie pueda cambiar su propio rol a Administrador
    /// sin conocer la llave.
    /// </summary>
    public class JwtTokenService : IAuthTokenService
    {
        private readonly JwtOptions _opciones;

        public JwtTokenService(IOptions<JwtOptions> opciones)
        {
            _opciones = opciones.Value;
        }

        public TokenGenerado Generar(Usuario usuario, string rol, int? medicoId)
        {
            var ahora = DateTime.UtcNow;
            var expiracion = ahora.AddMinutes(_opciones.MinutosExpiracion);

            var claims = new List<Claim>
            {
                new("sub", usuario.UsuarioID.ToString()),
                new("nombreUsuario", usuario.NombreUsuario),
                new("role", rol),
                new("rolId", usuario.RolID.ToString()),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            if (!string.IsNullOrWhiteSpace(usuario.NombreCompleto))
            {
                claims.Add(new Claim("nombreCompleto", usuario.NombreCompleto));
            }

            if (medicoId.HasValue)
            {
                claims.Add(new Claim("medicoId", medicoId.Value.ToString()));
            }

            if (usuario.SucursalID.HasValue)
            {
                claims.Add(new Claim("sucursalId", usuario.SucursalID.Value.ToString()));
            }

            var credenciales = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opciones.Key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _opciones.Issuer,
                audience: _opciones.Audience,
                claims: claims,
                notBefore: ahora,
                expires: expiracion,
                signingCredentials: credenciales);

            return new TokenGenerado
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiracion = expiracion
            };
        }
    }
}
