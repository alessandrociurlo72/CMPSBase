using FrmNetCore.Extensions;
using System.IO;
using System.Security.Cryptography;

namespace FrmNetCore.Crypto
{
    public class MD5Helper
    {
        /// <summary>
        /// Calculate Hash MD5 of a file
        /// </summary>
        /// <param name="path">path of the file for that MD5 has to be calculated</param>
        /// <returns>MD5 in array of bytes format</returns>
        /// <excpetion>if file is not accessible</excpetion>
        public static byte[] CalculateMD5(string path)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(path))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }
        /// <summary>
        /// Calculate Hash MD5 of a file
        /// </summary>
        /// <param name="path">path of the file for that MD5 has to be calculated</param>
        /// <returns>MD5 in string format</returns>
        /// <excpetion>if file is not accessible</excpetion>
        public static string MD5String(string path)
        {

            byte[] arrbyte = CalculateMD5(path);
            return arrbyte.ToHex(true);

        }


    }
}
