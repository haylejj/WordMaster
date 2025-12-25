using Asp.Versioning.ApiExplorer;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Text.Json;
using System.Text.Json.Serialization;
using WordMaster.API.Extensions;
using WordMaster.API.Filters;
using WordMaster.API.Middlewares;
using WordMaster.Infrastructure.BackgroundServices;
using WordMaster.Infrastructure.EfCore;
using WordMaster.Infrastructure.Extensions;

// .env dosyasını yükle
// Docker container'da /app/.env olarak mount edilir
// clobberExistingVars: false -> Docker-compose'un set ettiği doğru değerlerin (örn. redis:6379) 
// .env dosyasındaki local değerlerle (örn. localhost:6379) ezilmesini engeller.
// Normalde docker-compose da zaten environment variable'lar set edilmiş.Ama biz localden calıstırırken 
// .env dosyasını okuyarak environment variable'ları set ediyoruz.O yüzden bu şekilde yapıyoruz.Hem docker hem localde
// .env kullanabilmek için docker-compose da .env yi volume olarak tasımak lazım eğer aşşağıdaki gibi fiziksel dosyadan okumak istiyorsak.
// Normalde dockerda gerek yok ama localde calıstırmak için fiziksel dosyadan .env yi okumak lazım.
LoadOptions loadOptions = new(setEnvVars: true, clobberExistingVars: false);
string envPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".env");
if (File.Exists(envPath))
{
    Env.Load(envPath, loadOptions);
    Console.WriteLine($"[Startup] .env dosyası yüklendi: {envPath}");
}
else
{
    // Proje kök dizininde de ara (docker-compose dizini)
    string rootEnvPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
    if (File.Exists(rootEnvPath))
    {
        Env.Load(rootEnvPath, loadOptions);
        Console.WriteLine($"[Startup] .env dosyası yüklendi: {rootEnvPath}");
    }
    else
    {
        throw new Exception(".env dosyası bulunamadı!");
    }
}

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();
// Add services to the container.

// ValidationFilter'ı global olarak ekle (tüm controller'larda model doğrulama hatalarını otomatik yakalar)
builder.Services.AddControllers(configure =>
{
    configure.Filters.Add<ValidationFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
// Serilog yapılandırması
builder.Host.AddSerilogConfigurations();

// Fluent Validation yapılandırması
builder.Services.AddValidationConfigurations();

// OpenTelemetry metrics yapılandırması
builder.Services.AddOpenTelemetryMetrics(builder.Configuration);

// HttpContextAccessor (LoginService'te IP adresi almak için gerekli)
builder.Services.AddHttpContextAccessor();

// Configuration Settings
builder.Services.AddConfigurationSettings(builder.Configuration);

// JWT Authentication yapılandırması
builder.Services.AddJwtConfigurations(builder.Configuration);

// Application Services (DI)
builder.Services.AddApplicationServices();
// Redis Cache yapılandırması
builder.Services.AddRedis(builder.Configuration);
// Authorization
builder.Services.AddAuthorization();
// Forwarded Headers yapılandırması
builder.Services.AddForwardedHeadersConfigurations();
// DbContext yapılandırması
builder.Services.AddDbContextConfigurations(builder.Configuration);
// Identity yapılandırması
builder.Services.AddIdentityConfigurations();
// API Versioning yapılandırması
builder.Services.AddApiVersioningConfigurations();
// Swagger yapılandırması
builder.Services.AddSwaggerConfigurations();

// Rate Limiting
builder.Services.AddRateLimitingConfigurations();

// Background Jobs
builder.Services.AddHostedService<UserCleanupService>();

// CORS Politikası
builder.Services.AddCorsConfigurations();

WebApplication app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    // Her API versiyonu için Swagger UI endpoint'i oluştur
    IApiVersionDescriptionProvider apiVersionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    app.UseSwaggerUI(options =>
    {
        foreach (ApiVersionDescription description in apiVersionProvider.ApiVersionDescriptions)
        {
            string url = $"/swagger/{description.GroupName}/swagger.json";
            string name = description.GroupName.ToUpperInvariant();

            // Deprecated versiyonlar için etiket ekle
            if (description.IsDeprecated)
            {
                name += " (Deprecated)";
            }

            options.SwaggerEndpoint(url, name);
        }
    });
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseMiddleware<AppVersionHeaderMiddleware>();

// Production'da HTTPS'e yönlendir, development'ta HTTP kullan
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowSpecificOrigins");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Otomatik Migration
using (IServiceScope scope = app.Services.CreateScope())
{
    IServiceProvider services = scope.ServiceProvider;
    try
    {
        AppDbContext context = services.GetRequiredService<AppDbContext>();
        // Eğer veritabanı yoksa oluşturur, varsa eksik migration'ları uygular
        await context.Database.MigrateAsync();
        Log.Information("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while applying database migrations.");
    }
}

await app.LogStartupStatusAsync();

app.Run();
