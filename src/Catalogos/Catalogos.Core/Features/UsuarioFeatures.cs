using Catalogos.Core.Dtos;
using Catalogos.Core.Exceptions;
using Catalogos.Core.Mappings;
using Catalogos.Domain.Entities;
using GenericPersistence.Repository;
using MediatR;

namespace Catalogos.Core.Features.Usuarios
{
    /// <summary>
    /// Consulta de usuarios y roles. Solo lectura: dar de alta usuarios por API
    /// exige conocer los tipos exactos de las columnas y manejar el hash de la
    /// contraseña del lado del servidor, y eso se hace después, con el esquema
    /// a la vista. Por ahora los usuarios se siembran por SQL.
    /// </summary>
    public class ObtenerUsuariosQuery : IRequest<List<UsuarioDto>> { }

    public class ObtenerUsuariosHandler : IRequestHandler<ObtenerUsuariosQuery, List<UsuarioDto>>
    {
        private const int TopeListado = 500;

        private readonly IGenericRepository<Usuario> _usuarios;
        private readonly IGenericRepository<Medico> _medicos;

        public ObtenerUsuariosHandler(
            IGenericRepository<Usuario> usuarios,
            IGenericRepository<Medico> medicos)
        {
            _usuarios = usuarios;
            _medicos = medicos;
        }

        public async Task<List<UsuarioDto>> Handle(
            ObtenerUsuariosQuery request, CancellationToken cancellationToken)
        {
            var pagina = await _usuarios.GetPagedAsync(
                1, TopeListado, null, null, "NombreUsuario", true, false, cancellationToken,
                u => u.Rol!);

            var dtos = pagina.Data.ToDtoList();

            await AmarrarFichasDeMedicoAsync(dtos);

            return dtos;
        }

        /// <summary>
        /// Le pone a cada usuario su MedicoID si tiene ficha. Se traen todos los
        /// médicos de una sola vez y el cruce se hace en memoria: una consulta
        /// en total, no una por usuario.
        /// </summary>
        private async Task AmarrarFichasDeMedicoAsync(List<UsuarioDto> usuarios)
        {
            if (usuarios.Count == 0) return;

            var medicos = (await _medicos.ObtenerTodosAsync()).ToList();

            var porUsuario = medicos
                .Where(m => m.UsuarioID.HasValue)
                .GroupBy(m => m.UsuarioID!.Value)
                .ToDictionary(g => g.Key, g => g.First().MedicoID);

            foreach (var usuario in usuarios)
            {
                if (porUsuario.TryGetValue(usuario.UsuarioId, out var medicoId))
                {
                    usuario.MedicoId = medicoId;
                }
            }
        }
    }

    public class ObtenerUsuarioPorIdQuery : IRequest<UsuarioDto>
    {
        public int UsuarioId { get; set; }
    }

    public class ObtenerUsuarioPorIdHandler : IRequestHandler<ObtenerUsuarioPorIdQuery, UsuarioDto>
    {
        private readonly IGenericRepository<Usuario> _usuarios;
        private readonly IGenericRepository<Medico> _medicos;

        public ObtenerUsuarioPorIdHandler(
            IGenericRepository<Usuario> usuarios,
            IGenericRepository<Medico> medicos)
        {
            _usuarios = usuarios;
            _medicos = medicos;
        }

        public async Task<UsuarioDto> Handle(
            ObtenerUsuarioPorIdQuery request, CancellationToken cancellationToken)
        {
            var usuarioId = request.UsuarioId;

            var pagina = await _usuarios.GetPagedAsync(
                1, 1, u => u.UsuarioID == usuarioId, null, "UsuarioID", true, false, cancellationToken,
                u => u.Rol!);

            var usuario = pagina.Data.FirstOrDefault();

            if (usuario is null)
            {
                throw new DomainException(
                    Errores.USUARIO_NO_ENCONTRADO,
                    $"No existe un usuario con el Id {usuarioId}.");
            }

            var dto = usuario.ToDto();

            var medico = await _medicos.GetOneByAsync(m => m.UsuarioID == usuarioId);
            dto.MedicoId = medico?.MedicoID;

            return dto;
        }
    }

    public class ObtenerRolesQuery : IRequest<List<RolDto>> { }

    public class ObtenerRolesHandler : IRequestHandler<ObtenerRolesQuery, List<RolDto>>
    {
        private readonly IGenericRepository<Rol> _roles;

        public ObtenerRolesHandler(IGenericRepository<Rol> roles) => _roles = roles;

        public async Task<List<RolDto>> Handle(
            ObtenerRolesQuery request, CancellationToken cancellationToken)
        {
            // splitQuery en true: con JOIN, la fila del rol se repetiría una vez
            // por cada usuario que lo tiene.
            var pagina = await _roles.GetPagedAsync(
                1, 100, null, null, "RolID", true, true, cancellationToken,
                r => r.Usuarios);

            return pagina.Data.ToDtoList();
        }
    }
}
