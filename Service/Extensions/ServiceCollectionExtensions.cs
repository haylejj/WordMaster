using Core.Repositories;
using Core.Service;
using Core.UnitOfWorks;
using Microsoft.Extensions.DependencyInjection;
using Repository.Repositories;
using Repository.UnitOfWorks;
using Service.Service;
using Service.Services;

namespace Service.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // UnitOfWork
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Generic Repository
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Repositories
            services.AddScoped<IWordRepository, WordRepository>();
            services.AddScoped<IFavoriteRepository, FavoriteRepository>();
            services.AddScoped<IUnknowsRepository, UnknowsRepository>();

            // Services
            services.AddScoped<IWordService, WordService>();
            services.AddScoped<IFavoriteService, FavoriteService>();
            services.AddScoped<IUnknowsService, UnknowsService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IRegisterService, RegisterService>();
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<IEmailService, EmailService>();

            return services;
        }
    }
}
