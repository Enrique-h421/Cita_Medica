using Core.Dtos;
using Domain.Diagnosticos;
using GenericPersistence.Repository;

namespace Core.Mappings
{
    /// <summary>
    /// Le cuelga a cada atención los diagnósticos que el médico le puso.
    /// </summary>
    public static class DiagnosticosEnricher
    {
        /// <summary>
        /// Tope de filas puente que se traen de una sola vez. Con el listado
        /// tope de 1000 atenciones y varios diagnósticos por consulta, 5000
        /// cubre de sobra el uso real de la clínica.
        /// </summary>
        private const int TopeVinculos = 5000;

        /// <summary>
        /// Hace una sola consulta para todas las atenciones de la lista, no una
        /// por atención: se piden las filas puente de esos Ids con el catálogo
        /// incluido, y el reparto se hace en memoria.
        ///
        /// El nombre del diagnóstico viene por el include a Diagnostico, que es
        /// un solo nivel de navegación — lo único que soporta GenericPersistence.
        /// Por eso los diagnósticos se cargan desde esta tabla hacia el catálogo
        /// y no al revés desde la atención: ese camino necesitaría ThenInclude.
        ///
        /// Si algo falla al resolver el catálogo, el código y el nombre quedan
        /// en null y la atención se devuelve igual.
        /// </summary>
        public static async Task CargarDiagnosticosAsync(
            this List<AtencionDto> atenciones,
            IGenericRepository<AtencionDiagnostico> vinculos,
            CancellationToken cancellationToken = default)
        {
            if (atenciones.Count == 0)
            {
                return;
            }

            var ids = atenciones.Select(a => a.AtencionId).Distinct().ToList();

            var pagina = await vinculos.GetPagedAsync(
                1, TopeVinculos,
                ad => ids.Contains(ad.AtencionID),
                null, "AtencionDiagnosticoID", true, false, cancellationToken,
                ad => ad.Diagnostico!);

            var filas = pagina.Data.ToList();

            if (filas.Count == 0)
            {
                return;
            }

            var porAtencion = filas
                .GroupBy(ad => ad.AtencionID)
                .ToDictionary(g => g.Key, g => g.ToDtoList());

            foreach (var atencion in atenciones)
            {
                if (porAtencion.TryGetValue(atencion.AtencionId, out var suyos))
                {
                    atencion.Diagnosticos = suyos;
                }
            }
        }
    }
}
