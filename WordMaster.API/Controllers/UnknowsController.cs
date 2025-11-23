using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.API.Extensions;
using WordMaster.Application.Dto.Unknows;
using WordMaster.Application.Requests.Unknows;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

/// <summary>
/// Bilinmeyen kelime işlemlerini (Listeleme, Ekleme/Çıkarma) yöneten controller.
/// </summary>
[Authorize]
[Route("api/unknows")]
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
    public async Task<IActionResult> GetUnknows([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        Guid userId = User.GetUserId();
        ServiceResult<PagedResult<UnknowsWithWordDto>> result = await unknowsService.GetPagedUnknowsAsync(userId, search, page, pageSize);
        return CreateResult(result);
    }

    /// <summary>
    /// Bir kelimeyi bilinmeyenlere ekler veya bilinmeyenlerden çıkarır.
    /// </summary>
    /// <param name="request">İşlem yapılacak kelime ID'si.</param>
    /// <returns>İşlem sonucu (true: Eklendi, false: Çıkarıldı).</returns>
    [HttpPost("toggle")]
    public async Task<IActionResult> ToggleUnknows([FromBody] ToggleUnknowsRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult<bool> result = await unknowsService.ToggleUnknowsAsync(request, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen bilinmeyen kaydını siler.
    /// </summary>
    /// <param name="id">Bilinmeyen ID'si.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUnknows(int id)
    {
        Guid userId = User.GetUserId();
        ServiceResult result = await unknowsService.DeleteUnknowsAsync(id, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Bilinmeyen kelimelerden pratik yapmak için rastgele bir kelime getirir.
    /// </summary>
    /// <returns>İngilizce kelime.</returns>
    [HttpGet("practice/random")]
    public async Task<IActionResult> GetRandomWord()
    {
        Guid userId = User.GetUserId();
        ServiceResult<string> result = await unknowsService.GetRandomWordFromUnknowsAsync(userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Bilinmeyen kelimeler pratiği sırasında girilen çeviriyi kontrol eder ve istatistikleri günceller.
    /// </summary>
    /// <param name="request">Kontrol edilecek kelime bilgileri.</param>
    /// <returns>Doğru/Yanlış bilgisi.</returns>
    [HttpPost("practice/check")]
    public async Task<IActionResult> CheckTranslation([FromBody] CheckTranslationRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult<bool> result = await unknowsService.CheckTranslationAndUpdateAsync(userId, request.TurkishWord, request.EnglishWord);
        return CreateResult(result);
    }
}
