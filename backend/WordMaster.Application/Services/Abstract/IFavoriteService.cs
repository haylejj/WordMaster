using WordMaster.Application.Requests.Favorite;
using WordMaster.Application.Requests.Word;
using WordMaster.Application.Responses.Favorite;
using WordMaster.Application.Responses.Practice;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IFavoriteService
{
    Task<ServiceResult<bool>> ToggleFavoriteAsync(ToggleFavoriteRequest request, Guid userId);
    Task<ServiceResult<PracticeWordResponse>> GetRandomWordFromFavoritesAsync(Guid userId, long? excludeWordId = null);
    Task<ServiceResult<QuizResponse>> GetQuizAsync(Guid userId, long? excludeWordId = null);
    Task<ServiceResult<bool>> CheckTranslationAndUpdateAsync(Guid userId, CheckTranslationRequest request);
    Task<ServiceResult<PagedResult<FavoriteWithWordResponse>>> GetPagedFavoritesAsync(Guid userId, string? search, int page, int pageSize);
}
