using Microsoft.AspNetCore.Mvc;
using System.Net;
using WordMaster.API.Extensions;
using WordMaster.Application.Attributes;
using WordMaster.Application.Requests.Practice;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Practice;
using WordMaster.Application.Responses.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

/// <summary>
/// Kelime işlemlerini (Listeleme, Ekleme, Güncelleme, Silme, İçe Aktarma) yöneten controller.
/// </summary>
[Route("api/words")]
public class WordController(IWordService wordService, IExcelService excelService) : BaseController
{
    /// <summary>
    /// Kullanıcının kelimelerini listeler.
    /// </summary>
    /// <param name="search">Aranacak kelime (isteğe bağlı).</param>
    /// <param name="page">Sayfa numarası (varsayılan 1).</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan 10).</param>
    /// <returns>Sayfalanmış kelime listesi.</returns>
    [HttpGet]
    [RequirePermission("Public", "Words", "GetWords", "GET", "Kelimeleri listele")]
    public async Task<IActionResult> GetWords([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        Guid userId = User.GetUserId();
        ServiceResult<PagedResult<WordResponse>> result = await wordService.GetPagedWordsAsync(userId, search, page, pageSize);
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip kelimeyi getirir.
    /// </summary>
    /// <param name="id">Kelime ID'si.</param>
    /// <returns>Kelime detayları.</returns>
    [HttpGet("{id}")]
    [RequirePermission("Public", "Words", "GetWord", "GET", "Kelime detayını getir")]
    public async Task<IActionResult> GetWord(long id)
    {
        Guid userId = User.GetUserId();
        ServiceResult<WordResponse> result = await wordService.GetWordForUserAsync(id, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Yeni bir kelime ekler.
    /// </summary>
    /// <param name="request">Eklenecek kelime bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPost]
    [RequirePermission("Public", "Words", "AddWord", "POST", "Kelime ekle")]
    public async Task<IActionResult> AddWord([FromBody] CreateWordRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult result = await wordService.AddWordAsync(request, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Mevcut bir kelimeyi günceller.
    /// </summary>
    /// <param name="request">Güncellenecek kelime bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPut]
    [RequirePermission("Public", "Words", "UpdateWord", "PUT", "Kelime güncelle")]
    public async Task<IActionResult> UpdateWord([FromBody] UpdateWordRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult result = await wordService.UpdateWordAsync(request.Id, request, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip kelimeyi siler.
    /// </summary>
    /// <param name="id">Silinecek kelime ID'si.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpDelete("{id}")]
    [RequirePermission("Public", "Words", "DeleteWord", "DELETE", "Kelime sil")]
    public async Task<IActionResult> DeleteWord(long id)
    {
        Guid userId = User.GetUserId();

        ServiceResult result = await wordService.DeleteWordAsync(id, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// CSV dosyasından kelimeleri içe aktarır.
    /// </summary>
    /// <param name="file">Yüklenecek CSV dosyası.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPost("import-csv")]
    [RequirePermission("Public", "Words", "ImportCsv", "POST", "CSV içe aktar")]
    public async Task<IActionResult> ImportCsv(IFormFile file)
    {
        Guid userId = User.GetUserId();

        if (file == null || file.Length == 0)
        {
            return CreateResult(ServiceResult.Failure("Dosya seçilmedi.", HttpStatusCode.BadRequest));
        }

        if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return CreateResult(ServiceResult.Failure("Sadece .csv dosyaları kabul edilir.", HttpStatusCode.BadRequest));
        }

        using Stream stream = file.OpenReadStream();
        ServiceResult<WordImportSummaryResponse> result = await excelService.ImportWordsAsync(stream, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Kullanıcının tüm kelimelerini (dropdown için) listeler.
    /// </summary>
    /// <returns>Kelime listesi (ID ve İngilizce karşılık).</returns>
    [HttpGet("user-words")]
    [RequirePermission("Public", "Words", "GetUserWords", "GET", "Kullanıcı kelimelerini listele")]
    public async Task<IActionResult> GetUserWords()
    {
        Guid userId = User.GetUserId();
        ServiceResult<List<WordLookupResponse>> result = await wordService.GetUserWordsAsync(userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Pratik yapmak için rastgele bir kelime getirir.
    /// </summary>
    /// <returns>Kelime detayları (WordResponse).</returns>
    [HttpGet("practice/random")]
    [RequirePermission("Public", "Words", "GetRandomWord", "GET", "Rastgele kelime getir")]
    public async Task<IActionResult> GetRandomWord([FromQuery] int? excludeWordId)
    {
        Guid userId = User.GetUserId();
        ServiceResult<PracticeWordResponse> result = await wordService.GetRandomWordAsync(userId, excludeWordId);
        return CreateResult(result);
    }

    /// <summary>
    /// Pratik sırasında girilen çeviriyi kontrol eder ve istatistikleri günceller.
    /// </summary>
    /// <param name="request">Kontrol edilecek kelime bilgileri.</param>
    /// <returns>Doğru/Yanlış bilgisi.</returns>
    [HttpPost("practice/check")]
    [RequirePermission("Public", "Words", "CheckTranslation", "POST", "Çeviri kontrolü")]
    public async Task<IActionResult> CheckTranslation([FromBody] CheckTranslationRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult<bool> result = await wordService.CheckTranslationAndUpdateAsync(userId, request);
        return CreateResult(result);
    }

    /// <summary>
    /// Pratik sonuçlarını toplu olarak günceller.
    /// </summary>
    /// <param name="request">Sonuç listesi.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPost("practice/batch-update")]
    [RequirePermission("Public", "Words", "BulkUpdateStats", "POST", "Toplu istatistik güncelle")]
    public async Task<IActionResult> BulkUpdateStats([FromBody] BulkUpdateStatsRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult<bool> result = await wordService.BulkUpdateStatsAsync(userId, request);
        return CreateResult(result);
    }
}
