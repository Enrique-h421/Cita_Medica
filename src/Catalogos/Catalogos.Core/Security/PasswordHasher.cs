using System.Security.Cryptography;

namespace Catalogos.Core.Security
{
    /// <summary>
    /// Verifica y genera contraseñas con PBKDF2-HMAC-SHA256.
    ///
    /// Formato guardado:  PBKDF2$iteraciones$sal_base64$hash_base64
    ///
    /// Tres decisiones que importan:
    ///
    /// 1. Cada contraseña tiene su propia sal aleatoria. Dos usuarios con la
    ///    misma contraseña producen hashes distintos, así que nadie puede
    ///    deducir quién comparte contraseña con quién mirando la tabla.
    ///
    /// 2. 100.000 iteraciones. El cálculo es deliberadamente lento: para un
    ///    login son milisegundos, pero para alguien probando millones de
    ///    contraseñas robadas lo vuelve impagable.
    ///
    /// 3. La comparación es de tiempo fijo. Si se comparara con == normal, el
    ///    tiempo que tarda en fallar revelaría cuántos bytes acertó, y eso se
    ///    puede usar para adivinar el hash byte por byte.
    ///
    /// El número de iteraciones se lee del propio hash guardado, no de la
    /// constante: así se puede subir el costo para los usuarios nuevos sin
    /// invalidar las contraseñas de los que ya existen.
    /// </summary>
    public static class PasswordHasher
    {
        private const string Etiqueta = "PBKDF2";
        private const int IteracionesPorDefecto = 100_000;
        private const int TamanoSal = 16;
        private const int TamanoHash = 32;

        private static readonly HashAlgorithmName Algoritmo = HashAlgorithmName.SHA256;

        /// <summary>Genera el hash de una contraseña nueva.</summary>
        public static string Hashear(string password, int iteraciones = IteracionesPorDefecto)
        {
            var sal = RandomNumberGenerator.GetBytes(TamanoSal);

            var hash = Rfc2898DeriveBytes.Pbkdf2(password, sal, iteraciones, Algoritmo, TamanoHash);

            return string.Join('$',
                Etiqueta,
                iteraciones.ToString(),
                Convert.ToBase64String(sal),
                Convert.ToBase64String(hash));
        }

        /// <summary>
        /// Dice si la contraseña corresponde al hash guardado. Devuelve false
        /// ante cualquier problema — hash vacío, formato roto, base64 inválido —
        /// en lugar de lanzar: un registro mal guardado no debe tumbar el login
        /// de los demás, y tampoco debe dejar entrar a nadie.
        /// </summary>
        public static bool Verificar(string? password, string? hashGuardado)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(hashGuardado))
            {
                return false;
            }

            var partes = hashGuardado.Split('$');

            if (partes.Length != 4 || partes[0] != Etiqueta)
            {
                return false;
            }

            if (!int.TryParse(partes[1], out var iteraciones) || iteraciones <= 0)
            {
                return false;
            }

            byte[] sal;
            byte[] esperado;

            try
            {
                sal = Convert.FromBase64String(partes[2]);
                esperado = Convert.FromBase64String(partes[3]);
            }
            catch (FormatException)
            {
                return false;
            }

            if (sal.Length == 0 || esperado.Length == 0)
            {
                return false;
            }

            var calculado = Rfc2898DeriveBytes.Pbkdf2(password, sal, iteraciones, Algoritmo, esperado.Length);

            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
    }
}
