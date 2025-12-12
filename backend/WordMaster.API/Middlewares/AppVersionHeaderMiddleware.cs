using WordMaster.Application.Constants;

namespace WordMaster.API.Middlewares;

/// <summary>
/// Her response'a X-App-Version header'ı ekleyen middleware.
/// </summary>
public class AppVersionHeaderMiddleware(RequestDelegate next)
{
    /// <summary>
    /// Her response'a X-App-Version header'ı ekler.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-App-Version"] = AppInfo.Version;
            return Task.CompletedTask;
        });

        await next(context);
    }
}

