using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.API.Extensions;
using WordMaster.Application.Requests.Folder;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Folder;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

/// <summary>
/// Klasör işlemlerini (Listeleme, Ekleme, Güncelleme, Silme, Kelime Ekleme/Çıkarma) yöneten controller.
/// </summary>
[Authorize]
[Route("api/folders")]
public class FolderController(IFolderService folderService) : BaseController
{
    /// <summary>
    /// Kullanıcının klasörlerini listeler.
    /// </summary>
    /// <returns>Klasör listesi.</returns>
    [HttpGet]
    public async Task<IActionResult> GetFolders()
    {
        Guid userId = User.GetUserId();
        ServiceResult<List<FolderResponse>> result = await folderService.GetUserFoldersAsync(userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen ID'ye sahip klasörü getirir.
    /// </summary>
    /// <param name="id">Klasör ID'si.</param>
    /// <returns>Klasör detayları.</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetFolder(long id)
    {
        Guid userId = User.GetUserId();
        ServiceResult<FolderResponse> result = await folderService.GetUserFolderAsync(id, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Yeni bir klasör oluşturur.
    /// </summary>
    /// <param name="request">Oluşturulacak klasör bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPost]
    public async Task<IActionResult> AddFolder([FromBody] CreateFolderRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult result = await folderService.AddFolderAsync(request, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Mevcut bir klasörü günceller.
    /// </summary>
    /// <param name="request">Güncellenecek klasör bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPut]
    public async Task<IActionResult> UpdateFolder([FromBody] UpdateFolderRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult result = await folderService.UpdateFolderAsync(request.Id, request, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Belirtilen klasörü siler.
    /// </summary>
    /// <param name="id">Silinecek klasör ID'si.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFolder(long id)
    {
        Guid userId = User.GetUserId();
        ServiceResult result = await folderService.DeleteFolderAsync(id, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Bir klasördeki kelimeleri listeler.
    /// </summary>
    /// <param name="id">Klasör ID'si.</param>
    /// <returns>Klasördeki kelimelerin listesi.</returns>
    [HttpGet("{id}/words")]
    public async Task<IActionResult> GetWordsInFolder(long id)
    {
        Guid userId = User.GetUserId();
        ServiceResult<List<FolderWordResponse>> result = await folderService.GetWordsInFolderAsync(id, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Bir klasöre kelime ekler.
    /// </summary>
    /// <param name="request">Eklenecek kelime ve klasör bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpPost("words")]
    public async Task<IActionResult> AddWordToFolder([FromBody] AddWordToFolderRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult result = await folderService.AddWordToFolderAsync(request, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Bir klasörden kelime çıkarır.
    /// </summary>
    /// <param name="request">Çıkarılacak kelime ve klasör bilgileri.</param>
    /// <returns>İşlem sonucu.</returns>
    [HttpDelete("words")]
    public async Task<IActionResult> RemoveWordFromFolder([FromBody] AddWordToFolderRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult result = await folderService.RemoveWordFromFolderAsync(request, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Klasör pratiği sırasında girilen çeviriyi kontrol eder ve istatistikleri günceller.
    /// </summary>
    /// <param name="request">Kontrol edilecek kelime bilgileri.</param>
    /// <returns>Doğru/Yanlış bilgisi.</returns>
    [HttpPost("practice/check")]
    public async Task<IActionResult> CheckTranslation([FromBody] CheckTranslationRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult<bool> result = await folderService.CheckTranslationAndUpdateAsync(userId, request);
        return CreateResult(result);
    }
}
