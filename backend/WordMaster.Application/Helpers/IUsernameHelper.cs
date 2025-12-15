namespace WordMaster.Application.Helpers;

/// <summary>
/// Kullanıcı adı oluşturma işlemleri için helper interface.
/// </summary>
public interface IUsernameHelper
{
    /// <summary>
    /// FirstName ve LastName'den benzersiz bir username oluşturur.
    /// Örn: "Ahmet", "Yılmaz" -> "ahmetyilmaz"
    /// Eğer kullanılıyorsa sonuna sayı ekler.
    /// </summary>
    Task<string> GenerateUniqueUsernameAsync(string firstName, string lastName);
}
