using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace SafeInforBox
{
    public class AesOperation
    {
        const string key = "b14ca5667a4e6010bbce2ea2315a1978";
        static readonly byte[] iv = new byte[16] { 15, 16, 16, 17, 95, 96, 96, 97, 85, 86, 86, 87, 75, 76, 76, 77 };

        public static string EncryptString(string plainText)
        {
            byte[] array;
            using (Aes aes = Aes.Create())
            {
                ConfigureAes(aes);

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static string DecryptString(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                ConfigureAes(aes);

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                byte[] buffer = Convert.FromBase64String(cipherText);
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }

        static void ConfigureAes(Aes aes)
        {
            aes.Mode = CipherMode.CBC;
            aes.KeySize = 256;
            aes.BlockSize = 128;
            aes.FeedbackSize = 128;
            //aes.Padding = PaddingMode.None;

            aes.Key = Encoding.UTF8.GetBytes(key);
            aes.IV = iv;
        }
    }
}
