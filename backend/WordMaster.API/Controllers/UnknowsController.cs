using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WordMaster.API.Extensions;
using WordMaster.Application.Attributes;
using WordMaster.Application.Requests.Unknows;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Practice;
using WordMaster.Application.Responses.Unknows;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

/// <summary>
/// Bilinmeyen kelime işlemlerini (Listeleme, Ekleme/Çıkarma) yöneten controller.
/// </summary>
[Route("api/v{version:apiVersion}/unknows")]
[EnableRateLimiting("GeneralPolicy")]
public class UnknowsController(IUnknowsService unknowsService) : BaseController
{
    /// <summary>
    /// Kullanıcının bilinmeyen kelimelerini listeler.
    /// </summary>
    /// <param name="search">Aranacak kelime (isteğe bağlı).</param>
    /// <param name="page">Sayfa numarası (varsayılan 1).</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan 10).</param>
    /// <returns>Sayfalanmış bilinmeyen kelime listesi.</returns>
    [HttpGet]
    [RequirePermission("Public", "Unknowns", "GetUnknows", "GET", "Bilinmeyen kelimeleri listele")]
    public async Task<IActionResult> GetUnknows([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        Guid userId = User.GetUserId();
        ServiceResult<PagedResult<UnknowsWithWordResponse>> result = await unknowsService.GetPagedUnknowsAsync(userId, search, page, pageSize);
        return CreateResult(result);
    }

    /// <summary>
    /// Bir kelimeyi bilinmeyenlere ekler veya bilinmeyenlerden çıkarır.
    /// </summary>
    /// <param name="request">İşlem yapılacak kelime ID'si.</param>
    /// <returns>İşlem sonucu (true: Eklendi, false: Çıkarıldı).</returns>
    [HttpPost("toggle")]
    [RequirePermission("Public", "Unknowns", "ToggleUnknows", "POST", "Bilinmeyen kelime ekle/çıkar")]
    public async Task<IActionResult> ToggleUnknows([FromBody] ToggleUnknowsRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult<bool> result = await unknowsService.ToggleUnknowsAsync(request, userId);
        return CreateResult(result);
    }
    /// <summary>
    /// Bilinmeyen kelimelerden pratik yapmak için rastgele bir kelime getirir.
    /// </summary>
    /// <returns>Kelime detayları (WordResponse).</returns>
    [HttpGet("practice/random")]
    [RequirePermission("Public", "Unknowns", "GetRandomWord", "GET", "Rastgele bilinmeyen kelime getir")]
    public async Task<IActionResult> GetRandomWord([FromQuery] int? excludeWordId)
    {
        Guid userId = User.GetUserId();
        ServiceResult<PracticeWordResponse> result = await unknowsService.GetRandomWordFromUnknowsAsync(userId, excludeWordId);
        return CreateResult(result);
    }

    /// <summary>
    /// Bilinmeyen kelimeler pratiği sırasında girilen çeviriyi kontrol eder ve istatistikleri günceller.
    /// </summary>
    /// <param name="request">Kontrol edilecek kelime bilgileri.</param>
    /// <returns>Doğru/Yanlış bilgisi.</returns>
    [HttpPost("practice/check")]
    [RequirePermission("Public", "Unknowns", "CheckTranslation", "POST", "Çeviri kontrolü")]
    public async Task<IActionResult> CheckTranslation([FromBody] CheckTranslationRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult<bool> result = await unknowsService.CheckTranslationAndUpdateAsync(userId, request);
        return CreateResult(result);
    }
}
