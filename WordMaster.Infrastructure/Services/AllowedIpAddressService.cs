using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.AllowedIpAddress;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.AllowedIpAddress;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class AllowedIpAddressService(IGenericRepository<AllowedIpAddress> repository, IUnitOfWork unitOfWork, ICacheService cacheService) : IAllowedIpAddressService
{
    private const string AllowedIpAddressesCacheKey = "allowedipaddresses:list";
    private const string AllowedIpAddressesActiveCacheKey = "allowedipaddresses:active";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<List<AllowedIpAddressViewModel>> GetAllAsync()
    {
        List<AllowedIpAddressViewModel>? cached = await cacheService.GetAsync<List<AllowedIpAddressViewModel>>(AllowedIpAddressesCacheKey);
        if (cached != null)
        {
            return cached;
        }

        List<AllowedIpAddress> entities = await repository
            .GetAll()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        var viewModels = entities.Select(x => new AllowedIpAddressViewModel
        {
            Id = x.Id,
            IpAddress = x.IpAddress,
            Description = x.Description,
            CreatedAt = x.CreatedAt,
            IsActive = x.IsActive
        }).ToList();

        await cacheService.SetAsync(AllowedIpAddressesCacheKey, viewModels, CacheExpiration);
        return viewModels;
    }

    public async Task<Result<AllowedIpAddressViewModel>> GetByIdAsync(int id)
    {
        AllowedIpAddress? entity = await repository.GetByIdAsync(id);
        if (entity == null)
        {
            return Result<AllowedIpAddressViewModel>.Failure("IP adresi bulunamadı.");
        }

        var viewModel = new AllowedIpAddressViewModel
        {
            Id = entity.Id,
            IpAddress = entity.IpAddress,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            IsActive = entity.IsActive
        };

        return Result<AllowedIpAddressViewModel>.Success(viewModel);
    }

    public async Task<Result<IEnumerable<string>>> CreateAsync(AllowedIpAddressCreateRequest request)
    {
        List<string> errors = new();

        // IP adresi zaten var mı kontrol et
        AllowedIpAddress? existing = await repository.FirstOrDefaultAsync(x => x.IpAddress == request.IpAddress);

        if (existing != null)
        {
            errors.Add("Bu IP adresi zaten kayıtlı.");
            return new Result<IEnumerable<string>> { IsSuccess = false, ErrorMessage = "IP adresi eklenemedi.", Data = errors };
        }

        AllowedIpAddress entity = new()
        {
            IpAddress = request.IpAddress,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(entity);
        await unitOfWork.CommitAsync();

        await cacheService.RemoveAsync(AllowedIpAddressesCacheKey);
        await cacheService.RemoveAsync(AllowedIpAddressesActiveCacheKey);

        return Result<IEnumerable<string>>.Success(null);
    }

    public async Task<Result<IEnumerable<string>>> UpdateAsync(AllowedIpAddressUpdateRequest request)
    {
        List<string> errors = new();

        AllowedIpAddress? entity = await repository.GetByIdAsTrackingAsync(request.Id);
        if (entity == null)
        {
            errors.Add("IP adresi bulunamadı.");
            return new Result<IEnumerable<string>> { IsSuccess = false, ErrorMessage = "IP adresi güncellenemedi.", Data = errors };
        }

        // IP adresi değiştiyse ve başka bir kayıtta varsa kontrol et
        if (entity.IpAddress != request.IpAddress)
        {
            AllowedIpAddress? existing = await repository
                .FirstOrDefaultAsync(x => x.IpAddress == request.IpAddress && x.Id != request.Id);

            if (existing != null)
            {
                errors.Add("Bu IP adresi başka bir kayıtta zaten mevcut.");
                return new Result<IEnumerable<string>> { IsSuccess = false, ErrorMessage = "IP adresi güncellenemedi.", Data = errors };
            }
        }

        entity.IpAddress = request.IpAddress;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;

        repository.Update(entity);
        await unitOfWork.CommitAsync();

        await cacheService.RemoveAsync(AllowedIpAddressesCacheKey);
        await cacheService.RemoveAsync(AllowedIpAddressesActiveCacheKey);

        return Result<IEnumerable<string>>.Success(null);
    }

    public async Task<Result<IEnumerable<string>>> DeleteAsync(int id)
    {
        List<string> errors = new();

        AllowedIpAddress? entity = await repository.GetByIdAsTrackingAsync(id);
        if (entity == null)
        {
            errors.Add("IP adresi bulunamadı.");
            return new Result<IEnumerable<string>> { IsSuccess = false, ErrorMessage = "IP adresi silinemedi.", Data = errors };
        }

        repository.Remove(entity);
        await unitOfWork.CommitAsync();

        await cacheService.RemoveAsync(AllowedIpAddressesCacheKey);
        await cacheService.RemoveAsync(AllowedIpAddressesActiveCacheKey);

        return Result<IEnumerable<string>>.Success(null);
    }

    public async Task<bool> IsIpAllowedAsync(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return false;
        }

        // Cache'den aktif IP'leri kontrol et
        List<string>? cachedActiveIps = await cacheService.GetAsync<List<string>>(AllowedIpAddressesActiveCacheKey);
        if (cachedActiveIps != null)
        {
            return cachedActiveIps.Contains(ipAddress);
        }

        // Cache'de yoksa veritabanından çek
        List<string> activeIps = await repository
            .Where(x => x.IsActive)
            .Select(x => x.IpAddress)
            .ToListAsync();

        // Cache'e kaydet
        await cacheService.SetAsync(AllowedIpAddressesActiveCacheKey, activeIps, CacheExpiration);

        return activeIps.Contains(ipAddress);
    }
}

