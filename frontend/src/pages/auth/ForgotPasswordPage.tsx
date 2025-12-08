import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Link } from "react-router-dom";
import { forgotPasswordSchema } from "@/types/auth";
import type { ForgotPasswordRequest } from "@/types/auth";
import { authService } from "@/services/auth.service";
import { cn, getErrorMessage } from "@/lib/utils";
import AuthLayout from "@/layouts/AuthLayout";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { ArrowLeft, AlertCircle, CheckCircle } from "lucide-react";


export default function ForgotPasswordPage() {
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordRequest>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: {
      email: "",
    },
  });

  const onSubmit = async (data: ForgotPasswordRequest) => {
    setLoading(true);
    setError(null);
    setSuccess(null);
    try {
      const response = await authService.forgotPassword(data);
      if (response.isSuccess) {
        setSuccess("Şifre sıfırlama bağlantısı email adresinize gönderildi.");
      } else {
        if (response.errorList && response.errorList.length > 0) {
          setError(response.errorList.join(" "));
        } else {
          setError("İşlem başarısız.");
        }
      }
    } catch (err: any) {
      setError(getErrorMessage(err));
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout title="Şifremi Unuttum">
      <div className="text-center mb-6 text-gray-600 text-sm">
        Lütfen hesabınıza kayıtlı email adresinizi giriniz. Şifre sıfırlama bağlantısı email adresinize gönderilecektir.
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {error && (
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertTitle>Hata</AlertTitle>
            <AlertDescription>
              {error}
            </AlertDescription>
          </Alert>
        )}

        {success && (
          <Alert variant="success">
            <CheckCircle className="h-4 w-4" />
            <AlertTitle>Başarılı</AlertTitle>
            <AlertDescription>
              {success}
            </AlertDescription>
          </Alert>
        )}

        <div className="space-y-1">
          <label htmlFor="email" className="block text-[0.9rem] font-semibold text-primary-dark mb-1">
            Email Adresi
          </label>
          <input
            id="email"
            type="email"
            placeholder="ornek@email.com"
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

        <button
          type="submit"
          disabled={loading}
          className="w-full bg-primary-yellow text-primary-dark py-3 px-4 rounded-lg font-bold uppercase tracking-[0.5px] text-[0.9rem] transition-all duration-300 hover:bg-[#FFC107] hover:text-black hover:-translate-y-[2px] hover:shadow-[0_4px_12px_rgba(255,215,0,0.4)] active:translate-y-0 disabled:opacity-70 disabled:cursor-not-allowed"
        >
          {loading ? "Gönderiliyor..." : "Sıfırlama Linki Gönder"}
        </button>

        <div className="text-center mt-6 pt-6 border-t border-[#eee]">
          <Link
            to="/login"
            className="inline-flex items-center text-primary-dark font-bold text-[0.9rem] hover:text-[#555] transition-colors group"
          >
            <ArrowLeft size={16} className="mr-2 group-hover:-translate-x-1 transition-transform" />
            Giriş sayfasına dön
          </Link>
        </div>
      </form>
    </AuthLayout>
  );
}
