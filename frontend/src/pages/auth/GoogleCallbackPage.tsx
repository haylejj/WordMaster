import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuth } from "@/hooks/useAuth";
import AuthLayout from "@/layouts/AuthLayout";

/**
 * Google OAuth callback sayfası.
 * Backend'den gelen token'ı alıp auth state'e kaydeder.
 */
export default function GoogleCallbackPage() {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const { setAuth } = useAuth();
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const token = searchParams.get("token");
        const expiresAt = searchParams.get("expiresAt");
        const errorParam = searchParams.get("error");

        if (errorParam) {
            setError(decodeURIComponent(errorParam));
            setTimeout(() => navigate("/login"), 3000);
            return;
        }

        if (token && expiresAt) {
            // Token'ı auth state'e kaydet
            setAuth(token, expiresAt);
            // Dashboard'a yönlendir
            navigate("/dashboard");
        } else {
            setError("Google ile giriş başarısız oldu.");
            setTimeout(() => navigate("/login"), 3000);
        }
    }, [searchParams, setAuth, navigate]);

    if (error) {
        return (
            <AuthLayout title="">
                <div className="flex flex-col items-center text-center space-y-6 py-4">
                    <div className="relative">
                        <div className="absolute -inset-1 rounded-full bg-red-500/20 blur-xl animate-pulse"></div>
                        <div className="relative h-20 w-20 bg-white rounded-full flex items-center justify-center border-2 border-red-500 shadow-sm">
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                width="40"
                                height="40"
                                viewBox="0 0 24 24"
                                fill="none"
                                stroke="currentColor"
                                strokeWidth="2"
                                strokeLinecap="round"
                                strokeLinejoin="round"
                                className="text-red-500"
                            >
                                <circle cx="12" cy="12" r="10" />
                                <line x1="15" y1="9" x2="9" y2="15" />
                                <line x1="9" y1="9" x2="15" y2="15" />
                            </svg>
                        </div>
                    </div>

                    <div className="space-y-2">
                        <h2 className="text-2xl font-bold text-primary-dark">Giriş Başarısız</h2>
                        <p className="text-gray-600 text-[0.95rem] leading-relaxed max-w-[280px] mx-auto">
                            {error}
                        </p>
                        <p className="text-sm text-gray-500">
                            Giriş sayfasına yönlendiriliyorsunuz...
                        </p>
                    </div>
                </div>
            </AuthLayout>
        );
    }

    return (
        <AuthLayout title="">
            <div className="flex flex-col items-center text-center space-y-6 py-4">
                <div className="relative">
                    <div className="absolute -inset-1 rounded-full bg-primary-yellow/20 blur-xl animate-pulse"></div>
                    <div className="relative h-20 w-20 bg-white rounded-full flex items-center justify-center border-2 border-primary-yellow shadow-sm">
                        <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-primary-yellow"></div>
                    </div>
                </div>

                <div className="space-y-2">
                    <h2 className="text-2xl font-bold text-primary-dark">Giriş Yapılıyor</h2>
                    <p className="text-gray-600 text-[0.95rem] leading-relaxed max-w-[280px] mx-auto">
                        Google hesabınızla giriş yapılıyor, lütfen bekleyin...
                    </p>
                </div>
            </div>
        </AuthLayout>
    );
}
