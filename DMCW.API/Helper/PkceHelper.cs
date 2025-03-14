using System.Security.Cryptography;
using System.Text;

namespace DMCW.API.Helper
{
    public class PkceHelper
    {
        public static string GenerateCodeVerifier()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                var bytes = new byte[32];
                rng.GetBytes(bytes);
                return Base64UrlEncode(bytes);
            }
        }

        public static string GenerateCodeChallenge(string codeVerifier)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.ASCII.GetBytes(codeVerifier);
                var hash = sha256.ComputeHash(bytes);
                return Base64UrlEncode(hash);
            }
        }

        private static string Base64UrlEncode(byte[] input)
        {
            var output = Convert.ToBase64String(input);
            output = output.Replace("+", "-").Replace("/", "_").Replace("=", "");
            return output;
        }
    }
}
