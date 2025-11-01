using Core.OptionsModel;
using Core.Repositories;
using Core.Service;
using Core.UnitOfWorks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository;
using Repository.Repositories;
using Repository.UnitOfWorks;
using Service.Mapping;
using Service.Service;
using Service.Services;
using System.Reflection;
using UserInterface.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IWordRepository, WordRepository>();
builder.Services.AddScoped<IWordService, WordService>();
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<IUnknowsService, UnknowsService>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();
builder.Services.AddScoped<IUnknowsRepository, UnknowsRepository>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IRegisterService, RegisterService>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddAutoMapper(typeof(MapProfile));

builder.Services.AddDbContext<AppDbContext>(x =>
{
    x.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection"), option =>
    {
        option.MigrationsAssembly(Assembly.GetAssembly(typeof(AppDbContext)).GetName().Name);
    });
});


builder.Services.AddIdentityWithExtension();
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    options.ValidationInterval = TimeSpan.FromMinutes(30);
});
builder.Services.ConfigureApplicationCookie(options =>
{
    var cookieBuilder = new CookieBuilder();

    cookieBuilder.Name = "Cookie";

    options.LoginPath = new PathString("/Login/LogIn");
    options.AccessDeniedPath = new PathString("/Member/AccessDenied");
    options.Cookie = cookieBuilder;
    options.ExpireTimeSpan = TimeSpan.FromDays(10);
    options.SlidingExpiration = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Register}/{action=Register}/{id?}");
//app.Use(async (context, next) =>
//{
//    if (!context.User.Identity.IsAuthenticated)
//    {
//        context.Response.Redirect("/Login/LogIn");
//        return;
//    }
//    await next();
//});
app.Run();
