using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WordMaster.API.Extensions;
using WordMaster.Application.Attributes;
using WordMaster.Application.Requests.Favorite;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Favorite;
using WordMaster.Application.Responses.Practice;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers;

/// <summary>
/// Favori kelime işlemlerini (Listeleme, Ekleme/Çıkarma) yöneten controller.
/// </summary>
[Route("api/v{version:apiVersion}/favorites")]
[EnableRateLimiting("GeneralPolicy")]
public class FavoriteController(IFavoriteService favoriteService) : BaseController
{
    /// <summary>
    /// Kullanıcının favori kelimelerini listeler.
    /// </summary>
    /// <param name="search">Aranacak kelime (isteğe bağlı).</param>
    /// <param name="page">Sayfa numarası (varsayılan 1).</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan 10).</param>
    /// <returns>Sayfalanmış favori kelime listesi.</returns>
    [HttpGet]
    [RequirePermission("Public", "Favorites", "GetFavorites", "GET", "Favorileri listele")]
    public async Task<IActionResult> GetFavorites([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        Guid userId = User.GetUserId();
        ServiceResult<PagedResult<FavoriteWithWordResponse>> result = await favoriteService.GetPagedFavoritesAsync(userId, search, page, pageSize);
        return CreateResult(result);
    }

    /// <summary>
    /// Bir kelimeyi favorilere ekler veya favorilerden çıkarır.
    /// </summary>
    /// <param name="request">İşlem yapılacak kelime ID'si.</param>
    /// <returns>İşlem sonucu (true: Eklendi, false: Çıkarıldı).</returns>
    [HttpPost("toggle")]
    [RequirePermission("Public", "Favorites", "ToggleFavorite", "POST", "Favori ekle/çıkar")]
    public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult<bool> result = await favoriteService.ToggleFavoriteAsync(request, userId);
        return CreateResult(result);
    }



    /// <summary>
    /// Favorilerden pratik yapmak için rastgele bir kelime getirir.
    /// </summary>
    /// <returns>Kelime detayları (WordResponse).</returns>
    [HttpGet("practice/random")]
    [RequirePermission("Public", "Favorites", "GetRandomWord", "GET", "Rastgele favori kelime getir")]
    public async Task<IActionResult> GetRandomWord([FromQuery] int? excludeWordId)
    {
        Guid userId = User.GetUserId();
        ServiceResult<PracticeWordResponse> result = await favoriteService.GetRandomWordFromFavoritesAsync(userId, excludeWordId);
        return CreateResult(result);
    }

    /// <summary>
    /// Favorilerden test (quiz) sorusu getirir.
    /// </summary>
    /// <returns>Soru ve şıklar.</returns>
    [HttpGet("quiz")]
    [RequirePermission("Public", "Favorites", "GetQuiz", "GET", "Favori test sorusu getir")]
    public async Task<IActionResult> GetQuiz([FromQuery] int? excludeWordId)
    {
        Guid userId = User.GetUserId();
        ServiceResult<QuizResponse> result = await favoriteService.GetQuizAsync(userId, excludeWordId);
        return CreateResult(result);
    }

    /// <summary>
    /// Favoriler pratiği sırasında girilen çeviriyi kontrol eder ve istatistikleri günceller.
    /// </summary>
    /// <param name="request">Kontrol edilecek kelime bilgileri.</param>
    /// <returns>Doğru/Yanlış bilgisi.</returns>
    [HttpPost("practice/check")]
    [RequirePermission("Public", "Favorites", "CheckTranslation", "POST", "Çeviri kontrolü")]
    public async Task<IActionResult> CheckTranslation([FromBody] CheckTranslationRequest request)
    {
        Guid userId = User.GetUserId();
        ServiceResult<bool> result = await favoriteService.CheckTranslationAndUpdateAsync(userId, request);
        return CreateResult(result);
    }
}
