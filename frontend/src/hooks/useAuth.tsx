import { createContext, useContext, useState, useCallback, useEffect, useRef, useMemo, type ReactNode } from "react";
import axios from "axios";
import type { RefreshTokenResponse, ServiceResultWithData } from "@/types/api";

const BASE_URL = import.meta.env.VITE_API_URL || "http://localhost:3002/api/v1";

interface AuthContextType {
    /** Mevcut access token */
    accessToken: string | null;
    /** Access token'ın sona erme zamanı */
    expiresAt: Date | null;
    /** Token'ı set et (login sonrası) */
    setAuth: (accessToken: string, expiresAt: string) => void;
    /** Token'ı temizle (logout sonrası) */
    clearAuth: () => void;
    /** Kullanıcı login olmuş mu? */
    isAuthenticated: boolean;
    /** Auth durumu yükleniyor mu? */
    isLoading: boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

interface AuthProviderProps {
    children: ReactNode;
}

// Global state - API interceptor için
let globalAccessToken: string | null = null;
let globalSetAuth: ((token: string, expiresAt: string) => void) | null = null;
let globalClearAuth: (() => void) | null = null;

export function setGlobalAuthFunctions(
    setAuth: (token: string, expiresAt: string) => void,
    clearAuth: () => void
) {
    globalSetAuth = setAuth;
    globalClearAuth = clearAuth;
}

export function setGlobalAccessToken(token: string | null) {
    globalAccessToken = token;
}

export function getGlobalAccessToken(): string | null {
    return globalAccessToken;
}

export function getGlobalSetAuth() {
    return globalSetAuth;
}

export function getGlobalClearAuth() {
    return globalClearAuth;
}

// Public route'lar - bu sayfalarda refresh token kontrolü YAPILMAZ
const PUBLIC_ROUTES = [
    "/",
    "/login",
    "/register",
    "/forgot-password",
    "/ResetPassword",
    "/admin/login",
    "/confirm-email",
    "/auth/google-callback",
] as const;

/**
 * Mevcut sayfanın public olup olmadığını kontrol eder.
 * Public sayfalar listesinde değilse → Protected kabul edilir → Refresh token kontrolü yapılır.
 */
const isPublicRoute = (): boolean => {
    const path = window.location.pathname;
    return PUBLIC_ROUTES.some(route => path === route || path.startsWith(route + "/"));
};

// Token süresinin dolmasına kaç ms kala refresh yapılacak (2 dakika)
const PROACTIVE_REFRESH_BUFFER_MS = 2 * 60 * 1000;

/**
 * Authentication Context Provider
 * - Access Token: Memory'de saklanır
 * - Refresh Token: HttpOnly cookie'de
 * - Proaktif Refresh: Token süresinin 2 dk öncesinde otomatik yenileme
 */
export function AuthProvider({ children }: AuthProviderProps) {
    const [accessToken, setAccessToken] = useState<string | null>(null);
    const [expiresAt, setExpiresAt] = useState<Date | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const initializationRef = useRef(false);
    const refreshTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

    // Token yenileme fonksiyonu
    const refreshToken = useCallback(async (): Promise<boolean> => {
        try {
            const response = await axios.post<ServiceResultWithData<RefreshTokenResponse>>(
                `${BASE_URL}/auth/refresh-token`,
                { accessToken: globalAccessToken ?? "" },
                {
                    withCredentials: true,
                    timeout: 10000
                }
            );

            if (response.data.isSuccess && response.data.data) {
                const { accessToken: newToken, expiresAt: newExpiresAt } = response.data.data;
                setAccessToken(newToken);
                setExpiresAt(new Date(newExpiresAt));
                globalAccessToken = newToken;
                return true;
            }
        } catch {
            // Token yenileme başarısız - sessizce başarısız ol
        }
        return false;
    }, []);

    // Proaktif token yenileme zamanlayıcısını ayarla
    useEffect(() => {
        // Timer'ı temizle
        if (refreshTimerRef.current) {
            clearTimeout(refreshTimerRef.current);
            refreshTimerRef.current = null;
        }

        // Token yoksa veya expiresAt yoksa zamanlayıcı kurma
        if (!accessToken || !expiresAt) return;

        // Token'ın ne zaman sona ereceğini hesapla
        const timeUntilExpiry = expiresAt.getTime() - Date.now();
        const timeUntilRefresh = timeUntilExpiry - PROACTIVE_REFRESH_BUFFER_MS;

        // Eğer 2 dakikadan az kaldıysa hemen yenile
        if (timeUntilRefresh <= 0) {
            refreshToken();
            return;
        }

        // Timer kur - 2 dakika kala otomatik yenile
        refreshTimerRef.current = setTimeout(async () => {
            const success = await refreshToken();
            if (!success) {
                // Yenileme başarısız olursa kullanıcı 401 aldığında yönlendirilecek
                console.warn("Proactive token refresh failed");
            }
        }, timeUntilRefresh);

        // Cleanup
        return () => {
            if (refreshTimerRef.current) {
                clearTimeout(refreshTimerRef.current);
                refreshTimerRef.current = null;
            }
        };
    }, [accessToken, expiresAt, refreshToken]);

    // Sayfa yüklendiğinde refresh token ile yeni access token al
    useEffect(() => {
        // StrictMode'da çift çalışmayı önle
        if (initializationRef.current) return;
        initializationRef.current = true;

        const initializeAuth = async () => {
            if (isPublicRoute()) {
                setIsLoading(false);
                return;
            }

            try {
                const response = await axios.post<ServiceResultWithData<RefreshTokenResponse>>(
                    `${BASE_URL}/auth/refresh-token`,
                    { accessToken: "" },
                    {
                        withCredentials: true,
                        timeout: 5000 // 5 saniye timeout
                    }
                );

                if (response.data.isSuccess && response.data.data) {
                    const { accessToken: newToken, expiresAt: newExpiresAt } = response.data.data;
                    setAccessToken(newToken);
                    setExpiresAt(new Date(newExpiresAt));
                    // Global state'i de güncelle
                    globalAccessToken = newToken;
                }
            } catch {
                // Refresh token yok veya geçersiz - normal durum
            } finally {
                setIsLoading(false);
            }
        };

        initializeAuth();
    }, []);

    const setAuth = useCallback((token: string, expiresAtStr: string) => {
        setAccessToken(token);
        setExpiresAt(new Date(expiresAtStr));
        globalAccessToken = token;
    }, []);

    const clearAuth = useCallback(() => {
        setAccessToken(null);
        setExpiresAt(null);
        globalAccessToken = null;
        // Timer'ı da temizle
        if (refreshTimerRef.current) {
            clearTimeout(refreshTimerRef.current);
            refreshTimerRef.current = null;
        }
    }, []);

    const isAuthenticated = accessToken !== null;

    // Context value'yu memoize et - gereksiz re-render'ları önle
    const value = useMemo<AuthContextType>(() => ({
        accessToken,
        expiresAt,
        setAuth,
        clearAuth,
        isAuthenticated,
        isLoading,
    }), [accessToken, expiresAt, setAuth, clearAuth, isAuthenticated, isLoading]);

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

/**
 * Auth context hook
 */
export function useAuth(): AuthContextType {
    const context = useContext(AuthContext);
    if (context === undefined) {
        throw new Error("useAuth must be used within an AuthProvider");
    }
    return context;
}
