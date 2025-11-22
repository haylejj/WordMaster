using WordMaster.API.Extensions;
using WordMaster.API.Filters;
using WordMaster.Domain.Configuration;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// ValidationFilter'ı global olarak ekle (tüm controller'larda model doğrulama hatalarını otomatik yakalar)
builder.Services.AddControllers(configure =>
{
    configure.Filters.Add<ValidationFilter>();
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Fluent Validation yapılandırması
builder.Services.AddValidationConfigurations();

// Configuration Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<MailHogSettings>(builder.Configuration.GetSection("MailHog"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt")); // "Jwt" section name

// JWT Authentication yapılandırması
builder.Services.AddJwtConfigurations(builder.Configuration);

// Application Services (DI)
builder.Services.AddApplicationServices();

// Authorization
builder.Services.AddAuthorization();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
