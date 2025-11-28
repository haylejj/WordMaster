import axios, { type InternalAxiosRequestConfig } from "axios";
import type { RefreshTokenResponse, ServiceResultWithData } from "@/types/api";

const BASE_URL = import.meta.env.VITE_API_URL || "https://localhost:3001/api";

const api = axios.create({
    baseURL: BASE_URL,
    headers: {
        "Content-Type": "application/json",
    },
});

const AUTH_EXCLUDED_PATHS = [
    "/auth/ping",
    "/auth/login",
    "/auth/admin-login",
    "/auth/register",
    "/auth/forget-password",
    "/auth/reset-password",
    "/auth/refresh-token",
];

const AUTH_VIEW_PATHS = ["/login", "/register", "/forgot-password", "/ResetPassword"];

const isAuthView = (path: string) => AUTH_VIEW_PATHS.some((authPath) => path.startsWith(authPath));

const clearTokens = () => {
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
};

const redirectToLogin = () => {
    if (!isAuthView(window.location.pathname)) {
        window.location.href = "/login";
    }
};

let refreshPromise: Promise<string | null> | null = null;

const refreshAccessToken = async (): Promise<string | null> => {
    const refreshToken = localStorage.getItem("refreshToken");
    const accessToken = localStorage.getItem("accessToken");
    if (!refreshToken || !accessToken) return null;

    try {
        const response = await axios.post<ServiceResultWithData<RefreshTokenResponse>>(
            `${BASE_URL}/auth/refresh-token`,
            { accessToken, refreshToken }
        );

        if (response.data.isSuccess && response.data.data) {
            const { accessToken: newAccess, refreshToken: newRefresh } = response.data.data;
            localStorage.setItem("accessToken", newAccess);
            localStorage.setItem("refreshToken", newRefresh);
            return newAccess;
        }
    } catch (error) {
        console.error("Token refresh failed", error);
    }

    return null;
};

api.interceptors.request.use(
    (config) => {
        const shouldSkipAuth = config.url
            ? AUTH_EXCLUDED_PATHS.some((path) => config.url?.startsWith(path))
            : false;
        if (!shouldSkipAuth) {
            const accessToken = localStorage.getItem("accessToken");
            if (accessToken) {
                config.headers = config.headers ?? {};
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

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const status = error.response?.status;
        const originalRequest = error.config as RetryRequestConfig;

        if (status === 401) {
            const refreshToken = localStorage.getItem("refreshToken");
            const isAuthEndpoint = originalRequest?.url
                ? AUTH_EXCLUDED_PATHS.some((path) => originalRequest.url?.startsWith(path))
                : false;
            const isRefreshCall = originalRequest?.url?.includes("/auth/refresh-token");

            if (refreshToken && !originalRequest?._retry && !isAuthEndpoint && !isRefreshCall) {
                originalRequest._retry = true;

                if (!refreshPromise) {
                    refreshPromise = refreshAccessToken();
                }

                const newAccessToken = await refreshPromise;
                refreshPromise = null;

                if (newAccessToken) {
                    originalRequest.headers = originalRequest.headers ?? {};
                    originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;
                    return api(originalRequest);
                }
            }

            clearTokens();
            redirectToLogin();
        }

        if (status === 403) {
            window.dispatchEvent(new Event("permission-denied"));
        }

        return Promise.reject(error);
    }
);

export default api;
