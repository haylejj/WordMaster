using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using WordMaster.API.Extensions;
using WordMaster.API.Filters;
using WordMaster.Domain.Configuration;
using WordMaster.Infrastructure.EfCore;
using WordMaster.Infrastructure.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ValidationFilter'ı global olarak ekle (tüm controller'larda model doğrulama hatalarını otomatik yakalar)
builder.Services.AddControllers(configure =>
{
    configure.Filters.Add<ValidationFilter>();
});

// Fluent Validation yapılandırması
builder.Services.AddValidationConfigurations();

// HttpContextAccessor (LoginService'te IP adresi almak için gerekli)
builder.Services.AddHttpContextAccessor();

// Configuration Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<MailHogSettings>(builder.Configuration.GetSection("MailHog"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt")); // "Jwt" section name

// JWT Authentication yapılandırması
builder.Services.AddJwtConfigurations(builder.Configuration);

// Application Services (DI)
builder.Services.AddApplicationServices();
// Redis Cache yapılandırması
builder.Services.AddRedis(builder.Configuration);
// Authorization
builder.Services.AddAuthorization();
// Forwarded Headers yapılandırması
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
// DbContext yapılandırması
builder.Services.AddDbContext<AppDbContext>(x =>
{
    x.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"), option =>
    {
        option.MigrationsAssembly(Assembly.GetAssembly(typeof(AppDbContext))!.GetName().Name);
    });
});
// Identity yapılandırması
builder.Services.AddIdentityConfigurations();
// Swagger yapılandırması
builder.Services.AddSwaggerConfigurations();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
