namespace Core.Wrappers
{
    /// <summary>Sobre estándar con el que todos los microservicios del sistema
    /// responden, tanto en éxito como en error.</summary>
    public class HttpResponse<T>
    {
        public bool Succeeded { get; set; }
        public T? Result { get; set; }
        public int ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }
    }
}