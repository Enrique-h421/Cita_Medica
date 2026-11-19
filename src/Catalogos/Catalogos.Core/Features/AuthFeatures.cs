using Catalogos.Core.Dtos;
using Catalogos.Core.Exceptions;
using Catalogos.Core.Security;
using Catalogos.Core.Services;
using Catalogos.Domain.Entities;
using Catalogos.Domain.Seguridad;
using GenericPersistence.Repository;
using MediatR;

namespace Catalogos.Core.Features.Auth
{
    /// <summary>Inicio de sesión: valida credenciales y devuelve el token.</summary>
    public class LoginCommand : IRequest<SesionDto>
    {
        public LoginDto Credenciales { get; set; } = new();
    }

    public class LoginHandler : IRequestHandler<LoginCommand, SesionDto>
    {
        private readonly IGenericRepository<Usuario> _usuarios;
        private readonly IGenericRepository<Medico> _medicos;
        private readonly IAuthTokenService _tokens;

        public LoginHandler(
            IGenericRepository<Usuario> usuarios,
            IGenericRepository<Medico> medicos,
            IAuthTokenService tokens)
        {
            _usuarios = usuarios;
            _medicos = medicos;
            _tokens = tokens;
        }

        public async Task<SesionDto> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var nombreUsuario = request.Credenciales.NombreUsuario?.Trim() ?? string.Empty;

            // Se usa la versión paginada porque es la que admite includes: hace
            // falta el rol para armar el token sin una segunda consulta.
            var pagina = await _usuarios.GetPagedAsync(
                1, 1,
                u => u.NombreUsuario == nombreUsuario,
                null, "UsuarioID", true, false, cancellationToken,
                u => u.Rol!);

            var usuario = pagina.Data.FirstOrDefault();

            // El mismo mensaje para "no existe el usuario" y para "la contraseña
            // está mal". Si fueran distintos, cualquiera podría ir probando
            // nombres hasta averiguar qué cuentas existen en la clínica.
            //
            // Y se verifica la contraseña incluso cuando el usuario no existe,
            // contra un hash de relleno, para que los dos casos tarden lo mismo.
            // Si el caso "no existe" respondiera al instante, el tiempo de
            // respuesta delataría qué usuarios son reales.
            var hashAComparar = usuario?.PasswordHash ?? HashDeRelleno;
            var coincide = PasswordHasher.Verificar(request.Credenciales.Password, hashAComparar);

            if (usuario is null || !coincide)
            {
                throw new DomainException(
                    Errores.AUTENTICACION_FALLIDA,
                    "Usuario o contraseña incorrectos.");
            }

            if (!usuario.Activo)
            {
                throw new DomainException(
                    Errores.USUARIO_INACTIVO,
                    "La cuenta está desactivada. Contacte al administrador.");
            }

            // Si el usuario es médico, se busca su ficha. Esto es lo que después
            // permite que la pantalla diga "mis citas" en lugar de "todas".
            var medico = await _medicos.GetOneByAsync(m => m.UsuarioID == usuario.UsuarioID);

            var rolNormalizado = RolesSistema.Normalizar(usuario.Rol?.Nombre);

            var generado = _tokens.Generar(usuario, rolNormalizado, medico?.MedicoID);

            return new SesionDto
            {
                Token = generado.Token,
                Expiracion = generado.Expiracion,
                UsuarioId = usuario.UsuarioID,
                NombreUsuario = usuario.NombreUsuario,
                NombreCompleto = usuario.NombreCompleto,
                RolId = usuario.RolID,
                Rol = rolNormalizado,
                RolNombre = usuario.Rol?.Nombre,
                MedicoId = medico?.MedicoID
            };
        }

        /// <summary>
        /// Hash válido de una contraseña que nadie conoce. Solo existe para que
        /// el camino "usuario inexistente" haga el mismo trabajo de cómputo que
        /// el camino normal y tarde lo mismo.
        /// </summary>
        private const string HashDeRelleno =
            "PBKDF2$100000$AAAAAAAAAAAAAAAAAAAAAA==$AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";
    }
}
