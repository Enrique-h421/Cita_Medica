using Core.Dtos;
using Domain.Diagnosticos;

namespace Core.Mappings
{
    /// <summary>
    /// Traduce el catálogo de diagnósticos y los diagnósticos de una consulta
    /// a los DTOs que viajan por la API. A mano, igual que el resto: son pocos
    /// campos y así queda a la vista qué se expone y qué no.
    /// </summary>
    public static class DiagnosticoMapper
    {
        public static DiagnosticoDto ToDto(this Diagnostico d) => new DiagnosticoDto
        {
            DiagnosticoId = d.DiagnosticoID,
            CodigoCIE10 = d.CodigoCIE10,
            Nombre = d.Nombre,
            Descripcion = d.Descripcion
        };

        public static List<DiagnosticoDto> ToDtoList(this IEnumerable<Diagnostico> items) =>
            items.Select(x => x.ToDto()).ToList();

        /// <summary>
        /// El código y el nombre salen de la navegación al catálogo. Si el
        /// include no se pidió llegan en null, y la pantalla muestra el
        /// diagnóstico por Id: degradar es mejor que fallar.
        /// </summary>
        public static AtencionDiagnosticoDto ToDto(this AtencionDiagnostico ad) =>
            new AtencionDiagnosticoDto
            {
                AtencionDiagnosticoId = ad.AtencionDiagnosticoID,
                AtencionId = ad.AtencionID,
                DiagnosticoId = ad.DiagnosticoID,
                CodigoCIE10 = ad.Diagnostico?.CodigoCIE10,
                Nombre = ad.Diagnostico?.Nombre,
                TipoDiagnostico = ad.TipoDiagnostico,
                Observaciones = ad.Observaciones
            };

        /// <summary>
        /// Ordena como se lee un expediente: primero el principal, después los
        /// secundarios en el orden en que el médico los fue agregando.
        /// </summary>
        public static List<AtencionDiagnosticoDto> ToDtoList(
            this IEnumerable<AtencionDiagnostico> items) =>
            items
                .OrderBy(x => x.TipoDiagnostico == TiposDiagnostico.Principal ? 0 : 1)
                .ThenBy(x => x.AtencionDiagnosticoID)
                .Select(x => x.ToDto())
                .ToList();
    }
}
