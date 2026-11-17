namespace Catalogos.Core.Wrappers
{
    /// <summary>Mismo sobre estándar que usa Agenda, para que los dos
    /// microservicios respondan igual y el frontend trate los errores en un solo lugar.</summary>
    public class HttpResponse<T>
    {
        public bool Succeeded { get; set; }
        public T? Result { get; set; }
        public int ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
