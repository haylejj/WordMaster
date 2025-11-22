using WordMaster.API.Extensions;
using WordMaster.API.Filters;

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

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
