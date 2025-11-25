import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useSearchParams, useNavigate } from "react-router-dom";
import { resetPasswordSchema } from "@/types/auth";
import type { ResetPasswordRequest } from "@/types/auth";
import { authService } from "@/services/auth.service";
import { cn } from "@/lib/utils";
import AuthLayout from "@/layouts/AuthLayout";
import { Eye, EyeOff } from "lucide-react";
import type { ServiceResult } from "@/types/api";

export default function ResetPasswordPage() {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);
    const [showPasswordConfirm, setShowPasswordConfirm] = useState(false);

    const userId = searchParams.get("userId");
    const token = searchParams.get("token");

    const {
        register,
        handleSubmit,
        setValue,
        formState: { errors },
    } = useForm<ResetPasswordRequest>({
        resolver: zodResolver(resetPasswordSchema),
        defaultValues: {
            password: "",
            passwordConfirm: "",
            userId: userId || "",
            token: token || "",
        },
    });

    useEffect(() => {
        if (userId) setValue("userId", userId);
        if (token) setValue("token", token);
    }, [userId, token, setValue]);

    const onSubmit = async (data: ResetPasswordRequest) => {
        if (!userId || !token) {
            setError("Geçersiz şifre sıfırlama linki. Lütfen tekrar deneyiniz.");
            return;
        }

        setLoading(true);
        setError(null);
        setSuccess(null);
        try {
            const response = await authService.resetPassword(data);
            if (response.isSuccess) {
                setSuccess("Şifreniz başarıyla güncellendi. Giriş sayfasına yönlendiriliyorsunuz...");
                setTimeout(() => {
                    navigate("/login");
                }, 3000);
            } else {
                if (response.errorList && response.errorList.length > 0) {
                    setError(response.errorList.join(" "));
                } else {
                    setError("Şifre sıfırlama başarısız.");
                }
            }
        } catch (err: any) {
            const errorResponse = err.response?.data as ServiceResult;
            if (errorResponse?.errorList && errorResponse.errorList.length > 0) {
                setError(errorResponse.errorList.join(" "));
            } else {
                setError(err.response?.data?.message || "Bir hata oluştu.");
            }
        } finally {
            setLoading(false);
        }
    };

    return (
        <AuthLayout title="ŞİFRE YENİLEME">
            <div className="text-center mb-6 text-gray-600 text-sm">
                Lütfen yeni şifrenizi belirleyiniz.
            </div>

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                {error && (
                    <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative text-sm" role="alert">
                        <span className="block sm:inline">{error}</span>
                    </div>
                )}

                {success && (
                    <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded relative text-sm" role="alert">
                        <span className="block sm:inline">{success}</span>
                    </div>
                )}

                <div className="space-y-1 relative">
                    <label htmlFor="password" className="block text-[0.9rem] font-semibold text-primary-dark mb-1">
                        Yeni Şifre
                    </label>
                    <div className="relative">
                        <input
                            id="password"
                            type={showPassword ? "text" : "password"}
                            placeholder="........"
                            {...register("password")}
                            className={cn(
                                "w-full bg-[#f8f9fa] border-2 border-[#e9ecef] px-4 py-3 rounded-lg text-primary-dark text-[0.95rem] transition-all duration-300",
                                "focus:bg-white focus:border-primary-yellow focus:outline-none",
                                errors.password && "border-destructive focus:border-destructive"
                            )}
                        />
                        <button
                            type="button"
                            onClick={() => setShowPassword(!showPassword)}
                            className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 focus:outline-none"
                        >
                            {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                        </button>
                    </div>
                    {errors.password && (
                        <span className="text-xs text-destructive block mt-1">{errors.password.message}</span>
                    )}
                </div>

                <div className="space-y-1 relative">
                    <label htmlFor="passwordConfirm" className="block text-[0.9rem] font-semibold text-primary-dark mb-1">
                        Yeni Şifre (Tekrar)
                    </label>
                    <div className="relative">
                        <input
                            id="passwordConfirm"
                            type={showPasswordConfirm ? "text" : "password"}
                            placeholder="........"
                            {...register("passwordConfirm")}
                            className={cn(
                                "w-full bg-[#f8f9fa] border-2 border-[#e9ecef] px-4 py-3 rounded-lg text-primary-dark text-[0.95rem] transition-all duration-300",
                                "focus:bg-white focus:border-primary-yellow focus:outline-none",
                                errors.passwordConfirm && "border-destructive focus:border-destructive"
                            )}
                        />
                        <button
                            type="button"
                            onClick={() => setShowPasswordConfirm(!showPasswordConfirm)}
                            className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gray-600 focus:outline-none"
                        >
                            {showPasswordConfirm ? <EyeOff size={18} /> : <Eye size={18} />}
                        </button>
                    </div>
                    {errors.passwordConfirm && (
                        <span className="text-xs text-destructive block mt-1">{errors.passwordConfirm.message}</span>
                    )}
                </div>

                <button
                    type="submit"
                    disabled={loading}
                    className="w-full bg-primary-yellow text-primary-dark py-3 px-4 rounded-lg font-bold uppercase tracking-[0.5px] text-[0.9rem] transition-all duration-300 hover:bg-[#FFC107] hover:text-black hover:-translate-y-[2px] hover:shadow-[0_4px_12px_rgba(255,215,0,0.4)] active:translate-y-0 disabled:opacity-70 disabled:cursor-not-allowed"
                >
                    {loading ? "Güncelleniyor..." : "ŞİFREYİ GÜNCELLE"}
                </button>

                <div className="text-center mt-6 pt-6 border-t border-[#eee]">
                    <Link
                        to="/login"
                        className="inline-flex items-center text-primary-dark font-bold text-[0.9rem] hover:text-[#555] transition-colors group underline decoration-2 underline-offset-4"
                    >
                        Giriş sayfasına dön
                    </Link>
                </div>
            </form>
        </AuthLayout>
    );
}

