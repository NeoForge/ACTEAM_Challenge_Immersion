using System.Security.Cryptography;
using System.Text;

namespace GestionStock.Data
{
    public static class PasswordHasher
    {
        public static byte[] GenerateSalt()
        {
            const int saltLength = 32;
            {
                var randomNumber = new byte[saltLength];
                var rnd = RandomNumberGenerator.Create();
                rnd.GetBytes(randomNumber);
                return randomNumber;
            }
        }
        public static byte[] Combine(byte[] first , byte[] second)
        {
            var ret = new byte[first.Length + second.Length];

            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);

            return ret;
        }
        public static byte[] HashPasswordWithSalt(byte[] toBeHashed, byte[] salt)
         {
            using (var sha256 = SHA256.Create())
            {
                var combinedHash = Combine(toBeHashed, salt);
                return sha256.ComputeHash(combinedHash);
            }
        }

        //VerifyPasswordWithSalt
        public static bool VerifyPasswordWithSalt(byte[] hashedPassword, string salt, string password)
        {
            var hashedPasswordWithSalt = HashPasswordWithSalt(Encoding.UTF8.GetBytes(password), Convert.FromBase64String(salt));
            return hashedPasswordWithSalt.SequenceEqual(hashedPassword);
        }
    }
}
