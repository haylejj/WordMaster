using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WordMaster.Application.Responses.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Configuration;
using WordMaster.Domain.Results;
using WordMaster.Infrastructure.Helpers;

namespace WordMaster.Infrastructure.Services;

/// <summary>
/// JWT token oluşturma ve yönetme işlemlerini gerçekleştiren servis.
/// </summary>
public class JwtService(
    IOptions<JwtSettings> jwtSettings,
    IUserService userService,
    IHttpContextAccessor httpContextAccessor,
    IRefreshTokenCookieHelper cookieHelper) : IJwtService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    /// <summary>
    /// Kullanıcı için JWT access token oluşturur.
    /// Token içinde kullanıcı ID, email, roller ve security stamp claim olarak eklenir.
    /// </summary>
    public ServiceResult<string> GenerateAccessToken(string userId, string userName, string email, IList<string> roles, string securityStamp)
    {
        try
        {
            // Token'a eklenecek claim'leri (kullanıcı bilgileri) oluştur
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, userId),  // Kullanıcı ID
                new Claim(ClaimTypes.Name, userName),          // Kullanıcı Adı
                new Claim(ClaimTypes.Email, email),            // Email
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Token ID (her token için benzersiz)
                new Claim("SecurityStamp", securityStamp),     // Security Stamp (şifre değişince token geçersiz olur)
            };

            // Kullanıcının her bir rolü için ayrı claim ekle
            foreach (string role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Token imzalama için kullanılacak güvenlik anahtarını oluştur
            SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

            // JWT token'ı oluştur
            JwtSecurityToken token = new(
                issuer: _jwtSettings.Issuer,                                    // Token'ı oluşturan (örn: "WordMasterAPI")
                audience: _jwtSettings.Audience,                                // Token'ın hedef kitlesi (örn: "WordMasterClient")
                claims: claims,                                                 // Kullanıcı bilgileri
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes), // Token geçerlilik süresi
                signingCredentials: credentials                                 // İmzalama bilgileri
            );

            // Token'ı string formatına çevir ve döndür
            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return ServiceResult<string>.Success(tokenString, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Failure($"Access token oluşturulurken hata: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Kriptografik olarak güvenli bir refresh token oluşturur.
    /// Bu token veritabanında saklanır ve access token yenilemek için kullanılır.
    /// </summary>
    public ServiceResult<string> GenerateRefreshToken()
    {

        // 64 byte'lık rastgele bir dizi oluştur
        byte[] randomNumber = new byte[64];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);

        // Byte dizisini Base64 string'e çevir
        string refreshToken = Convert.ToBase64String(randomNumber);
        return ServiceResult<string>.Success(refreshToken, HttpStatusCode.OK);

    }

    /// <summary>
    /// Süresi dolmuş bir access token'dan claim'leri çıkarır.
    /// Refresh token işleminde kullanıcı bilgilerini almak için kullanılır.
    /// NOT: Bu metod token'ın geçerlilik süresini kontrol ETMEZ.
    /// </summary>
    public ServiceResult<ClaimsPrincipal> GetPrincipalFromExpiredToken(string accessToken)
    {
        try
        {
            // Token doğrulama parametrelerini ayarla
            TokenValidationParameters tokenValidationParameters = new()
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,

                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key)),

                // ÖNEMLİ: Geçerlilik süresini kontrol etme, çünkü süresi dolmuş token'ları da kabul etmemiz gerekiyor
                ValidateLifetime = false
            };

            JwtSecurityTokenHandler tokenHandler = new();

            // Token'ı doğrula ve claim'leri çıkar
            ClaimsPrincipal principal = tokenHandler.ValidateToken(
                accessToken,
                tokenValidationParameters,
                out SecurityToken securityToken);

            // Token'ın gerçekten JWT olduğunu ve doğru algoritma ile imzalandığını kontrol et
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return ServiceResult<ClaimsPrincipal>.Failure("Geçersiz token formatı", HttpStatusCode.BadRequest);
            }

            return ServiceResult<ClaimsPrincipal>.Success(principal, HttpStatusCode.OK);
        }
        catch (SecurityTokenMalformedException)
        {
            return ServiceResult<ClaimsPrincipal>.Failure("Token formatı hatalı", HttpStatusCode.BadRequest);
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return ServiceResult<ClaimsPrincipal>.Failure("Token imzası geçersiz", HttpStatusCode.Unauthorized);
        }
        catch (SecurityTokenException)
        {
            return ServiceResult<ClaimsPrincipal>.Failure("Token doğrulanamadı", HttpStatusCode.Unauthorized);
        }
    }

    /// <summary>
    /// Refresh token kullanarak yeni bir access token oluşturur.
    /// Veritabanındaki refresh token'ı doğrular ve yeni token çifti döndürür.
    /// NOT: Sayfa yenileme durumunda boş access token gönderilebilir.
    /// </summary>
    public async Task<ServiceResult<RefreshTokenInternalResponse>> RefreshAccessTokenAsync(string expiredAccessToken, string refreshToken)
    {
        string? userId = null;

        // Access token boş değilse, ondan kullanıcı ID'sini çıkarmaya çalış
        if (!string.IsNullOrWhiteSpace(expiredAccessToken))
        {
            ServiceResult<ClaimsPrincipal> principalResult = GetPrincipalFromExpiredToken(expiredAccessToken);
            if (principalResult.IsSuccess && principalResult.Data != null)
            {
                userId = principalResult.Data.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            }
        }

        // Eğer access token'dan userId alınamadıysa, refresh token ile kullanıcıyı bul
        if (string.IsNullOrEmpty(userId))
        {
            var userByRefreshTokenResult = await userService.FindUserByRefreshTokenAsync(refreshToken);
            if (!userByRefreshTokenResult.IsSuccess || userByRefreshTokenResult.Data == null)
            {
                return ServiceResult<RefreshTokenInternalResponse>.Failure(
                    "Oturum doğrulanamadı. Lütfen tekrar giriş yapın.",
                    HttpStatusCode.Unauthorized
                );
            }
            userId = userByRefreshTokenResult.Data.Id;
        }

        // 2. UserService ile refresh token'ı doğrula ve kullanıcı bilgilerini al
        // Bu metod hash'li karşılaştırma yapıyor
        var userValidationResult = await userService.ValidateAndGetUserByRefreshTokenAsync(userId, refreshToken);
        if (!userValidationResult.IsSuccess || userValidationResult.Data == null)
        {
            return ServiceResult<RefreshTokenInternalResponse>.Failure(
                userValidationResult.ErrorList?.FirstOrDefault() ?? "Refresh token doğrulanamadı",
                userValidationResult.StatusCode
            );
        }

        var userWithRoles = userValidationResult.Data;

        // 3. Yeni access token oluştur (SecurityStamp ile)
        ServiceResult<string> accessTokenResult = GenerateAccessToken(
            userWithRoles.Id,
            userWithRoles.UserName,
            userWithRoles.Email,
            userWithRoles.Roles,
            userWithRoles.SecurityStamp ?? string.Empty
        );

        if (!accessTokenResult.IsSuccess || accessTokenResult.Data == null)
        {
            return ServiceResult<RefreshTokenInternalResponse>.Failure("Yeni access token oluşturulamadı", HttpStatusCode.InternalServerError);
        }

        // 4. Yeni refresh token oluştur
        ServiceResult<string> newRefreshTokenResult = GenerateRefreshToken();
        if (!newRefreshTokenResult.IsSuccess || newRefreshTokenResult.Data == null)
        {
            return ServiceResult<RefreshTokenInternalResponse>.Failure("Yeni refresh token oluşturulamadı", HttpStatusCode.InternalServerError);
        }

        // 5. Yeni refresh token'ı veritabanına kaydet (hash'lenerek kaydedilecek)
        ServiceResult updateResult = await userService.UpdateRefreshTokenAsync(
            userId,
            newRefreshTokenResult.Data,
            _jwtSettings.RefreshTokenExpiresInDays
        );

        if (!updateResult.IsSuccess)
        {
            return ServiceResult<RefreshTokenInternalResponse>.Failure("Refresh token güncellenemedi", HttpStatusCode.InternalServerError);
        }

        // 6. Yeni token çiftini döndür (plain refresh token Controller'a, o cookie olarak set edecek)
        RefreshTokenInternalResponse response = new()
        {
            AccessToken = accessTokenResult.Data,
            RefreshToken = newRefreshTokenResult.Data,  // Plain text, cookie için
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes)
        };

        return ServiceResult<RefreshTokenInternalResponse>.Success(response, HttpStatusCode.OK);
    }

    /// <summary>
    /// Cookie'den refresh token okuyarak yeni bir access token oluşturur.
    /// Cookie okuma ve yazma işlemlerini servis içinde yapar.
    /// </summary>
    public async Task<ServiceResult<RefreshTokenResponse>> RefreshAccessTokenWithCookieAsync(string expiredAccessToken)
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return ServiceResult<RefreshTokenResponse>.Failure("HTTP context bulunamadı", HttpStatusCode.InternalServerError);
        }

        // Cookie'den refresh token oku
        string? refreshToken = cookieHelper.GetRefreshTokenFromCookie(httpContext);
        if (string.IsNullOrEmpty(refreshToken))
        {
            return ServiceResult<RefreshTokenResponse>.Failure("Refresh token bulunamadı. Lütfen tekrar giriş yapın.", HttpStatusCode.Unauthorized);
        }

        // Mevcut RefreshAccessTokenAsync metodunu çağır
        ServiceResult<RefreshTokenInternalResponse> result = await RefreshAccessTokenAsync(expiredAccessToken, refreshToken);

        if (!result.IsSuccess || result.Data == null)
        {
            return ServiceResult<RefreshTokenResponse>.Failure(result.ErrorList ?? [], result.StatusCode);
        }

        // Yeni refresh token'ı cookie'ye yaz
        cookieHelper.SetRefreshTokenCookie(httpContext, result.Data.RefreshToken);

        // Sadece access token ve expiry döndür
        RefreshTokenResponse response = new()
        {
            AccessToken = result.Data.AccessToken,
            ExpiresAt = result.Data.ExpiresAt
        };

        return ServiceResult<RefreshTokenResponse>.Success(response, HttpStatusCode.OK);
    }
}
