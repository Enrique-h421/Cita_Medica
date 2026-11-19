namespace Catalogos.Core.Configs
{
    /// <summary>
    /// Configuración del token. Vive en appsettings.json bajo la sección "Jwt".
    ///
    /// Los dos microservicios tienen que tener EXACTAMENTE los mismos valores:
    /// Catálogos firma el token y Agenda verifica esa firma. Si la llave, el
    /// emisor o la audiencia difieren en un carácter, Agenda rechaza todos los
    /// tokens con 401 y el error no dice por qué.
    /// </summary>
    public class JwtOptions
    {
        public const string Seccion = "Jwt";

        /// <summary>Quién emite el token. Lo emite Catálogos.</summary>
        public string Issuer { get; set; } = "CitaMedica.Catalogos";

        /// <summary>Para quién vale el token: los microservicios del sistema.</summary>
        public string Audience { get; set; } = "CitaMedica";

        /// <summary>
        /// Llave simétrica con la que se firma. HMAC-SHA256 necesita al menos
        /// 32 bytes, así que una llave corta hace que el servicio truene al
        /// arrancar en lugar de al primer login.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>Cuánto dura la sesión. 480 minutos = una jornada de trabajo.</summary>
        public int MinutosExpiracion { get; set; } = 480;
    }
}
