using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using WordMaster.API.Extensions;
using WordMaster.API.Filters;
using WordMaster.API.Middlewares;
using WordMaster.Infrastructure.BackgroundServices;
using WordMaster.Infrastructure.EfCore;
using WordMaster.Infrastructure.Extensions;

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

// Rate Limiting
builder.Services.AddRateLimitingConfigurations();

// Background Jobs
builder.Services.AddHostedService<UserCleanupService>();

// CORS Politikası
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "https://localhost:5173") // Frontend URL
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

WebApplication app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigins");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.LogStartupStatusAsync();

app.Run();
