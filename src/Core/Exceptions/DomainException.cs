namespace Core.Exceptions
{
    public enum Errores
    {
        PAGO_FALLIDO = 601,
        PASARELA_NO_DISPONIBLE = 701,
        AUTENTICACION_FALLIDA = 702
    }

    /// <summary>Error de negocio con código propio; el middleware de la API lo
    /// transforma en una respuesta 400 con el sobre HttpResponse.</summary>
    public class DomainException : Exception
    {
        public Errores Codigo { get; }

        public DomainException(Errores codigo, string message) : base(message)
        {
            Codigo = codigo;
        }
    }
}