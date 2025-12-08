using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WordMaster.API.Extensions;
using WordMaster.Application.Attributes;
using WordMaster.Application.Requests.Folder;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Folder;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

/// <summary>
/// Klasör işlemlerini (Listeleme, Ekleme, Güncelleme, Silme, Kelime Ekleme/Çıkarma) yöneten controller.
/// </summary>
[Route("api/folders")]
[EnableRateLimiting("GeneralPolicy")]
public class FolderController(IFolderService folderService) : BaseController
{
    /// <summary>
    /// Kullanıcının klasörlerini listeler.
    /// </summary>
    /// <returns>Klasör listesi.</returns>
    [HttpGet]
    [RequirePermission("Public", "Folders", "GetFolders", "GET", "Klasörleri listele")]
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
    [RequirePermission("Public", "Folders", "GetFolder", "GET", "Klasör detayını getir")]
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
    [RequirePermission("Public", "Folders", "AddFolder", "POST", "Klasör ekle")]
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
    [RequirePermission("Public", "Folders", "UpdateFolder", "PUT", "Klasör güncelle")]
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
    [RequirePermission("Public", "Folders", "DeleteFolder", "DELETE", "Klasör sil")]
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
    [RequirePermission("Public", "Folders", "GetWordsInFolder", "GET", "Klasördeki kelimeleri listele")]
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
    [RequirePermission("Public", "Folders", "AddWordToFolder", "POST", "Klasöre kelime ekle")]
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
    [RequirePermission("Public", "Folders", "RemoveWordFromFolder", "DELETE", "Klasörden kelime çıkar")]
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
    [RequirePermission("Public", "Folders", "CheckTranslation", "POST", "Çeviri kontrolü")]
    public async Task<IActionResult> CheckTranslation([FromBody] CheckTranslationRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult<bool> result = await folderService.CheckTranslationAndUpdateAsync(userId, request);
        return CreateResult(result);
    }
}
