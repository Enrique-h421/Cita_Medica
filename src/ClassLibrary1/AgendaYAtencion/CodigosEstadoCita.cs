namespace Domain.AgendaYAtencion
{
    /// <summary>
    /// Códigos de la tabla Estados_Cita. El Id numérico cambia según el orden en
    /// que se sembraron los estados, así que el código es la forma estable de
    /// referirse a uno desde la lógica de negocio.
    /// </summary>
    public static class CodigosEstadoCita
    {
        public const string Pendiente = "PEND";
        public const string Confirmada = "CONF";
        public const string Atendida = "ATEN";
        public const string Cancelada = "CANC";
        public const string NoAsistio = "NOAS";
    }
}
