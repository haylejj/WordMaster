using Core.OptionsModel;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Repository;
using Service.Extensions;
using Service.Mapping;
using Service.Validators;
using System.Reflection;
using UserInterface.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Application Services
builder.Services.AddApplicationServices();

builder.Services.AddAutoMapper(typeof(MapProfile));

builder.Services.AddDbContext<AppDbContext>(x =>
{
    x.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"), option =>
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
    var cookieBuilder = new CookieBuilder
    {
        Name = "SecureCookie",
        HttpOnly = true,
        SecurePolicy = CookieSecurePolicy.Always,
        IsEssential = true
    };

    options.LoginPath = new PathString("/Login");
    options.AccessDeniedPath = new PathString("/Member/AccessDenied");
    options.Cookie = cookieBuilder;

    options.ExpireTimeSpan = TimeSpan.FromDays(1);

    options.SlidingExpiration = true;
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
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
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Word}/{action=Index}/{id?}");
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
