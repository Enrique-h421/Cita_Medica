namespace Catalogos.Core.Exceptions
{
    public enum Errores
    {
        PACIENTE_NO_ENCONTRADO = 901,
        MEDICO_NO_ENCONTRADO = 902,
        ESPECIALIDAD_NO_ENCONTRADA = 903,
        DATOS_INVALIDOS = 904
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
