using System.Security.Cryptography;

namespace PlumitaGrisAPI.Utils
{
    /// <summary>
    /// Utilidad para generar y verificar hashes de contraseñas usando PBKDF2 (Rfc2898DeriveBytes).
    /// No requiere paquetes NuGet adicionales.
    /// Formato almacenado: {iteraciones}.{saltBase64}.{hashBase64}
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;      // 128 bits
        private const int HashSize = 32;      // 256 bits
        private const int Iterations = 100_000;

        public static string Hash(string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256,
                HashSize);

            return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
        }

        public static bool Verify(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            var partes = hashedPassword.Split('.', 3);
            if (partes.Length != 3)
                return false; // formato inesperado (p.ej. contraseña antigua en texto plano)

            if (!int.TryParse(partes[0], out int iterations))
                return false;

            byte[] salt;
            byte[] hash;
            try
            {
                salt = Convert.FromBase64String(partes[1]);
                hash = Convert.FromBase64String(partes[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] hashIngresado = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                hash.Length);

            return CryptographicOperations.FixedTimeEquals(hash, hashIngresado);
        }
    }
}
