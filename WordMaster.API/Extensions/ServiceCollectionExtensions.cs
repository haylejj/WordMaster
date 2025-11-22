using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Validation.Word;

namespace WordMaster.API.Extensions;

/// <summary>
/// IServiceCollection için extension metodları.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Fluent Validation ve custom ValidationFilter yapılandırmasını ekler.
    /// </summary>
    public static IServiceCollection AddValidationConfigurations(this IServiceCollection services)
    {
        // Fluent Validation validator'larını assembly'den otomatik olarak kaydet
        services.AddValidatorsFromAssemblyContaining<WordDtoValidator>();

        // Fluent Validation'ı otomatik validation için etkinleştir
        services.AddFluentValidationAutoValidation(o =>
        {
            o.DisableDataAnnotationsValidation = true;
        });

        // ApiController'ın ModelState hatalarında otomatik olarak 400 BadRequest + ProblemDetails döndürme davranışını devre dışı bırak
        // Bu sayede ModelState geçersiz olduğunda default pipeline çalışmaz, custom ValidationFilter devreye girer
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        return services;
    }
}
