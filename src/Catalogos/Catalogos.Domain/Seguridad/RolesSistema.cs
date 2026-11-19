using System.Globalization;
using System.Text;

namespace Catalogos.Domain.Seguridad
{
    /// <summary>
    /// Nombres de rol tal como viajan dentro del token y como se escriben en los
    /// atributos [Authorize(Roles = ...)].
    ///
    /// En la base los roles están con acento ("Médico", "Recepción"), y un
    /// acento mal escrito en un atributo deja a alguien afuera sin ninguna
    /// señal de por qué. Así que el nombre se normaliza una sola vez, al armar
    /// el token, y de ahí en adelante todos comparan contra estas constantes.
    ///
    /// OJO: este archivo está duplicado en el microservicio de Agenda
    /// (Domain/Seguridad/RolesSistema.cs). Los dos tienen que decir lo mismo:
    /// Agenda valida tokens que emite Catálogos. Si se agrega un rol, se agrega
    /// en los dos.
    /// </summary>
    public static class RolesSistema
    {
        public const string Administrador = "Administrador";
        public const string Medico = "Medico";
        public const string Recepcion = "Recepcion";

        // Combinaciones para [Authorize(Roles = ...)], que espera los roles
        // separados por coma. Se concatenan como constantes para que el atributo
        // siga siendo una expresión constante y no haya que escribir las comas
        // a mano en cada controlador.
        public const string AdministradorORecepcion = Administrador + "," + Recepcion;
        public const string AdministradorOMedico = Administrador + "," + Medico;

        /// <summary>
        /// Quita los acentos del nombre guardado en la base: "Médico" queda
        /// como "Medico" y "Recepción" como "Recepcion".
        /// </summary>
        public static string Normalizar(string? nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return string.Empty;
            }

            var descompuesto = nombre.Trim().Normalize(NormalizationForm.FormD);
            var limpio = new StringBuilder(descompuesto.Length);

            foreach (var caracter in descompuesto)
            {
                // Los acentos quedan como caracteres aparte al descomponer;
                // se descartan y queda la letra sola.
                if (CharUnicodeInfo.GetUnicodeCategory(caracter) != UnicodeCategory.NonSpacingMark)
                {
                    limpio.Append(caracter);
                }
            }

            return limpio.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
