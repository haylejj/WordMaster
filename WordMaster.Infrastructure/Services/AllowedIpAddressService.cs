using Microsoft.EntityFrameworkCore;
using System.Net;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.AllowedIpAddress;
using WordMaster.Application.Responses.AllowedIpAddress;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class AllowedIpAddressService(IGenericRepository<AllowedIpAddress> repository, IUnitOfWork unitOfWork, ICacheService cacheService) : IAllowedIpAddressService
{
    private const string AllowedIpAddressesCacheKey = "allowedipaddresses:list";
    private const string AllowedIpAddressesActiveCacheKey = "allowedipaddresses:active";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<ServiceResult<List<AllowedIpAddressResponse>>> GetAllAsync()
    {
        List<AllowedIpAddressResponse>? cached = await cacheService.GetAsync<List<AllowedIpAddressResponse>>(AllowedIpAddressesCacheKey);
        if (cached != null)
        {
            return ServiceResult<List<AllowedIpAddressResponse>>.Success(cached, HttpStatusCode.OK);
        }

        List<AllowedIpAddress> entities = await repository
            .GetAll()
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        List<AllowedIpAddressResponse> viewModels = entities.Select(x => new AllowedIpAddressResponse
        {
            Id = x.Id,
            IpAddress = x.IpAddress,
            Description = x.Description,
            CreatedAt = x.CreatedAt,
            IsActive = x.IsActive
        }).ToList();

        await cacheService.SetAsync(AllowedIpAddressesCacheKey, viewModels, CacheExpiration);
        return ServiceResult<List<AllowedIpAddressResponse>>.Success(viewModels, HttpStatusCode.OK);
    }

    public async Task<ServiceResult<AllowedIpAddressResponse>> GetByIdAsync(int id)
    {
        AllowedIpAddress? entity = await repository.GetByIdAsync(id);
        if (entity == null)
        {
            return ServiceResult<AllowedIpAddressResponse>.Failure("IP adresi bulunamadı.", HttpStatusCode.NotFound);
        }

        AllowedIpAddressResponse viewModel = new()
        {
            Id = entity.Id,
            IpAddress = entity.IpAddress,
            Description = entity.Description,
            CreatedAt = entity.CreatedAt,
            IsActive = entity.IsActive
        };

        return ServiceResult<AllowedIpAddressResponse>.Success(viewModel, HttpStatusCode.OK);
    }

    public async Task<ServiceResult> CreateAsync(AllowedIpAddressCreateRequest request)
    {
        // IP adresi zaten var mı kontrol et
        AllowedIpAddress? existing = await repository.FirstOrDefaultAsync(x => x.IpAddress == request.IpAddress);

        if (existing != null)
        {
            return ServiceResult.Failure("Bu IP adresi zaten kayıtlı.", HttpStatusCode.Conflict);
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

        return ServiceResult.SuccessAsCreated();
    }

    public async Task<ServiceResult> UpdateAsync(AllowedIpAddressUpdateRequest request)
    {
        AllowedIpAddress? entity = await repository.GetByIdAsTrackingAsync(request.Id);
        if (entity == null)
        {
            return ServiceResult.Failure("IP adresi bulunamadı.", HttpStatusCode.NotFound);
        }

        // IP adresi değiştiyse ve başka bir kayıtta varsa kontrol et
        if (entity.IpAddress != request.IpAddress)
        {
            AllowedIpAddress? existing = await repository
                .FirstOrDefaultAsync(x => x.IpAddress == request.IpAddress && x.Id != request.Id);

            if (existing != null)
            {
                return ServiceResult.Failure("Bu IP adresi başka bir kayıtta zaten mevcut.", HttpStatusCode.Conflict);
            }
        }

        entity.IpAddress = request.IpAddress;
        entity.Description = request.Description;
        entity.IsActive = request.IsActive;

        repository.Update(entity);
        await unitOfWork.CommitAsync();

        await cacheService.RemoveAsync(AllowedIpAddressesCacheKey);
        await cacheService.RemoveAsync(AllowedIpAddressesActiveCacheKey);

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    public async Task<ServiceResult> DeleteAsync(int id)
    {
        AllowedIpAddress? entity = await repository.GetByIdAsTrackingAsync(id);
        if (entity == null)
        {
            return ServiceResult.Failure("IP adresi bulunamadı.", HttpStatusCode.NotFound);
        }

        repository.Remove(entity);
        await unitOfWork.CommitAsync();

        await cacheService.RemoveAsync(AllowedIpAddressesCacheKey);
        await cacheService.RemoveAsync(AllowedIpAddressesActiveCacheKey);

        return ServiceResult.Success(HttpStatusCode.NoContent);
    }

    public async Task<ServiceResult<bool>> IsIpAllowedAsync(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return ServiceResult<bool>.Success(false, HttpStatusCode.OK);
        }

        // Cache'den aktif IP'leri kontrol et
        List<string>? cachedActiveIps = await cacheService.GetAsync<List<string>>(AllowedIpAddressesActiveCacheKey);
        if (cachedActiveIps != null)
        {
            return ServiceResult<bool>.Success(cachedActiveIps.Contains(ipAddress), HttpStatusCode.OK);
        }

        // Cache'de yoksa veritabanından çek
        List<string> activeIps = await repository
            .Where(x => x.IsActive)
            .Select(x => x.IpAddress)
            .ToListAsync();

        // Cache'e kaydet
        await cacheService.SetAsync(AllowedIpAddressesActiveCacheKey, activeIps, CacheExpiration);

        return ServiceResult<bool>.Success(activeIps.Contains(ipAddress), HttpStatusCode.OK);
    }
}
