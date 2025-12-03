using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using WordMaster.Domain.Results;

namespace WordMaster.API.Filters;

/// <summary>
/// Model doğrulama hatalarını otomatik olarak yakalayan action filter.
/// Controller action'ları çalışmadan önce ModelState'i kontrol eder ve geçersizse
/// standart ServiceResult formatında HTTP 400 hatası döndürür.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    /// <summary>
    /// Action çalıştırılmadan önce devreye girer ve model doğrulamasını kontrol eder.
    /// </summary>
    /// <param name="context">Action çalıştırma bağlamı ve istek verileri.</param>
    /// <param name="next">Zincirdeki bir sonraki adımı temsil eden delege.</param>
    /// <returns>Asenkron işlem görevi.</returns>
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.ModelState.IsValid)
        {
            List<string> errors = context.ModelState
                                .Values
                                .SelectMany(v => v.Errors)
                                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Geçersiz değer." : e.ErrorMessage)
                                .ToList();

            ServiceResult serviceResult = ServiceResult.Failure(errors, HttpStatusCode.BadRequest);

            context.Result = new ObjectResult(serviceResult)
            {
                StatusCode = (int)serviceResult.StatusCode
            };

            return;
        }
        await next();
    }
}
