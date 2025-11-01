using Core.Entity;
using Core.Service;
using Core.Requests;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace Service.Service
{
    public class LoginService : ILoginService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;


        public LoginService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;

        }
        public async Task<AppUser> FindByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }
        public async Task<bool> LoginAsync(LoginRequest request, AppUser user)
        {
            var result = await _signInManager.PasswordSignInAsync(user, request.Password, request.RememberMe, true);

            if (result.Succeeded)
            {
                return (true);
            }

            return (false);
        }
        public async Task<string> GeneratePasswordResetTokenAsync(string userıd)
        {
            var user = await _userManager.FindByIdAsync(userıd);
            return await _userManager.GeneratePasswordResetTokenAsync(user);

        }


    }
}
