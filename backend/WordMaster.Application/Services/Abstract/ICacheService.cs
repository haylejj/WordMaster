namespace WordMaster.Application.Services.Abstract;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<bool> RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task ClearAsync();
}

