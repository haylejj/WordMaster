using Core.Entity;
using Core.Requests;

namespace Core.Service
{
    public interface ILoginService
    {
        Task<AppUser> FindByEmailAsync(string email);
        Task<bool> LoginAsync(LoginRequest request, AppUser user);
        Task<string> GeneratePasswordResetTokenAsync(string userıd);
    }
}
