namespace Domain.Seguridad
{
    /// <summary>
    /// Nombres de rol tal como viajan dentro del token, para usarlos en los
    /// atributos [Authorize(Roles = ...)] de este microservicio.
    ///
    /// Agenda no emite tokens, solo los verifica: los tokens los firma el
    /// microservicio de Catálogos. Por eso aquí solo están las constantes y no
    /// la función que normaliza el nombre — esa corre del lado que arma el token.
    ///
    /// OJO: este archivo tiene su gemelo en Catálogos
    /// (Catalogos.Domain/Seguridad/RolesSistema.cs). Los dos tienen que decir
    /// lo mismo; si se agrega un rol, se agrega en los dos.
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
    }
}
