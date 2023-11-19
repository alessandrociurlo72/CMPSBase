using FrmNetCore.Extensions;
using System.IO;
using System.Security.Cryptography;

namespace FrmNetCore.Crypto
{
    public class SHA1Helper
    {

        public static byte[] CalculateSHA1(string path)
        {


            using (var stream = File.OpenRead(path))
            {

                using (SHA1 sha1 = SHA1.Create())
                {

                    return sha1.ComputeHash(stream);

                }


            }
        }

        public static string SHA1String(string path)
        {
            byte[] arrbyte = CalculateSHA1(path);
            return arrbyte.ToHex(true);


        }
    }
}
