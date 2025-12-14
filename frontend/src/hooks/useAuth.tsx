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

// Protected route'lar - sadece bu sayfalarda refresh token kontrolü yapılır
const isProtectedRoute = (): boolean => {
    const path = window.location.pathname;
    return path.startsWith("/dashboard") || path.startsWith("/admin/");
};

/**
 * Authentication Context Provider
 * - Access Token: Memory'de saklanır
 * - Refresh Token: HttpOnly cookie'de
 */
export function AuthProvider({ children }: AuthProviderProps) {
    const [accessToken, setAccessToken] = useState<string | null>(null);
    const [expiresAt, setExpiresAt] = useState<Date | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const initializationRef = useRef(false);

    // Sayfa yüklendiğinde refresh token ile yeni access token al
    useEffect(() => {
        // StrictMode'da çift çalışmayı önle
        if (initializationRef.current) return;
        initializationRef.current = true;

        const initializeAuth = async () => {
            // Sadece protected route'larda refresh token kontrolü yap
            if (!isProtectedRoute()) {
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
