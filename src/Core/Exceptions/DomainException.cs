namespace Core.Exceptions
{
    public enum Errores
    {
        // Pagos y pasarela externa
        PAGO_FALLIDO = 601,
        PASARELA_NO_DISPONIBLE = 701,
        AUTENTICACION_FALLIDA = 702,

        // Agenda y atención
        CITA_NO_ENCONTRADA = 801,
        CITA_INVALIDA = 802,

        // Datos que viven en el microservicio de Catálogos
        PACIENTE_NO_ENCONTRADO = 803,
        MEDICO_NO_ENCONTRADO = 804,
        ESPECIALIDAD_NO_ENCONTRADA = 805,
        CATALOGOS_NO_DISPONIBLE = 806,

        // Recursos propios de Agenda
        CONSULTORIO_NO_ENCONTRADO = 807,
        SUCURSAL_NO_ENCONTRADA = 808,
        AREA_NO_ENCONTRADA = 809,

        // Atenciones y expediente clínico
        ATENCION_NO_ENCONTRADA = 810,
        ATENCION_INVALIDA = 811,
        ATENCION_YA_CERRADA = 812,
        ATENCION_YA_ANULADA = 813,
        EXPEDIENTE_NO_ENCONTRADO = 814,

        // Diagnósticos (catálogo CIE-10 y diagnósticos de una consulta)
        DIAGNOSTICO_NO_ENCONTRADO = 815,
        DIAGNOSTICO_INVALIDO = 816,
        DIAGNOSTICO_DUPLICADO = 817
    }

    /// <summary>Error de negocio con código propio; el middleware de la API lo
    /// transforma en una respuesta con el sobre HttpResponse.</summary>
    public class DomainException : Exception
    {
        public Errores Codigo { get; }

        public DomainException(Errores codigo, string message) : base(message)
        {
            Codigo = codigo;
        }
    }
}
