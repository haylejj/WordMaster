import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link } from "react-router-dom";
import { registerSchema } from "@/types/auth";
import type { RegisterRequest } from "@/types/auth";
import { authService } from "@/services/auth.service";
import { cn, getErrorMessage } from "@/lib/utils";
import AuthLayout from "@/layouts/AuthLayout";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { Eye, EyeOff, AlertCircle } from "lucide-react";


export default function RegisterPage() {
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<RegisterRequest>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      firstName: "",
      lastName: "",
      email: "",
      phone: "",
      password: "",
      passwordConfirm: "",
    },
  });

  const [success, setSuccess] = useState(false);

  const onSubmit = async (data: RegisterRequest) => {
    setLoading(true);
    setError(null);
    try {
      const response = await authService.register(data);
      if (response.isSuccess) {
        setSuccess(true);
      } else {
        if (response.errorList && response.errorList.length > 0) {
          setError(response.errorList.join(" "));
        } else {
          setError("Kayıt başarısız.");
        }
      }
    } catch (err: any) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  if (success) {
    return (
      <AuthLayout title="">
        <div className="flex flex-col items-center text-center space-y-6 py-4">
          <div className="relative">
            <div className="absolute -inset-1 rounded-full bg-primary-yellow/20 blur-xl animate-pulse"></div>
            <div className="relative h-20 w-20 bg-white rounded-full flex items-center justify-center border-2 border-primary-yellow shadow-sm">
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
                className="text-primary-yellow"
              >
                <rect width="20" height="16" x="2" y="4" rx="2" />
                <path d="m22 7-8.97 5.7a1.94 1.94 0 0 1-2.06 0L2 7" />
              </svg>
            </div>
          </div>

          <div className="space-y-2">
            <h2 className="text-2xl font-bold text-primary-dark">Emailinizi Kontrol Edin</h2>
            <p className="text-gray-600 text-[0.95rem] leading-relaxed max-w-[280px] mx-auto">
              Kaydınız başarıyla oluşturuldu. Hesabınızı aktif etmek için lütfen email adresinize gönderilen bağlantıya tıklayın.
            </p>
            <p className="text-sm text-gray-500">
              Gönderilen bağlantı <span className="font-semibold text-primary-dark">1 saat</span> süreyle geçerlidir ve <span className="font-semibold text-primary-dark">tek kullanımlıktır</span>.
            </p>
          </div>

          <div className="w-full pt-2">
            <Link
              to="/login"
              className="block w-full text-center bg-primary-yellow text-primary-dark py-3 px-4 rounded-lg font-bold uppercase tracking-[0.5px] text-[0.9rem] transition-all duration-300 hover:bg-[#FFC107] hover:text-black hover:-translate-y-[2px] hover:shadow-[0_4px_12px_rgba(255,215,0,0.4)]"
            >
              Giriş Sayfasına Dön
            </Link>
          </div>
        </div>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout title="Kayıt Ol">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-3">
        {error && (
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertTitle>Hata</AlertTitle>
            <AlertDescription>
              {error}
            </AlertDescription>
          </Alert>
        )}

        <div className="grid grid-cols-2 gap-4">
          <div className="space-y-1">
            <label className="block text-[0.9rem] font-semibold text-primary-dark mb-1">Ad</label>
            <input
              {...register("firstName")}
              placeholder="Adınız"
              className={cn(
                "w-full bg-[#f8f9fa] border-2 border-[#e9ecef] px-4 py-3 rounded-lg text-primary-dark text-[0.95rem] transition-all duration-300",
                "focus:bg-white focus:border-primary-yellow focus:outline-none",
                errors.firstName && "border-destructive focus:border-destructive"
              )}
            />
            {errors.firstName && <span className="text-xs text-destructive">{errors.firstName.message}</span>}
          </div>
          <div className="space-y-1">
            <label className="block text-[0.9rem] font-semibold text-primary-dark mb-1">Soyad</label>
            <input
              {...register("lastName")}
              placeholder="Soyadınız"
              className={cn(
                "w-full bg-[#f8f9fa] border-2 border-[#e9ecef] px-4 py-3 rounded-lg text-primary-dark text-[0.95rem] transition-all duration-300",
                "focus:bg-white focus:border-primary-yellow focus:outline-none",
                errors.lastName && "border-destructive focus:border-destructive"
              )}
            />
            {errors.lastName && <span className="text-xs text-destructive">{errors.lastName.message}</span>}
          </div>
        </div>

        <div className="space-y-1">
          <label className="block text-[0.9rem] font-semibold text-primary-dark mb-1">Email</label>
          <input
            type="email"
            placeholder="ornek@email.com"
            {...register("email")}
            className={cn(
              "w-full bg-[#f8f9fa] border-2 border-[#e9ecef] px-4 py-3 rounded-lg text-primary-dark text-[0.95rem] transition-all duration-300",
              "focus:bg-white focus:border-primary-yellow focus:outline-none",
              errors.email && "border-destructive focus:border-destructive"
            )}
          />
          {errors.email && <span className="text-xs text-destructive">{errors.email.message}</span>}
        </div>

        <div className="space-y-1">
          <label className="block text-[0.9rem] font-semibold text-primary-dark mb-1">Telefon Numarası</label>
          <input
            type="tel"
            placeholder="555 123 45 67"
            {...register("phone")}
            className={cn(
              "w-full bg-[#f8f9fa] border-2 border-[#e9ecef] px-4 py-3 rounded-lg text-primary-dark text-[0.95rem] transition-all duration-300",
              "focus:bg-white focus:border-primary-yellow focus:outline-none",
              errors.phone && "border-destructive focus:border-destructive"
            )}
          />
          {errors.phone && <span className="text-xs text-destructive">{errors.phone.message}</span>}
        </div>



        <div className="grid grid-cols-2 gap-4">
          <div className="space-y-1">
            <label className="block text-[0.9rem] font-semibold text-primary-dark mb-1">Şifre</label>
            <div className="relative">
              <input
                type={showPassword ? "text" : "password"}
                {...register("password")}
                className={cn(
                  "w-full bg-[#f8f9fa] border-2 border-[#e9ecef] px-4 py-3 rounded-lg text-primary-dark text-[0.95rem] transition-all duration-300 pr-10",
                  "focus:bg-white focus:border-primary-yellow focus:outline-none",
                  errors.password && "border-destructive focus:border-destructive"
                )}
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                className="absolute right-2 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-primary-dark"
              >
                {showPassword ? <EyeOff size={16} /> : <Eye size={16} />}
              </button>
            </div>
            {errors.password && <span className="text-xs text-destructive">{errors.password.message}</span>}
          </div>
          <div className="space-y-1">
            <label className="block text-[0.9rem] font-semibold text-primary-dark mb-1">Şifre Tekrar</label>
            <div className="relative">
              <input
                type={showConfirmPassword ? "text" : "password"}
                {...register("passwordConfirm")}
                className={cn(
                  "w-full bg-[#f8f9fa] border-2 border-[#e9ecef] px-4 py-3 rounded-lg text-primary-dark text-[0.95rem] transition-all duration-300 pr-10",
                  "focus:bg-white focus:border-primary-yellow focus:outline-none",
                  errors.passwordConfirm && "border-destructive focus:border-destructive"
                )}
              />
              <button
                type="button"
                onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                className="absolute right-2 top-1/2 transform -translate-y-1/2 text-gray-500 hover:text-primary-dark"
              >
                {showConfirmPassword ? <EyeOff size={16} /> : <Eye size={16} />}
              </button>
            </div>
            {errors.passwordConfirm && <span className="text-xs text-destructive">{errors.passwordConfirm.message}</span>}
          </div>
        </div>

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-primary-yellow text-primary-dark py-3 px-4 rounded-lg font-bold uppercase tracking-[0.5px] text-[0.9rem] transition-all duration-300 hover:bg-[#FFC107] hover:text-black hover:-translate-y-[2px] hover:shadow-[0_4px_12px_rgba(255,215,0,0.4)] active:translate-y-0 disabled:opacity-70 disabled:cursor-not-allowed mt-4"
        >
          {loading ? "Kayıt Yapılıyor..." : "Kayıt Ol"}
        </button>

        {/* Google ile Kayıt */}
        <div className="relative my-4">
          <div className="absolute inset-0 flex items-center">
            <div className="w-full border-t border-[#e9ecef]"></div>
          </div>
          <div className="relative flex justify-center text-sm">
            <span className="px-3 bg-white text-[#888]">veya</span>
          </div>
        </div>

        <a
          href="http://localhost:3002/api/v1/auth/google-login"
          className="w-full flex items-center justify-center gap-3 bg-white border-2 border-[#e9ecef] text-primary-dark py-3 px-4 rounded-lg font-semibold text-[0.9rem] transition-all duration-300 hover:border-[#4285F4] hover:bg-[#f8f9fa] hover:-translate-y-[1px] hover:shadow-md"
        >
          <svg width="20" height="20" viewBox="0 0 24 24">
            <path
              fill="#4285F4"
              d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
            />
            <path
              fill="#34A853"
              d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
            />
            <path
              fill="#FBBC05"
              d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
            />
            <path
              fill="#EA4335"
              d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
            />
          </svg>
          Google ile Kaydol
        </a>

        <div className="text-center mt-6 pt-6 border-t border-[#eee] text-[0.9rem] text-[#666]">
          Zaten hesabınız var mı?{" "}
          <Link
            to="/login"
            className="text-primary-dark font-bold border-b-2 border-primary-yellow pb-[1px] hover:bg-primary-yellow hover:border-transparent transition-all duration-200"
          >
            Giriş Yap
          </Link>
        </div>
      </form>
    </AuthLayout>
  );
}
