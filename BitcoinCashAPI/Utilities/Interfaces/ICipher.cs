namespace BitcoinCash.API.Utilities.Interfaces
{
    public interface ICipher
    {
        string Encrypt(string plainText);

        string Decrypt(string cipherText);
    }
}
