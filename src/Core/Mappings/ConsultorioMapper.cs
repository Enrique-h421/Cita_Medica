using Core.Dtos;
using Domain.AgendaYAtencion;

namespace Core.Mappings
{
    public static class ConsultorioMapper
    {
        public static ConsultorioDto ToDto(this Consultorio c) => new ConsultorioDto
        {
            ConsultorioId = c.ConsultorioID,
            Codigo = c.Codigo,
            Nombre = c.Nombre,
            Piso = c.Piso,
            SucursalId = c.SucursalID,
            SucursalNombre = c.Sucursal?.Nombre,
            AreaId = c.AreaID,
            AreaNombre = c.Area?.Nombre,
            Activo = c.Activo,
            Descripcion = Describir(c)
        };

        public static List<ConsultorioDto> ToDtoList(this IEnumerable<Consultorio> items) =>
            items.Select(x => x.ToDto()).ToList();

        public static Consultorio ToEntity(this CrearConsultorioDto dto) => new Consultorio
        {
            Codigo = dto.Codigo,
            Nombre = dto.Nombre,
            Piso = dto.Piso,
            SucursalID = dto.SucursalId,
            AreaID = dto.AreaId,
            Activo = dto.Activo
        };

        public static void AplicarCambios(this ActualizarConsultorioDto dto, Consultorio c)
        {
            c.Codigo = dto.Codigo;
            c.Nombre = dto.Nombre;
            c.Piso = dto.Piso;
            c.SucursalID = dto.SucursalId;
            c.AreaID = dto.AreaId;
            c.Activo = dto.Activo;
        }

        /// <summary>Texto de una línea para combos y listados.</summary>
        public static string Describir(Consultorio c)
        {
            var texto = string.IsNullOrWhiteSpace(c.Codigo) ? c.Nombre : $"{c.Codigo} — {c.Nombre}";

            if (!string.IsNullOrWhiteSpace(c.Area?.Nombre))
            {
                texto += $" ({c.Area!.Nombre})";
            }

            return texto;
        }
    }
}
