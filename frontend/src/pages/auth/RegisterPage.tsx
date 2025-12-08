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
      userName: "",
      email: "",
      phone: "",
      password: "",
      passwordConfirm: "",
      gender: "1",
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

        <div className="space-y-1">
          <label className="block text-[0.9rem] font-semibold text-primary-dark mb-1">Kullanıcı Adı</label>
          <input
            {...register("userName")}
            className={cn(
              "w-full bg-[#f8f9fa] border-2 border-[#e9ecef] px-4 py-3 rounded-lg text-primary-dark text-[0.95rem] transition-all duration-300",
              "focus:bg-white focus:border-primary-yellow focus:outline-none",
              errors.userName && "border-destructive focus:border-destructive"
            )}
          />
          {errors.userName && <span className="text-xs text-destructive">{errors.userName.message}</span>}
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

        <div className="space-y-1">
          <label className="block text-[0.9rem] font-semibold text-primary-dark mb-1">Cinsiyet</label>
          <div className="flex gap-6 p-2">
            <div className="flex items-center space-x-2 cursor-pointer">
              <input
                type="radio"
                value="1"
                id="gender-female"
                {...register("gender")}
                className="h-4 w-4 border-gray-300 text-primary-yellow focus:ring-primary-yellow accent-primary-yellow cursor-pointer"
              />
              <label htmlFor="gender-female" className="font-normal cursor-pointer">Kadın</label>
            </div>
            <div className="flex items-center space-x-2 cursor-pointer">
              <input
                type="radio"
                value="2"
                id="gender-male"
                {...register("gender")}
                className="h-4 w-4 border-gray-300 text-primary-yellow focus:ring-primary-yellow accent-primary-yellow cursor-pointer"
              />
              <label htmlFor="gender-male" className="font-normal cursor-pointer">Erkek</label>
            </div>
          </div>
          {errors.gender && <span className="text-xs text-destructive">{errors.gender.message}</span>}
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
