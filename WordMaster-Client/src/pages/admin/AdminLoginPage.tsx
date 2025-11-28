import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { useNavigate } from "react-router-dom";
import { loginSchema } from "@/types/auth";
import type { LoginRequest } from "@/types/auth";
import { authService } from "@/services/auth.service";
import { cn, getErrorMessage } from "@/lib/utils";
import { Eye, EyeOff, ShieldAlert } from "lucide-react";

export default function AdminLoginPage() {
    const navigate = useNavigate();
    const [error, setError] = useState<string | null>(null);
    const [loading, setLoading] = useState(false);
    const [showPassword, setShowPassword] = useState(false);

    const {
        register,
        handleSubmit,
        formState: { errors },
    } = useForm<LoginRequest>({
        resolver: zodResolver(loginSchema),
        defaultValues: {
            email: "",
            password: "",
            rememberMe: false,
        },
    });

    const onSubmit = async (data: LoginRequest) => {
        setLoading(true);
        setError(null);
        try {
            const response = await authService.adminLogin(data);
            if (response.isSuccess) {
                localStorage.setItem("accessToken", response.data.accessToken);
                localStorage.setItem("refreshToken", response.data.refreshToken);
                navigate("/admin/dashboard");
            } else {
                if (response.errorList && response.errorList.length > 0) {
                    setError(response.errorList.join(" "));
                } else {
                    setError("Giriş başarısız.");
                }
            }
        } catch (err: any) {
            setError(getErrorMessage(err));
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="min-h-screen w-full bg-black flex items-center justify-center font-[Segoe_UI] overflow-hidden relative selection:bg-red-600 selection:text-white">
            {/* Background Effects */}
            <div className="fixed inset-0 bg-[radial-gradient(circle_at_center,_var(--tw-gradient-stops))] from-[#2a0000] via-[#000000] to-[#000000] z-0" />

            <div className="relative z-10 w-full max-w-[400px] m-4">
                <div className="bg-[#0a0a0a] border border-red-900/30 rounded-xl shadow-[0_0_50px_rgba(220,38,38,0.1)] p-8">

                    <div className="text-center mb-8">
                        <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-red-900/20 mb-4">
                            <ShieldAlert className="w-8 h-8 text-red-600" />
                        </div>
                        <h1 className="text-2xl font-bold text-white mb-2">Admin Panel</h1>
                        <p className="text-gray-500 text-sm">Yönetici girişi yapınız</p>
                    </div>

                    <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
                        {error && (
                            <div className="bg-red-950/50 border border-red-900 text-red-400 px-4 py-3 rounded text-sm text-center">
                                {error}
                            </div>
                        )}

                        <div className="space-y-1.5">
                            <label className="text-xs font-medium text-gray-400 uppercase tracking-wider">Email</label>
                            <input
                                type="email"
                                {...register("email")}
                                className={cn(
                                    "w-full bg-[#121212] border border-[#333] text-white px-4 py-3 rounded-lg focus:outline-none focus:border-red-600 transition-colors",
                                    errors.email && "border-red-500"
                                )}
                                placeholder="admin@wordmaster.com"
                            />
                            {errors.email && (
                                <span className="text-xs text-red-500">{errors.email.message}</span>
                            )}
                        </div>

                        <div className="space-y-1.5">
                            <label className="text-xs font-medium text-gray-400 uppercase tracking-wider">Şifre</label>
                            <div className="relative">
                                <input
                                    type={showPassword ? "text" : "password"}
                                    {...register("password")}
                                    className={cn(
                                        "w-full bg-[#121212] border border-[#333] text-white px-4 py-3 rounded-lg focus:outline-none focus:border-red-600 transition-colors pr-12",
                                        errors.password && "border-red-500"
                                    )}
                                    placeholder="••••••••"
                                />
                                <button
                                    type="button"
                                    onClick={() => setShowPassword(!showPassword)}
                                    className="absolute right-0 top-0 h-full px-3 text-gray-500 hover:text-white transition-colors"
                                >
                                    {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                                </button>
                            </div>
                            {errors.password && (
                                <span className="text-xs text-red-500">{errors.password.message}</span>
                            )}
                        </div>

                        <button
                            type="submit"
                            disabled={loading}
                            className="w-full bg-red-700 hover:bg-red-600 text-white font-semibold py-3 rounded-lg transition-all duration-300 transform hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed shadow-[0_4px_14px_0_rgba(220,38,38,0.39)]"
                        >
                            {loading ? "Giriş Yapılıyor..." : "Giriş Yap"}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
}
