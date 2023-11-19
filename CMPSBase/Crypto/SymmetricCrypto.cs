using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FrmNetCore.Crypto
{
    public class SymmetricCrypto
    {
        public static bool AESEncryptFile(string inputFile, string keypass, ref string outpath, out string msg)
        {
            msg = null;
            try
            {
                //generate random salt
                byte[] salt = GenerateRandomBytes(32);


                if (String.IsNullOrEmpty(outpath))
                {
                    outpath = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile) + ".aes");
                }

                //create output file name
                using (FileStream fsCrypt = new FileStream(outpath, FileMode.Create))
                {

                    //convert password string to byte arrray
                    byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(keypass);

                    //Set Rijndael symmetric encryption algorithm
                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = 256;
                        aes.BlockSize = 128;
                        aes.Padding = PaddingMode.PKCS7;


                        var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        aes.IV = key.GetBytes(aes.BlockSize / 8);

                        aes.Mode = CipherMode.CFB;

                        //write salt to the begining of the output file, so in this case can be random every time
                        fsCrypt.Write(salt, 0, salt.Length);

                        using (CryptoStream cs = new CryptoStream(fsCrypt, aes.CreateEncryptor(), CryptoStreamMode.Write))
                        {

                            using (FileStream fsIn = new FileStream(inputFile, FileMode.Open))
                            {

                                //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
                                byte[] buffer = new byte[1048576];
                                int read;

                                while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    cs.Write(buffer, 0, read);
                                }

                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }
        }

        public static bool AESDecryptFile(string inputFile, string keypass, ref string outpath, out string msg)
        {
            msg = string.Empty;
            try
            {
                if (!File.Exists(inputFile))
                    return false;

                if (String.IsNullOrEmpty(outpath))
                    outpath = Path.Combine(Path.GetDirectoryName(inputFile), Path.GetFileNameWithoutExtension(inputFile) + ".decrypted");

                byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(keypass);
                byte[] salt = new byte[32];

                using (FileStream fsCrypt = new FileStream(inputFile, FileMode.Open))
                {
                    fsCrypt.Read(salt, 0, salt.Length);

                    using (Aes aes = Aes.Create())
                    {
                        aes.KeySize = 256;
                        aes.BlockSize = 128;
                        var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
                        aes.Key = key.GetBytes(aes.KeySize / 8);
                        aes.IV = key.GetBytes(aes.BlockSize / 8);
                        aes.Padding = PaddingMode.PKCS7;
                        aes.Mode = CipherMode.CFB;

                        using (CryptoStream cs = new CryptoStream(fsCrypt, aes.CreateDecryptor(), CryptoStreamMode.Read))
                        {

                            using (FileStream fsOut = new FileStream(outpath, FileMode.Create))
                            {
                                int read;
                                byte[] buffer = new byte[1048576];
                                while ((read = cs.Read(buffer, 0, buffer.Length)) > 0)
                                    fsOut.Write(buffer, 0, read);
                            }
                        }
                    }
                }
                return true;
            }
            catch (System.Security.Cryptography.CryptographicException ex_CryptographicException)
            {
                msg = ex_CryptographicException.Message;
                return false;
            }
            catch (Exception ex)
            {
                msg = ex.Message;
                return false;
            }

        }

        public string Encrypt(string text, string IV, string key)
        {
            Aes cipher = CreateCipher(key);
            cipher.IV = Convert.FromBase64String(IV);

            ICryptoTransform cryptTransform = cipher.CreateEncryptor();
            byte[] plaintext = Encoding.UTF8.GetBytes(text);
            byte[] cipherText = cryptTransform.TransformFinalBlock(plaintext, 0, plaintext.Length);

            return Convert.ToBase64String(cipherText);
        }

        public string Decrypt(string encryptedText, string IV, string key)
        {
            Aes cipher = CreateCipher(key);
            cipher.IV = Convert.FromBase64String(IV);

            ICryptoTransform cryptTransform = cipher.CreateDecryptor();
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            byte[] plainBytes = cryptTransform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }

        public (string Key, string IVBase64) InitSymmetricEncryptionKeyIV()
        {
            var key = GetEncodedRandomString(32); // 256
            Aes cipher = CreateCipher(key);
            var IVBase64 = Convert.ToBase64String(cipher.IV);
            return (key, IVBase64);
        }

        public string GetEncodedRandomString(int length)
        {
            var rnd = Convert.ToBase64String(GenerateRandomBytes(length));
            return rnd;
        }

        private Aes CreateCipher(string keyBase64)
        {
            // Default values: Keysize 256, Padding PKC27
            Aes cipher = Aes.Create();
            cipher.Mode = CipherMode.CBC;  // Ensure the integrity of the ciphertext if using CBC

            cipher.Padding = PaddingMode.ISO10126;
            cipher.Key = Convert.FromBase64String(keyBase64);

            return cipher;
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            var byteArray = new byte[length];
            RandomNumberGenerator.Fill(byteArray);
            return byteArray;
        }

        public static byte[] GenerateRandomSalt(int length = 32, int iteration = 10)
        {

            byte[] data = new byte[32];

            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                // Ten iterations.
                for (int i = 0; i < 10; i++)
                {
                    // Fill buffer.
                    rng.GetBytes(data);
                }
            }
            return data;
        }


    }

}
