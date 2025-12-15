namespace WordMaster.Application.Helpers;

public interface IDataProtectionHelper
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
