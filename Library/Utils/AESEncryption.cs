﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Library.Utils
{
    public class AESEncryption
    {

		public static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
		{
			// Check arguments.
			if (plainText == null || plainText.Length <= 0)
				throw new ArgumentNullException("plainText");
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("Key");
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException("IV");
			byte[] encrypted;

			// Create an Aes object
			// with the specified key and IV.
			using (var aesAlg = Aes.Create())
			{
				aesAlg.Key = Key;
				aesAlg.IV = IV;

				// Create an encryptor to perform the stream transform.
				var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

				// Create the streams used for encryption.
				using var msEncrypt = new MemoryStream();
				using var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
				using (var swEncrypt = new StreamWriter(csEncrypt))
				{
					//Write all data to the stream.
					swEncrypt.Write(plainText);
				}
				encrypted = msEncrypt.ToArray();
			}


			// Return the encrypted bytes from the memory stream.
			return encrypted;

		}

		public static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
		{
			// Check arguments.
			if (cipherText == null || cipherText.Length <= 0)
				throw new ArgumentNullException("cipherText");
			if (Key == null || Key.Length <= 0)
				throw new ArgumentNullException("Key");
			if (IV == null || IV.Length <= 0)
				throw new ArgumentNullException("IV");

			// Declare the string used to hold
			// the decrypted text.
			string plaintext;

			// Create an Aes object
			// with the specified key and IV.
			using (var aesAlg = Aes.Create())
			{
				aesAlg.Key = Key;
				aesAlg.IV = IV;

				// Create a decryptor to perform the stream transform.
				var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

				// Create the streams used for decryption.
				using var msDecrypt = new MemoryStream(cipherText);
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