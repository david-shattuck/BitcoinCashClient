using BitcoinCash.API.Utilities.Interfaces;
using System.Security.Cryptography;

namespace BitcoinCash.API.Utilities
{
    public class Cipher(IConfiguration configuration) : ICipher
    {
        private readonly IConfiguration Configuration = configuration;

        public string Encrypt(string plainText)
        {
            if (!IsEncryptionEnabled())
                return plainText;

            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = GetKey();
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using MemoryStream memoryStream = new();
                using CryptoStream cryptoStream = new(memoryStream, encryptor, CryptoStreamMode.Write);
                using (StreamWriter streamWriter = new(cryptoStream))
                {
                    streamWriter.Write(plainText);
                }

                array = memoryStream.ToArray();
            }

            return Convert.ToBase64String(array);
        }

        public string Decrypt(string cipherText)
        {
            if (!IsEncryptionEnabled())
                return cipherText;

            byte[] iv = new byte[16];
            byte[] buffer = Convert.FromBase64String(cipherText);

            using Aes aes = Aes.Create();
            aes.Key = GetKey();
            aes.IV = iv;
            ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            using MemoryStream memoryStream = new(buffer);
            using CryptoStream cryptoStream = new(memoryStream, decryptor, CryptoStreamMode.Read);
            using StreamReader streamReader = new(cryptoStream);
            return streamReader.ReadToEnd();
        }

        private bool IsEncryptionEnabled() => Convert.ToBoolean(Configuration["EncryptionEnabled"]);

        private byte[] GetKey()
        {
            var key = Configuration["Encryption:Key"];

            return Convert.FromBase64String(key!);
        }
    }
}
