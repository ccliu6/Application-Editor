﻿using System;
using System.IO;
using System.Text;

namespace EncrytionCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            //Read in the original text file
            var reader = new StreamReader(File.OpenRead("C:\\Spm1\\Application Modes\\smallInput.txt"));
            string plainText = reader.ReadToEnd();
            reader.Close();
            Console.WriteLine($"Read-in plainText {plainText.Length} chars");

            //Creat an encryoter
            RyptoObject encrypter = new RyptoObject("");
            string key = "a4rR1lKeIHmGuWj5jm5TX1aiAWdRFWkFRAkGJSK";
            encrypter.SetKey(ref key);

            //Encrtpt
            byte[] enBytes;
            encrypter.encrypt(plainText, out enBytes);
            File.WriteAllBytes("C:\\Spm1\\Application Modes\\smallE.bin", enBytes);
            Console.WriteLine($"Writing Encrypted {enBytes.Length} bytes");

            //Read in the encrypted file
            string cipherText = File.ReadAllText("C:\\Spm1\\Application Modes\\smallE.bin");
            Console.WriteLine($"Read-in cipherText {cipherText.Length} chars");

            //Decrypt it and wrtite out
            string decryptedStr = "";
            encrypter.SeedGenerator("");
            encrypter.decrypt(cipherText, ref decryptedStr);
            Console.WriteLine($"Writing Decrypted {decryptedStr.Length} chars");
            File.WriteAllText("C:\\Spm1\\Application Modes\\smallOut.txt", decryptedStr);

            Console.ReadLine();
        }
    }
}
