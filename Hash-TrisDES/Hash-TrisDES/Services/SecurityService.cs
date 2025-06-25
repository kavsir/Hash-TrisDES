using System.Security.Cryptography;
using System.Text;

namespace Hash_TrisDES.Services
{
    public class SecurityService
    {
        private readonly string secretKey = "SuperSecretKey123"; // nên lưu trong cấu hình

        public string GenerateSalt()
        {
            byte[] saltBytes = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            return Convert.ToBase64String(saltBytes);
        }

        public string EncryptPassword(string username, string password, string salt)
        {
            string hashedUsername = HashSHA256(username);
            string hashedPasswordSalt = HashSHA256(password + salt);

            string combined = hashedUsername + hashedPasswordSalt;
            string finalHash = HashSHA256(combined);

            return EncryptTripleDES(finalHash);
        }

        private string HashSHA256(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        private string EncryptTripleDES(string plainText)
        {
            using (TripleDES tripleDES = TripleDES.Create())
            {
                using (SHA256 sha = SHA256.Create())
                {
                    tripleDES.Key = sha.ComputeHash(Encoding.UTF8.GetBytes(secretKey)).Take(24).ToArray();
                }

                tripleDES.Mode = CipherMode.ECB;
                tripleDES.Padding = PaddingMode.PKCS7;

                var data = Encoding.UTF8.GetBytes(plainText);
                var encryptor = tripleDES.CreateEncryptor();
                byte[] encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
                return Convert.ToBase64String(encrypted);
            }
        }
    }
}
