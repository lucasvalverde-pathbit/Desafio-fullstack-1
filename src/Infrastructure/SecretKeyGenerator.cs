using System.Security.Cryptography;

namespace Infrastructure
{
    public static class SecurityKeyGenerator
    {
        public static string GenerateSecretKey(int length = 64)
        {
            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                var randomNumber = new byte[length];
                randomNumberGenerator.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber).TrimEnd('=');
            }
        }
    }
}
