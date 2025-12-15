using Microsoft.AspNetCore.DataProtection;
using WordMaster.Application.Helpers;

namespace WordMaster.Infrastructure.Helpers;

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
            return string.Empty;
        }
    }
}

