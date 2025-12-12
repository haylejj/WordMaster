using System.Net;
using System.Text.Json;
using WordMaster.Domain.Results;

namespace WordMaster.API.Middlewares;

/// <summary>
/// Uygulama genelinde oluşan ve yakalanmayan hataları ele alan ara katman (middleware).
/// </summary>
/// <remarks>
/// GlobalExceptionHandlerMiddleware sınıfının yapıcı metodu.
/// </remarks>
/// <param name="next">Bir sonraki middleware delegesi.</param>
/// <param name="logger">Loglama servisi.</param>
public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
{

    /// <summary>
    /// HTTP isteğini işler ve hata oluşursa yakalar.
    /// </summary>
    /// <param name="context">HTTP bağlamı.</param>
    /// <returns>Asenkron işlem görevi.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            // Hatanın nereden geldiğini, mesajını ve stack trace'ini detaylıca logluyoruz.
            logger.LogError(ex, "Beklenmeyen bir hata oluştu! Mesaj: {Message}, Kaynak: {Source}, StackTrace: {StackTrace}", ex.Message, ex.Source, ex.StackTrace);

            // Eğer yanıt zaten istemciye gönderilmeye başlandıysa (headers sent), 
            // durum kodunu değiştiremeyiz ve JSON yazamayız.
            // Not: Exception zaten yukarıda LogError ile loglandı.
            // throw yapmak bağlantıyı koparır, bu yüzden sadece ek bilgi loglayıp graceful return yapıyoruz.
            if (context.Response.HasStarted)
            {
                logger.LogError(
                    ex,
                    "Response zaten başladığı için özel hata yanıtı döndürülemiyor. " +
                    "Request: {Method} {Path}",
                    context.Request.Method,
                    context.Request.Path);
                return;
            }

            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Yakalanan hatayı işleyerek istemciye uygun formatta yanıt döner.
    /// </summary>
    /// <param name="context">HTTP bağlamı.</param>
    /// <param name="exception">Oluşan hata.</param>
    /// <returns>Asenkron işlem görevi.</returns>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Hata mesajını ServiceResult yapısı ile döndürüyoruz.
        ServiceResult serviceResult = ServiceResult.Failure("Beklenmeyen bir hata oluştu!", HttpStatusCode.InternalServerError);

        string json = JsonSerializer.Serialize(serviceResult, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        return context.Response.WriteAsync(json);
    }
}