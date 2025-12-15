using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text.Json;
using WordMaster.Application.Helpers;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Configuration;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Helpers;
using WordMaster.Domain.Results;
using WordMaster.Application.Responses.Auth;
using WordMaster.Application.Responses.Google;

namespace WordMaster.Infrastructure.Services;

/// <summary>
/// Google OAuth 2.0 ile kimlik doğrulama işlemlerini yöneten servis.
/// </summary>
public class GoogleAuthService(
    UserManager<AppUser> userManager,
    IOptions<GoogleAuthSettings> googleAuthSettings,
    IOptions<UrlsSettings> urlsSettings,
    IOptions<JwtSettings> jwtSettings,
    IJwtService jwtService,
    ILogHistoryService logHistoryService,
    IHttpContextAccessor httpContextAccessor,
    IRefreshTokenCookieHelper cookieHelper,
    ICacheService cacheService,
    IUsernameHelper usernameHelper,
    ILogger<GoogleAuthService> logger) : IGoogleAuthService
{
    private readonly GoogleAuthSettings _googleSettings = googleAuthSettings.Value;
    private readonly UrlsSettings _urlsSettings = urlsSettings.Value;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;
    private static readonly HttpClient _httpClient = new();
    private const string GoogleLoginProvider = "Google";

    /// <inheritdoc />
    public string GetGoogleAuthUrl()
    {
        string redirectUri = $"{_urlsSettings.Api}/api/v1/auth/google-callback";
        string scope = "openid email profile";

        return $"https://accounts.google.com/o/oauth2/v2/auth?" +
               $"client_id={_googleSettings.ClientId}&" +
               $"redirect_uri={Uri.EscapeDataString(redirectUri)}&" +
               $"response_type=code&" +
               $"scope={Uri.EscapeDataString(scope)}&" +
               $"access_type=offline&" +
               $"prompt=consent";
    }

    /// <inheritdoc />
    public async Task<ServiceResult<LoginTokenResult>> HandleGoogleCallbackAsync(string code)
    {
        string? ipAddress = httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        try
        {
            // 1. Authorization code'u access token ile değiştir
            GoogleTokenResponse? tokenResponse = await ExchangeCodeForTokensAsync(code);
            if (tokenResponse == null)
            {
                logger.LogWarning("Google OAuth: Failed to exchange code for tokens");
                return ServiceResult<LoginTokenResult>.Failure("Google ile giriş başarısız oldu.", HttpStatusCode.BadRequest);
            }

            // 2. ID token'ı doğrula ve kullanıcı bilgilerini al
            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(tokenResponse.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_googleSettings.ClientId]
                });
            }
            catch (InvalidJwtException ex)
            {
                logger.LogWarning(ex, "Google OAuth: Invalid ID token");
                return ServiceResult<LoginTokenResult>.Failure("Geçersiz Google token.", HttpStatusCode.BadRequest);
            }

            // 3. Kullanıcıyı bul veya oluştur (Best Practice: UserLogins tablosu kullanılır)
            AppUser? user = await FindOrCreateUserAsync(payload);
            if (user == null)
            {
                return ServiceResult<LoginTokenResult>.Failure("Kullanıcı oluşturulamadı.", HttpStatusCode.InternalServerError);
            }

            // 4. JWT token oluştur
            return await GenerateTokensAsync(user, ipAddress);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Google OAuth callback failed");
            return ServiceResult<LoginTokenResult>.Failure("Google ile giriş başarısız oldu.", HttpStatusCode.InternalServerError);
        }
    }

    /// <summary>
    /// Google Subject ID veya email ile kullanıcıyı bulur, yoksa oluşturur.
    /// Best Practice: Önce UserLogins tablosunda Google Subject ID ile arar.
    /// </summary>
    private async Task<AppUser?> FindOrCreateUserAsync(GoogleJsonWebSignature.Payload payload)
    {
        // 1. Önce Google Subject ID ile ara (en güvenilir yöntem)
        // Bu sayede kullanıcı Google'da email değiştirse bile eşleşir
        AppUser? user = await userManager.FindByLoginAsync(GoogleLoginProvider, payload.Subject);

        if (user != null)
        {
            logger.LogInformation("Google OAuth: User found by Google Subject ID {Subject}", payload.Subject);
            return user;
        }

        // 2. Google login kaydı yoksa, email ile ara (mevcut kullanıcıyı link etmek için)
        user = await userManager.FindByEmailAsync(payload.Email);

        if (user != null)
        {
            // Mevcut kullanıcıya Google login'i ekle (hesap linking)
            IdentityResult linkResult = await AddGoogleLoginAsync(user, payload.Subject);
            if (!linkResult.Succeeded)
            {
                logger.LogWarning("Failed to link Google account: {Errors}",
                    string.Join(", ", linkResult.Errors.Select(e => e.Description)));
                // Link başarısız olsa bile kullanıcıyı döndür (login devam edebilir)
            }
            else
            {
                logger.LogInformation("Google OAuth: Linked Google account to existing user {Email}", payload.Email);
            }
            return user;
        }

        // 3. Hiç kullanıcı yoksa yeni oluştur
        user = await CreateGoogleUserAsync(payload);
        if (user != null)
        {
            logger.LogInformation("Google OAuth: New user created with email {Email}", payload.Email);
        }

        return user;
    }

    /// <summary>
    /// Kullanıcıya Google external login kaydı ekler.
    /// </summary>
    private async Task<IdentityResult> AddGoogleLoginAsync(AppUser user, string googleSubjectId)
    {
        UserLoginInfo loginInfo = new(GoogleLoginProvider, googleSubjectId, GoogleLoginProvider);
        return await userManager.AddLoginAsync(user, loginInfo);
    }

    /// <summary>
    /// Google'dan alınan authorization code'u access token ile değiştirir.
    /// </summary>
    private async Task<GoogleTokenResponse?> ExchangeCodeForTokensAsync(string code)
    {
        string redirectUri = $"{_urlsSettings.Api}/api/v1/auth/google-callback";

        Dictionary<string, string> parameters = new()
        {
            ["code"] = code,
            ["client_id"] = _googleSettings.ClientId,
            ["client_secret"] = _googleSettings.ClientSecret,
            ["redirect_uri"] = redirectUri,
            ["grant_type"] = "authorization_code"
        };

        HttpResponseMessage response = await _httpClient.PostAsync(
            "https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(parameters));

        if (!response.IsSuccessStatusCode)
        {
            string error = await response.Content.ReadAsStringAsync();
            logger.LogWarning("Google token exchange failed: {Error}", error);
            return null;
        }

        string content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<GoogleTokenResponse>(content);
    }

    /// <summary>
    /// Google kullanıcı bilgileriyle yeni kullanıcı oluşturur ve Google login kaydını ekler.
    /// </summary>
    private async Task<AppUser?> CreateGoogleUserAsync(GoogleJsonWebSignature.Payload payload)
    {
        // Ad ve soyadı ayır
        string firstName = payload.GivenName ?? payload.Name?.Split(' ').FirstOrDefault() ?? "User";
        string lastName = payload.FamilyName ?? payload.Name?.Split(' ').Skip(1).FirstOrDefault() ?? "";

        // Benzersiz username oluştur
        string username = await usernameHelper.GenerateUniqueUsernameAsync(firstName, lastName);

        AppUser newUser = new()
        {
            UserName = username,
            Email = payload.Email,
            EmailConfirmed = payload.EmailVerified, // Google doğrulamış
            FirstName = firstName,
            LastName = string.IsNullOrEmpty(lastName) ? null : lastName
        };

        IdentityResult result = await userManager.CreateAsync(newUser);
        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to create Google user: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
            return null;
        }

        // Google login kaydını ekle (UserLogins tablosuna)
        IdentityResult loginResult = await AddGoogleLoginAsync(newUser, payload.Subject);
        if (!loginResult.Succeeded)
        {
            logger.LogWarning("Failed to add Google login for new user: {Errors}",
                string.Join(", ", loginResult.Errors.Select(e => e.Description)));
            // Kullanıcı oluşturuldu ama login kaydı eklenemedi - yine de devam et
        }

        // Varsayılan rol ata
        await userManager.AddToRoleAsync(newUser, "user");

        return newUser;
    }

    /// <summary>
    /// Access ve refresh token oluşturur.
    /// </summary>
    private async Task<ServiceResult<LoginTokenResult>> GenerateTokensAsync(AppUser user, string? ipAddress)
    {
        // Refresh token oluştur
        ServiceResult<string> refreshTokenResult = jwtService.GenerateRefreshToken();

        // Refresh token'ı HASH'leyerek veritabanına kaydet
        user.RefreshToken = RefreshTokenHasher.HashRefreshToken(refreshTokenResult.Data!);
        user.RefreshTokenExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiresInDays);
        await userManager.UpdateAsync(user);

        // SecurityStamp cache'ini set et
        await cacheService.SetAsync($"security_stamp:{user.Id}", user.SecurityStamp, TimeSpan.FromHours(1));

        IList<string> roles = await userManager.GetRolesAsync(user);

        // Access token oluştur
        ServiceResult<string> accessTokenResult = jwtService.GenerateAccessToken(
            user.Id.ToString(), user.UserName!, user.Email!, roles, user.SecurityStamp!);

        if (!accessTokenResult.IsSuccess)
        {
            return ServiceResult<LoginTokenResult>.Failure("Token oluşturulamadı.", HttpStatusCode.InternalServerError);
        }

        // Login kaydı
        await logHistoryService.RecordAsync(user.Id.ToString(), user.Email!, ipAddress, true, "GoogleLogin");

        // Cookie'ye refresh token yaz
        HttpContext? httpContext = httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            cookieHelper.SetRefreshTokenCookie(httpContext, refreshTokenResult.Data!);
        }

        return ServiceResult<LoginTokenResult>.Success(new LoginTokenResult
        {
            AccessToken = accessTokenResult.Data!,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiresInMinutes)
        }, HttpStatusCode.OK);
    }
}


