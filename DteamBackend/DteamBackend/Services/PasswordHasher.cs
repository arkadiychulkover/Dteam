using System.Security.Cryptography;
using System.Text;

namespace DteamBackend.Services
{
    public interface IPasswordHasher
    {
        void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt);
        bool VerifyPasswordHash(string password, string storedHash, string storedSalt);
    }

    public class PasswordHasher : IPasswordHasher
    {
        public void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using var hmac = new HMACSHA512();
            var saltBytes = hmac.Key;
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            passwordSalt = Convert.ToBase64String(saltBytes);
            passwordHash = Convert.ToBase64String(hashBytes);
        }

        public bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash) || string.IsNullOrWhiteSpace(storedSalt))
                return false;

            try
            {
                var saltBytes = Convert.FromBase64String(storedSalt);
                var storedHashBytes = Convert.FromBase64String(storedHash);

                using var hmac = new HMACSHA512(saltBytes);
                var computedHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                return CryptographicOperations.FixedTimeEquals(computedHashBytes, storedHashBytes);
            }
            catch
            {
                return false;
            }
        }
    }
}
