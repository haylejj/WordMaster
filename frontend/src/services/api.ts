import axios, { type InternalAxiosRequestConfig } from "axios";
import type { RefreshTokenResponse, ServiceResultWithData } from "@/types/api";
import {
    getGlobalAccessToken,
    setGlobalAccessToken,
    getGlobalSetAuth,
    getGlobalClearAuth
} from "@/hooks/useAuth";

const BASE_URL = import.meta.env.VITE_API_URL || "http://localhost:3002/api/v1";

/**
 * Axios instance - HttpOnly cookie desteği için withCredentials: true
 */
const api = axios.create({
    baseURL: BASE_URL,
    headers: {
        "Content-Type": "application/json",
    },
    withCredentials: true,
    timeout: 30000, // 30 saniye default timeout
});

// Auth gerektirmeyen endpoint'ler
const AUTH_EXCLUDED_PATHS = [
    "/auth/ping",
    "/auth/login",
    "/auth/admin-login",
    "/auth/register",
    "/auth/forget-password",
    "/auth/reset-password",
    "/auth/refresh-token",
    "/auth/confirm-email",
] as const;

// Auth sayfaları - buralardan redirect yapılmaz
const AUTH_VIEW_PATHS = ["/login", "/register", "/forgot-password", "/ResetPassword", "/admin/login"] as const;

const isAuthView = (path: string): boolean =>
    AUTH_VIEW_PATHS.some(authPath => path.startsWith(authPath));

const isAdminPath = (path: string): boolean =>
    path.startsWith("/admin");

const isAuthExcludedPath = (url: string | undefined): boolean =>
    url ? AUTH_EXCLUDED_PATHS.some(path => url.startsWith(path)) : false;

/**
 * Auth hatası durumunda token temizle ve uygun login sayfasına yönlendir
 */
const handleAuthFailure = (): void => {
    const clearAuth = getGlobalClearAuth();
    clearAuth?.();
    setGlobalAccessToken(null);

    const currentPath = window.location.pathname;

    if (!isAuthView(currentPath)) {
        // Admin panelindeyse admin login'e, değilse normal login'e yönlendir
        const redirectPath = isAdminPath(currentPath) ? "/admin/login" : "/login";
        window.location.href = redirectPath;
    }
};

// Concurrent refresh request'leri önlemek için singleton promise
let refreshPromise: Promise<string | null> | null = null;

/**
 * Access token'ı yenile
 */
const refreshAccessToken = async (): Promise<string | null> => {
    const currentToken = getGlobalAccessToken();

    try {
        const response = await axios.post<ServiceResultWithData<RefreshTokenResponse>>(
            `${BASE_URL}/auth/refresh-token`,
            { accessToken: currentToken ?? "" },
            {
                withCredentials: true,
                timeout: 10000 // Refresh için 10 saniye
            }
        );

        if (response.data.isSuccess && response.data.data) {
            const { accessToken: newAccess, expiresAt } = response.data.data;

            setGlobalAccessToken(newAccess);
            getGlobalSetAuth()?.(newAccess, expiresAt);

            return newAccess;
        }
    } catch (error) {
        // Sadece beklenmeyen hataları logla
        if (!axios.isAxiosError(error) || error.response?.status !== 401) {
            console.error("Token refresh failed:", error);
        }
    }

    return null;
};

/**
 * Request interceptor - Authorization header ekle
 */
api.interceptors.request.use(
    (config) => {
        if (!isAuthExcludedPath(config.url)) {
            const accessToken = getGlobalAccessToken();
            if (accessToken) {
                config.headers.Authorization = `Bearer ${accessToken}`;
            }
        }
        return config;
    },
    (error) => Promise.reject(error)
);

interface RetryRequestConfig extends InternalAxiosRequestConfig {
    _retry?: boolean;
}

/**
 * Response interceptor - Hata handling ve token refresh
 */
api.interceptors.response.use(
    (response) => {
        // 204 No Content durumunda default başarı objesi döndür
        if (response.status === 204) {
            response.data = {
                isSuccess: true,
                statusCode: 204,
                data: null,
                errorList: null
            };
        }
        return response;
    },
    async (error) => {
        const status = error.response?.status;
        const originalRequest = error.config as RetryRequestConfig | undefined;

        // 401 Unauthorized - Token refresh dene
        if (status === 401 && originalRequest) {
            const isAuthEndpoint = isAuthExcludedPath(originalRequest.url);
            const isRefreshCall = originalRequest.url?.includes("/auth/refresh-token");

            if (!originalRequest._retry && !isAuthEndpoint && !isRefreshCall) {
                originalRequest._retry = true;

                // Concurrent refresh'leri önle - tek promise kullan
                if (!refreshPromise) {
                    refreshPromise = refreshAccessToken().finally(() => {
                        refreshPromise = null;
                    });
                }

                const newAccessToken = await refreshPromise;

                if (newAccessToken) {
                    originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
                    return api(originalRequest);
                }
            }

            handleAuthFailure();
        }

        // 403 Forbidden - Yetki hatası
        if (status === 403) {
            window.dispatchEvent(new Event("permission-denied"));
        }

        // 429 Too Many Requests - Rate limit
        if (status === 429) {
            const errorList = error.response?.data?.errorList;
            const message = Array.isArray(errorList) && errorList.length > 0
                ? errorList[0]
                : "Çok fazla istek gönderildi. Lütfen bekleyin.";

            window.dispatchEvent(new CustomEvent("rate-limit-exceeded", {
                detail: { message }
            }));
        }

        return Promise.reject(error);
    }
);

export default api;
