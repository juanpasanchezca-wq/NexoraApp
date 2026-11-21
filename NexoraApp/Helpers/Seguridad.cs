using System.Security.Cryptography;
using System.Text;

namespace NexoraApp.Helpers
{
    // Las contraseñas nunca se guardan en texto plano: se guarda su hash (PBKDF2 con sal).
    public static class Seguridad
    {
        // Contraseña de los usuarios de ejemplo
        public const string ContrasenaDemo = "Nexora123";

        public static string Hash(string correo, string contrasena)
        {
            // La sal es distinta para cada usuario (sale de su correo)
            var sal = Encoding.UTF8.GetBytes("nexora|" + correo.Trim().ToLowerInvariant());
            var bytes = Rfc2898DeriveBytes.Pbkdf2(
                Encoding.UTF8.GetBytes(contrasena), sal, 10_000, HashAlgorithmName.SHA256, 32);
            return Convert.ToBase64String(bytes);
        }

        public static bool Verificar(string correo, string contrasena, string hashGuardado)
        {
            try
            {
                return CryptographicOperations.FixedTimeEquals(
                    Convert.FromBase64String(Hash(correo, contrasena)),
                    Convert.FromBase64String(hashGuardado));
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
