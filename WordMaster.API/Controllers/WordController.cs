using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WordMaster.API.Extensions;
using WordMaster.Application.Dto.Word;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

/// <summary>
/// Kelime işlemlerini (Listeleme, Ekleme, Güncelleme, Silme, İçe Aktarma) yöneten controller.
/// </summary>
[Authorize]
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
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetWords([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        Guid userId = User.GetUserId();
        ServiceResult<PagedResult<WordDto>> result = await wordService.GetPagedWordsAsync(userId, search, page, pageSize);
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip kelimeyi getirir.
    /// </summary>
    /// <param name="id">Kelime ID'si.</param>
    /// <returns>Kelime detayları.</returns>
    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetWord(long id)
    {
        Guid userId = User.GetUserId();
        ServiceResult<WordDto> result = await wordService.GetWordForUserAsync(id, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Yeni bir kelime ekler.
    /// </summary>
    /// <param name="request">Eklenecek kelime bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    [Authorize]
    [HttpPost]
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
    [Authorize]
    [HttpPut]
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
    [Authorize]
    [HttpDelete("{id}")]
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
    [Authorize]
    [HttpPost("import-csv")]
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
        ServiceResult result = await excelService.ImportWordsAsync(stream, userId);
        return CreateResult(result);
    }
}
