import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link, useNavigate } from "react-router-dom";
import { loginSchema } from "@/types/auth";
import type { LoginRequest } from "@/types/auth";
import { authService } from "@/services/auth.service";
import { cn } from "@/lib/utils";
import AuthLayout from "@/layouts/AuthLayout";
import { Eye, EyeOff } from "lucide-react";
import type { ServiceResult } from "@/types/api";

export default function LoginPage() {
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
      const response = await authService.login(data);
      if (response.isSuccess) {
        localStorage.setItem("accessToken", response.data.accessToken);
        localStorage.setItem("refreshToken", response.data.refreshToken);
        navigate("/");
      } else {
        // Show errorList content if available
        if (response.errorList && response.errorList.length > 0) {
          setError(response.errorList.join(" "));
        } else {
          setError("Giriş başarısız.");
        }
      }
    } catch (err: any) {
      // Handle network errors or unexpected issues
      const errorResponse = err.response?.data as ServiceResult;
      if (errorResponse?.errorList && errorResponse.errorList.length > 0) {
        setError(errorResponse.errorList.join(" "));
      } else {
        setError(err.response?.data?.message || "Giriş yapılırken bir hata oluştu.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout title="Giriş Yap">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {error && (
          <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded relative text-sm" role="alert">
            <span className="block sm:inline">{error}</span>
          </div>
        )}

        <div className="space-y-1">
          <label htmlFor="email" className="block text-[0.9rem] font-semibold text-primary-dark mb-1">
            Email Adresi
          </label>
          <input
            id="email"
            type="email"
            {...register("email")}
            className={cn(
              "w-full bg-[#f8f9fa] border-2 border-[#e9ecef] px-4 py-3 rounded-lg text-primary-dark text-[0.95rem] transition-all duration-300",
              "focus:bg-white focus:border-primary-yellow focus:outline-none",
              errors.email && "border-destructive focus:border-destructive"
            )}
          />
          {errors.email && (
            <span className="text-xs text-destructive block mt-1">{errors.email.message}</span>
          )}
        </div>

        <div className="space-y-1">
          <div className="flex items-center justify-between mb-1">
            <label htmlFor="password" className="block text-[0.9rem] font-semibold text-primary-dark">
              Şifre
            </label>
            <Link
              to="/forgot-password"
              className="text-xs text-[#121212] font-semibold hover:text-[#555] transition-colors"
            >
              Şifremi Unuttum
            </Link>
          </div>
          <div className="relative">
            <input
              id="password"
              type={showPassword ? "text" : "password"}
              {...register("password")}
              className={cn(
                "w-full bg-[#f8f9fa] border-2 border-[#e9ecef] px-4 py-3 rounded-lg text-primary-dark text-[0.95rem] transition-all duration-300 pr-12",
                "focus:bg-white focus:border-primary-yellow focus:outline-none",
                errors.password && "border-destructive focus:border-destructive"
              )}
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute right-0 top-0 h-full px-3 bg-[#f8f9fa] border-2 border-[#e9ecef] border-l-0 rounded-r-lg text-[#6c757d] hover:bg-[#e9ecef] hover:text-primary-dark transition-colors flex items-center justify-center"
              style={{ marginTop: '0', height: '100%' }}
            >
              {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          </div>
          {errors.password && (
            <span className="text-xs text-destructive block mt-1">{errors.password.message}</span>
          )}
        </div>

        <div className="flex items-center space-x-2 py-2">
          <input
            type="checkbox"
            id="rememberMe"
            {...register("rememberMe")}
            className="h-4 w-4 rounded border-gray-300 text-primary-yellow focus:ring-primary-yellow accent-primary-yellow"
          />
          <label htmlFor="rememberMe" className="text-sm text-[#888]">
            Beni Hatırla
          </label>
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-primary-yellow text-primary-dark py-3 px-4 rounded-lg font-bold uppercase tracking-[0.5px] text-[0.9rem] transition-all duration-300 hover:bg-[#FFC107] hover:text-black hover:-translate-y-[2px] hover:shadow-[0_4px_12px_rgba(255,215,0,0.4)] active:translate-y-0 disabled:opacity-70 disabled:cursor-not-allowed"
        >
          {loading ? "Giriş Yapılıyor..." : "Giriş Yap"}
        </button>

        <div className="text-center mt-6 pt-6 border-t border-[#eee] text-[0.9rem] text-[#666]">
          Hesabınız yok mu?{" "}
          <Link
            to="/register"
            className="text-primary-dark font-bold border-b-2 border-primary-yellow pb-[1px] hover:bg-primary-yellow hover:border-transparent transition-all duration-200"
          >
            Kayıt Ol
          </Link>
        </div>
      </form>
    </AuthLayout>
  );
}
