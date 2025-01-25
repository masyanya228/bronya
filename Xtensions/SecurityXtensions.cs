using System.Security.Cryptography;
using System.Text;

namespace Buratino.Xtensions
{
    public static class SecurityXtensions
    {
        public static string GetSHA512(this string txt)
        {
            var cr = new SHA512CryptoServiceProvider();
            byte[] hashRes = cr.ComputeHash(Encoding.UTF8.GetBytes(txt));
            string res = Convert.ToBase64String(hashRes);
            return res;
        }
        public static string GetSHA512(this byte[] data)
        {
            var cr = new SHA512CryptoServiceProvider();
            byte[] hashRes = cr.ComputeHash(data);
            string res = Convert.ToBase64String(hashRes);
            return res;
        }
    }
}