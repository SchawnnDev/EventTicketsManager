using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Library.Utils;
using Server;

namespace Library.Api
{
    public class KeyGenerator
    {

        private readonly SaveableEvent _event;

        public KeyGenerator(SaveableEvent saveableEvent)
        {
            _event = saveableEvent;
        }

        public string GenerateNewKey()
        {
            //AESEncryption.EncryptStringToBase64String(DateTime.Now.ToString("g"), key).Base64Encode();
            // Convert.FromBase64String(Base64Key);
            var random = new Random();
            var key = new byte[random.Next(5,10)];
            random.NextBytes(key);
            return Convert.ToBase64String(key.Combine(Encoding.Unicode.GetBytes(_event.Name)));
        }

       // private string Base64Key => "+CffHxKmykUvCrrCILd4rZDBcrIoe3w89jnPNXYi0rU="; 

    }
}
