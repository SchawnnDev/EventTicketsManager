using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Library.Utils
{
    public class AESEncryption
    {

        private const int KeySize = 256; // in bits
        public static string EncryptStringToBase64String(string plainText, byte[] key)
        {
            // Check arguments. 
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");
            byte[] returnValue;
            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                var iv = aes.IV;
                if (string.IsNullOrEmpty(plainText))
                    return Convert.ToBase64String(iv);
                var encryptor = aes.CreateEncryptor(key, iv);

                // Create the streams used for encryption. 
                using var msEncrypt = new MemoryStream();
                using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
                using (var swEncrypt = new StreamWriter(csEncrypt))
                {
                    //Write all data to the stream.
                    swEncrypt.Write(plainText);
                }
                // this is just our encrypted data
                var encrypted = msEncrypt.ToArray();
                returnValue = new byte[encrypted.Length + iv.Length];
                // append our IV so our decrypt can get it
                Array.Copy(iv, returnValue, iv.Length);
                // append our encrypted data
                Array.Copy(encrypted, 0, returnValue, iv.Length, encrypted.Length);
            }

            // return encrypted bytes converted to Base64String
            return Convert.ToBase64String(returnValue);
        }

        public static string DecryptStringFromBase64String(string cipherText, byte[] key)
        {
            // Check arguments. 
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("key");

            string plaintext = null;
            // this is all of the bytes
            var allBytes = Convert.FromBase64String(cipherText);

            using (var aes = Aes.Create())
            {
                aes.KeySize = KeySize;
                aes.Mode = CipherMode.CBC;

                // get our IV that we pre-pended to the data
                var iv = new byte[aes.BlockSize / 8];
                if (allBytes.Length < iv.Length)
                    throw new ArgumentException("Message was less than IV size.");
                Array.Copy(allBytes, iv, iv.Length);
                // get the data we need to decrypt
                var cipherBytes = new byte[allBytes.Length - iv.Length];
                Array.Copy(allBytes, iv.Length, cipherBytes, 0, cipherBytes.Length);

                // Create a decrytor to perform the stream transform.
                var decryptor = aes.CreateDecryptor(key, iv);

                // Create the streams used for decryption. 
                using var msDecrypt = new MemoryStream(cipherBytes);
                using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
                using var srDecrypt = new StreamReader(csDecrypt);
                // Read the decrypted bytes from the decrypting stream 
                // and place them in a string.
                plaintext = srDecrypt.ReadToEnd();
            }

            return plaintext;
        }


    }
}
