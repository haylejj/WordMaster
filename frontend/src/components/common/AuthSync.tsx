import { useEffect } from "react";
import { useAuth, setGlobalAuthFunctions, setGlobalAccessToken } from "@/hooks/useAuth";

/**
 * AuthContext ve global state senkronizasyonu
 * API interceptor'ün token'a erişebilmesi için gerekli
 */
export default function AuthSync() {
    const { accessToken, setAuth, clearAuth } = useAuth();

    // Mount'ta global fonksiyonları bağla
    useEffect(() => {
        setGlobalAuthFunctions(setAuth, clearAuth);
    }, [setAuth, clearAuth]);

    // Token değiştiğinde global state'i güncelle
    useEffect(() => {
        setGlobalAccessToken(accessToken);
    }, [accessToken]);

    return null;
}
