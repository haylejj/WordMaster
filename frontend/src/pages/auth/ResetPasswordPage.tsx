import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useSearchParams, useNavigate } from "react-router-dom";
import { resetPasswordSchema } from "@/types/auth";
import type { ResetPasswordRequest } from "@/types/auth";
import { authService } from "@/services/auth.service";
import { cn, getErrorMessage } from "@/lib/utils";
import AuthLayout from "@/layouts/AuthLayout";
import { Eye, EyeOff, Lock, ShieldCheck, Check, X } from "lucide-react";


const passwordRequirements = [
    { id: "length", label: "En az 6 karakter", test: (pwd: string) => pwd.length >= 6 },
    { id: "uppercase", label: "En az bir büyük harf (A-Z)", test: (pwd: string) => /[A-Z]/.test(pwd) },
    { id: "lowercase", label: "En az bir küçük harf (a-z)", test: (pwd: string) => /[a-z]/.test(pwd) },
    { id: "number", label: "En az bir rakam (0-9)", test: (pwd: string) => /\d/.test(pwd) },
    { id: "special", label: "En az bir özel karakter (!@#$%^&*)", test: (pwd: string) => /[^a-zA-Z0-9]/.test(pwd) },
];

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
        watch,
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

    const newPassword = watch("password") || "";

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
            setError(getErrorMessage(err));
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

                <div className="space-y-1">
                    <div
                        className={cn(
                            "flex items-center border-2 rounded-xl px-4 py-3 transition-colors",
                            errors.password ? "border-red-300 bg-red-50" : "border-gray-200 focus-within:border-primary-yellow"
                        )}
                    >
                        <Lock className="text-primary-yellow mr-3" size={18} />
                        <div className="flex-1">
                            <label className="block text-xs font-medium text-gray-500">Yeni Şifre</label>
                            <input
                                {...register("password")}
                                type={showPassword ? "text" : "password"}
                                className="w-full bg-transparent border-none outline-none text-gray-800 text-sm py-0.5"
                                placeholder="Yeni şifrenizi girin"
                            />
                        </div>
                        <button
                            type="button"
                            onClick={() => setShowPassword(!showPassword)}
                            className="text-gray-400 hover:text-gray-600 transition-colors"
                        >
                            {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                        </button>
                    </div>
                    {errors.password && <p className="text-xs text-red-500 ml-4">{errors.password.message}</p>}
                </div>

                <div className="space-y-1">
                    <div
                        className={cn(
                            "flex items-center border-2 rounded-xl px-4 py-3 transition-colors",
                            errors.passwordConfirm ? "border-red-300 bg-red-50" : "border-gray-200 focus-within:border-primary-yellow"
                        )}
                    >
                        <ShieldCheck className="text-primary-yellow mr-3" size={18} />
                        <div className="flex-1">
                            <label className="block text-xs font-medium text-gray-500">Yeni Şifre (Tekrar)</label>
                            <input
                                {...register("passwordConfirm")}
                                type={showPasswordConfirm ? "text" : "password"}
                                className="w-full bg-transparent border-none outline-none text-gray-800 text-sm py-0.5"
                                placeholder="Yeni şifrenizi tekrar girin"
                            />
                        </div>
                        <button
                            type="button"
                            onClick={() => setShowPasswordConfirm(!showPasswordConfirm)}
                            className="text-gray-400 hover:text-gray-600 transition-colors"
                        >
                            {showPasswordConfirm ? <EyeOff size={18} /> : <Eye size={18} />}
                        </button>
                    </div>
                    {errors.passwordConfirm && <p className="text-xs text-red-500 ml-4">{errors.passwordConfirm.message}</p>}
                </div>

                {/* Password Requirements - Live Check */}
                <div className="bg-gray-50 rounded-xl p-4">
                    <p className="text-sm font-medium text-gray-700 mb-3">Şifre gereksinimleri:</p>
                    <ul className="space-y-2">
                        {passwordRequirements.map((req) => {
                            const passed = req.test(newPassword);
                            return (
                                <li
                                    key={req.id}
                                    className={cn(
                                        "flex items-center gap-2 text-xs transition-colors",
                                        passed ? "text-green-600" : "text-gray-400"
                                    )}
                                >
                                    {passed ? (
                                        <Check size={14} className="text-green-500" />
                                    ) : (
                                        <X size={14} className="text-gray-300" />
                                    )}
                                    <span className={passed ? "font-medium" : ""}>{req.label}</span>
                                </li>
                            );
                        })}
                    </ul>
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

