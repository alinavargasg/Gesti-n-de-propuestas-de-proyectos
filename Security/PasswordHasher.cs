using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace AuthenticationModule.Security
{
    public static class PasswordHasher
    {
        // Constantes para la configuración de seguridad
        private const int SaltSize = 128 / 8;      // 16 bytes = 128 bits
        private const int HashSize = 256 / 8;      // 32 bytes = 256 bits
        private const int Iterations = 10000;      // Número de iteraciones
        private const KeyDerivationPrf Prf = KeyDerivationPrf.HMACSHA256;

        /// <summary>
        /// Genera un hash y salt para una contraseña
        /// </summary>
        public static (string hash, string salt) HashPassword(string password)
        {
            // 1. Generar Salt aleatorio
            byte[] saltBytes = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);  // Rellena con bytes aleatorios
            }

            string salt = Convert.ToBase64String(saltBytes);

            // 2. Generar Hash usando PBKDF2
            byte[] hashBytes = KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: Prf,
                iterationCount: Iterations,
                numBytesRequested: HashSize);

            string hash = Convert.ToBase64String(hashBytes);

            return (hash, salt);
        }

        /// <summary>
        /// Verifica si una contraseña coincide con el hash almacenado
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash, string storedSalt)
        {
            // 1. Convertir el salt almacenado a bytes
            byte[] saltBytes = Convert.FromBase64String(storedSalt);

            // 2. Calcular el hash de la contraseña proporcionada
            byte[] computedHashBytes = KeyDerivation.Pbkdf2(
                password: password,
                salt: saltBytes,
                prf: Prf,
                iterationCount: Iterations,
                numBytesRequested: HashSize);

            string computedHash = Convert.ToBase64String(computedHashBytes);

            // 3. Comparar de forma segura (evita timing attacks)
            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(computedHash),
                Convert.FromBase64String(storedHash));
        }
    }
}
