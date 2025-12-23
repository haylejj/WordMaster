using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net;
using WordMaster.Application.Constants;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.AllowedIpAddress;
using WordMaster.Application.Responses.AllowedIpAddress;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Helpers;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class AllowedIpAddressService(IGenericRepository<AllowedIpAddress> repository, IUnitOfWork unitOfWork, ICacheService cacheService, ILogger<AllowedIpAddressService> logger) : IAllowedIpAddressService
{

    public async Task<ServiceResult<List<AllowedIpAddressResponse>>> GetAllAsync()
    {
        List<AllowedIpAddressResponse>? cached = await cacheService.GetAsync<List<AllowedIpAddressResponse>>(CacheKeys.AllowedIpAddressesList);
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

        await cacheService.SetAsync(CacheKeys.AllowedIpAddressesList, viewModels, CacheDurations.Normal);
        return ServiceResult<List<AllowedIpAddressResponse>>.Success(viewModels, HttpStatusCode.OK);
    }
    public async Task<ServiceResult<AllowedIpAddressResponse>> GetByIdAsync(int id)
    {
        AllowedIpAddress? entity = await repository.GetByIdAsync(id);
        if (entity == null)
        {
            logger.LogWarning("Allowed IP address not found with ID: {Id}", id);
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
        // IP adresini normalize et (::ffff: prefix'ini kaldır)
        string normalizedIp = IpAddressHelper.Normalize(request.IpAddress) ?? request.IpAddress;

        // IP adresi zaten var mı kontrol et (normalize edilmiş haliyle)
        List<AllowedIpAddress> allIps = await repository.GetAll().ToListAsync();
        bool exists = allIps.Any(x => IpAddressHelper.Normalize(x.IpAddress) == normalizedIp);

        if (exists)
        {
            logger.LogWarning("Allowed IP address creation failed. IP already exists: {IpAddress}", normalizedIp);
            return ServiceResult.Failure("Bu IP adresi zaten kayıtlı.", HttpStatusCode.Conflict);
        }

        AllowedIpAddress entity = new()
        {
            IpAddress = normalizedIp,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await repository.AddAsync(entity);
        await unitOfWork.CommitAsync();

        await cacheService.RemoveAsync(CacheKeys.AllowedIpAddressesList);
        await cacheService.RemoveAsync(CacheKeys.AllowedIpAddressesActive);

        logger.LogInformation("Allowed IP address created successfully: {IpAddress}", normalizedIp);
        return ServiceResult.SuccessAsCreated();
    }
    public async Task<ServiceResult> UpdateAsync(AllowedIpAddressUpdateRequest request)
    {
        AllowedIpAddress? entity = await repository.GetByIdAsTrackingAsync(request.Id);
        if (entity == null)
        {
            logger.LogWarning("Allowed IP address update failed. ID not found: {Id}", request.Id);
            return ServiceResult.Failure("IP adresi bulunamadı.", HttpStatusCode.NotFound);
        }

        // IP adresini normalize et
        string normalizedRequestIp = IpAddressHelper.Normalize(request.IpAddress) ?? request.IpAddress;
        string currentNormalizedIp = IpAddressHelper.Normalize(entity.IpAddress) ?? entity.IpAddress;

        // IP adresi değiştiyse kontrol et
        if (currentNormalizedIp != normalizedRequestIp)
        {
            // Yeni IP başka bir kayıtta var mı kontrol et
            List<AllowedIpAddress> allIps = await repository.GetAll().ToListAsync();
            bool exists = allIps.Any(x => x.Id != request.Id && IpAddressHelper.Normalize(x.IpAddress) == normalizedRequestIp);

            if (exists)
            {
                logger.LogWarning("Allowed IP address update failed. New IP already exists elsewhere: {IpAddress}", normalizedRequestIp);
                return ServiceResult.Failure("Bu IP adresi başka bir kayıtta zaten mevcut.", HttpStatusCode.Conflict);
            }

            // IP değişti, normalize edilmiş halini kaydet
            entity.IpAddress = normalizedRequestIp;
        }
        // IP değişmediyse, mevcut değeri koru (normalize etme)

        entity.Description = request.Description;
        entity.IsActive = request.IsActive;

        repository.Update(entity);
        await unitOfWork.CommitAsync();

        await cacheService.RemoveAsync(CacheKeys.AllowedIpAddressesList);
        await cacheService.RemoveAsync(CacheKeys.AllowedIpAddressesActive);

        logger.LogInformation("Allowed IP address updated successfully. ID: {Id}", request.Id);
        return ServiceResult.Success(HttpStatusCode.NoContent);
    }
    public async Task<ServiceResult> DeleteAsync(int id)
    {
        AllowedIpAddress? entity = await repository.GetByIdAsTrackingAsync(id);
        if (entity == null)
        {
            logger.LogWarning("Allowed IP address deletion failed. ID not found: {Id}", id);
            return ServiceResult.Failure("IP adresi bulunamadı.", HttpStatusCode.NotFound);
        }

        repository.Remove(entity);
        await unitOfWork.CommitAsync();

        await cacheService.RemoveAsync(CacheKeys.AllowedIpAddressesList);
        await cacheService.RemoveAsync(CacheKeys.AllowedIpAddressesActive);

        logger.LogInformation("Allowed IP address deleted successfully. ID: {Id}", id);
        return ServiceResult.Success(HttpStatusCode.NoContent);
    }
    public async Task<ServiceResult<bool>> IsIpAllowedAsync(string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(ipAddress))
        {
            return ServiceResult<bool>.Success(false, HttpStatusCode.OK);
        }

        // IPv6-mapped IPv4 adreslerini normalize et (::ffff:192.168.1.1 -> 192.168.1.1)
        string? normalizedIp = IpAddressHelper.Normalize(ipAddress);

        // Cache'den aktif IP'leri kontrol et
        List<string>? cachedActiveIps = await cacheService.GetAsync<List<string>>(CacheKeys.AllowedIpAddressesActive);
        if (cachedActiveIps != null)
        {
            bool isAllowed = cachedActiveIps.Any(ip => IpAddressHelper.Normalize(ip) == normalizedIp);
            return ServiceResult<bool>.Success(isAllowed, HttpStatusCode.OK);
        }

        // Cache'de yoksa veritabanından çek
        List<string> activeIps = await repository
            .Where(x => x.IsActive)
            .Select(x => x.IpAddress)
            .ToListAsync();

        // Cache'e kaydet
        await cacheService.SetAsync(CacheKeys.AllowedIpAddressesActive, activeIps, CacheDurations.Normal);

        bool isIpAllowed = activeIps.Any(ip => IpAddressHelper.Normalize(ip) == normalizedIp);
        return ServiceResult<bool>.Success(isIpAllowed, HttpStatusCode.OK);
    }
}
