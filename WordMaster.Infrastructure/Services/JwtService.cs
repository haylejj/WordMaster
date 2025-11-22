using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using WordMaster.Application.Responses;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Configuration;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

/// <summary>
/// JWT token oluşturma ve yönetme işlemlerini gerçekleştiren servis.
/// </summary>
public class JwtService(IOptions<JwtSettings> jwtSettings, IUserService userService) : IJwtService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    /// <summary>
    /// Kullanıcı için JWT access token oluşturur.
    /// Token içinde kullanıcı ID, email ve roller claim olarak eklenir.
    /// </summary>
    public ServiceResult<string> GenerateAccessToken(string userId, string email, IList<string> roles)
    {
        try
        {
            // Token'a eklenecek claim'leri (kullanıcı bilgileri) oluştur
            List<Claim> claims = new()
            {
                new Claim(ClaimTypes.NameIdentifier, userId),  // Kullanıcı ID
                new Claim(ClaimTypes.Email, email),            // Email
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // Token ID (her token için benzersiz)
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
        try
        {
            // 64 byte'lık rastgele bir dizi oluştur
            byte[] randomNumber = new byte[64];
            using RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);

            // Byte dizisini Base64 string'e çevir
            string refreshToken = Convert.ToBase64String(randomNumber);
            return ServiceResult<string>.Success(refreshToken, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return ServiceResult<string>.Failure($"Refresh token oluşturulurken hata: {ex.Message}", HttpStatusCode.InternalServerError);
        }
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
        catch (SecurityTokenException ex)
        {
            return ServiceResult<ClaimsPrincipal>.Failure($"Token doğrulama hatası: {ex.Message}", HttpStatusCode.Unauthorized);
        }
        catch (Exception ex)
        {
            return ServiceResult<ClaimsPrincipal>.Failure($"Token işlenirken hata: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Refresh token kullanarak yeni bir access token oluşturur.
    /// Veritabanındaki refresh token'ı doğrular ve yeni token çifti döndürür.
    /// </summary>
    public async Task<ServiceResult<RefreshTokenResponse>> RefreshAccessTokenAsync(string expiredAccessToken, string refreshToken)
    {
        try
        {
            // 1. Süresi dolmuş access token'dan kullanıcı bilgilerini çıkar
            var principalResult = GetPrincipalFromExpiredToken(expiredAccessToken);
            if (!principalResult.IsSuccess || principalResult.Data == null)
            {
                return ServiceResult<RefreshTokenResponse>.Failure("Geçersiz access token", HttpStatusCode.Unauthorized);
            }

            ClaimsPrincipal principal = principalResult.Data;
            string? userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return ServiceResult<RefreshTokenResponse>.Failure("Token'da kullanıcı bilgisi bulunamadı", HttpStatusCode.Unauthorized);
            }

            // 2. UserService ile refresh token'ı doğrula ve kullanıcı bilgilerini al
            var userValidationResult = await userService.ValidateAndGetUserByRefreshTokenAsync(userId, refreshToken);
            if (!userValidationResult.IsSuccess || userValidationResult.Data == null)
            {
                return ServiceResult<RefreshTokenResponse>.Failure(
                    userValidationResult.ErrorList?.FirstOrDefault() ?? "Refresh token doğrulanamadı",
                    userValidationResult.StatusCode
                );
            }

            var userWithRoles = userValidationResult.Data;

            // 3. Yeni access token oluştur
            var accessTokenResult = GenerateAccessToken(
                userWithRoles.User.Id.ToString(),
                userWithRoles.User.Email!,
                userWithRoles.Roles
            );

            if (!accessTokenResult.IsSuccess || accessTokenResult.Data == null)
            {
                return ServiceResult<RefreshTokenResponse>.Failure("Yeni access token oluşturulamadı", HttpStatusCode.InternalServerError);
            }

            // 4. Yeni refresh token oluştur
            var newRefreshTokenResult = GenerateRefreshToken();
            if (!newRefreshTokenResult.IsSuccess || newRefreshTokenResult.Data == null)
            {
                return ServiceResult<RefreshTokenResponse>.Failure("Yeni refresh token oluşturulamadı", HttpStatusCode.InternalServerError);
            }

            // 5. Yeni refresh token'ı veritabanına kaydet
            var updateResult = await userService.UpdateRefreshTokenAsync(
                userId,
                newRefreshTokenResult.Data,
                _jwtSettings.RefreshTokenExpiresInDays
            );

            if (!updateResult.IsSuccess)
            {
                return ServiceResult<RefreshTokenResponse>.Failure("Refresh token güncellenemedi", HttpStatusCode.InternalServerError);
            }

            // 6. Yeni token çiftini döndür
            RefreshTokenResponse response = new()
            {
                AccessToken = accessTokenResult.Data,
                RefreshToken = newRefreshTokenResult.Data
            };

            return ServiceResult<RefreshTokenResponse>.Success(response, HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return ServiceResult<RefreshTokenResponse>.Failure($"Refresh token işlemi sırasında hata: {ex.Message}", HttpStatusCode.InternalServerError);
        }
    }
}
