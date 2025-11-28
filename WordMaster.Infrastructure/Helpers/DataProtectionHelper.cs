using Microsoft.AspNetCore.DataProtection;

namespace WordMaster.Infrastructure.Helpers;

public interface IDataProtectionHelper
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}

public class DataProtectionHelper(IDataProtectionProvider provider) : IDataProtectionHelper
{
    private readonly IDataProtector _protector = provider.CreateProtector("WordMaster.UrlProtection+Zj1rA");

    public string Encrypt(string plainText)
    {
        return _protector.Protect(plainText);
    }

    public string Decrypt(string cipherText)
    {
        try
        {
            return _protector.Unprotect(cipherText);
        }
        catch
        {
            // Decryption failed
            return string.Empty;
        }
    }
}

